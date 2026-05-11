using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PlataformaECommerce.Tests.Web.Integration;

[TestFixture]
public class SecurityAndObservabilityIntegrationTests
{
    private TestWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new TestWebApplicationFactory();
        _client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task HealthLive_PropagaCorrelationIdYHeadersDeSeguridad()
    {
        HttpRequestMessage request = new(HttpMethod.Get, "/health/live");
        request.Headers.Add("X-Correlation-ID", "integration-correlation-001");

        HttpResponseMessage response = await _client.SendAsync(request, CancellationToken.None);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Headers.TryGetValues("X-Correlation-ID", out IEnumerable<string>? values), Is.True);
        Assert.That(values?.Single(), Is.EqualTo("integration-correlation-001"));
        Assert.That(response.Headers.Contains("X-Content-Type-Options"), Is.True);
        Assert.That(response.Headers.Contains("Content-Security-Policy"), Is.True);
        string contentSecurityPolicy = response.Headers.GetValues("Content-Security-Policy").Single();
        Assert.That(contentSecurityPolicy, Does.Not.Contain("'unsafe-inline'"));
        Assert.That(contentSecurityPolicy, Does.Contain("script-src 'self'"));
        Assert.That(contentSecurityPolicy, Does.Contain("style-src 'self' 'nonce-"));
        Assert.That(contentSecurityPolicy, Does.Contain("font-src 'self' data:"));
        Assert.That(contentSecurityPolicy, Does.Not.Contain("fonts.googleapis.com"));
        Assert.That(contentSecurityPolicy, Does.Not.Contain("fonts.gstatic.com"));
    }

    [Test]
    public async Task HomePage_EmiteNonceDeEstiloParaBrandingDinamico()
    {
        HttpResponseMessage response = await _client.GetAsync("/", CancellationToken.None);

        string html = await response.Content.ReadAsStringAsync(CancellationToken.None);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(html, Does.Contain("<style nonce=\""));
        Assert.That(html, Does.Contain("/css/fonts"));
        Assert.That(html, Does.Not.Contain("fonts.googleapis.com"));
    }

    [Test]
    public async Task AdminUsers_SinAutenticacion_RedireccionaALogin()
    {
        HttpResponseMessage response = await _client.GetAsync("/Admin/Users", CancellationToken.None);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
        Assert.That(response.Headers.Location?.OriginalString, Does.Contain("/Auth/Login"));
    }

    [Test]
    public async Task AntiforgeryTokenEndpoint_SinAutenticacion_RedireccionaALogin()
    {
        HttpResponseMessage response = await _client.GetAsync("/security/antiforgery/token", CancellationToken.None);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
        Assert.That(response.Headers.Location?.OriginalString, Does.Contain("/Auth/Login"));
    }

    [Test]
    public async Task Login_PostSinTokenAntiforgery_RetornaBadRequest()
    {
        using FormUrlEncodedContent content = new(
        [
            new KeyValuePair<string, string>("Input.Email", "root@tenant-demo.example"),
            new KeyValuePair<string, string>("Input.Password", "Password#2026"),
            new KeyValuePair<string, string>("Input.RememberMe", bool.FalseString)
        ]);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

        HttpResponseMessage response = await _client.PostAsync("/Auth/Login", content, CancellationToken.None);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Login_ExcedeRateLimiting_RetornaTooManyRequests()
    {
        HttpResponseMessage? lastResponse = null;

        for (int index = 0; index < 11; index++)
        {
            lastResponse = await _client.GetAsync("/Auth/Login", CancellationToken.None);
        }

        Assert.That(lastResponse, Is.Not.Null);
        Assert.That(lastResponse!.StatusCode, Is.EqualTo((HttpStatusCode)429));
        Assert.That(lastResponse.Headers.Contains("Retry-After"), Is.True);

        var payload = await lastResponse.Content.ReadFromJsonAsync<RateLimitPayload>(cancellationToken: CancellationToken.None);
        Assert.That(payload?.Codigo, Is.EqualTo(429));
        Assert.That(payload?.Mensaje, Does.Contain("límite temporal"));
    }

    [Test]
    public async Task SwaggerUi_Development_EstaDisponible()
    {
        HttpResponseMessage response = await _client.GetAsync("/swagger/index.html", CancellationToken.None);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task SwaggerUi_Staging_SinAutenticacion_RedireccionaALogin()
    {
        using EnvironmentVariableScope _ = CreateProductionSecretsEnvironmentScope();
        using WebApplicationFactory<Program> factory = CreateFactoryWithOpenApiSecurityEnvironment("Staging");
        using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        HttpResponseMessage response = await client.GetAsync("/swagger/index.html", CancellationToken.None);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
        Assert.That(response.Headers.Location?.OriginalString, Does.Contain("/Auth/Login"));
    }

    [Test]
    public async Task OpenApiJson_Staging_SinAutenticacion_Retorna401()
    {
        using EnvironmentVariableScope _ = CreateProductionSecretsEnvironmentScope();
        using WebApplicationFactory<Program> factory = CreateFactoryWithOpenApiSecurityEnvironment("Staging");
        using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        HttpResponseMessage response = await client.GetAsync("/swagger/public/swagger.json", CancellationToken.None);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Uploads_PublicosPermitidos_SeSirvenConTipoMimeRestringido()
    {
        string uploadsDirectory = $"uploads/integration-tests/{Guid.NewGuid():N}";
        string fileName = "producto.webp";
        string physicalDirectory = CreateUploadsDirectory(uploadsDirectory);
        await File.WriteAllBytesAsync(Path.Combine(physicalDirectory, fileName), [1, 2, 3, 4], CancellationToken.None);

        using WebApplicationFactory<Program> factory = CreateFactoryWithUploadsConfiguration(uploadsDirectory, "/integration-uploads");
        using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        HttpResponseMessage response = await client.GetAsync($"/integration-uploads/{fileName}", CancellationToken.None);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("image/webp"));
        Assert.That(response.Headers.TryGetValues("X-Content-Type-Options", out IEnumerable<string>? values), Is.True);
        Assert.That(values?.Single(), Is.EqualTo("nosniff"));
    }

    [Test]
    public async Task Uploads_ConExtensionNoPermitida_NoSeSirvenPublicamente()
    {
        string uploadsDirectory = $"uploads/integration-tests/{Guid.NewGuid():N}";
        string fileName = "producto.txt";
        string physicalDirectory = CreateUploadsDirectory(uploadsDirectory);
        await File.WriteAllTextAsync(Path.Combine(physicalDirectory, fileName), "contenido de prueba", CancellationToken.None);

        using WebApplicationFactory<Program> factory = CreateFactoryWithUploadsConfiguration(uploadsDirectory, "/integration-uploads");
        using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        HttpResponseMessage response = await client.GetAsync($"/integration-uploads/{fileName}", CancellationToken.None);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    private WebApplicationFactory<Program> CreateFactoryWithUploadsConfiguration(string uploadsDirectory, string requestPath)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Backoffice:ProductImages:UploadsDirectory"] = uploadsDirectory,
                    ["Backoffice:ProductImages:RequestPath"] = requestPath,
                    ["Backoffice:ProductImages:AllowedExtensions:0"] = ".jpg",
                    ["Backoffice:ProductImages:AllowedExtensions:1"] = ".jpeg",
                    ["Backoffice:ProductImages:AllowedExtensions:2"] = ".png",
                    ["Backoffice:ProductImages:AllowedExtensions:3"] = ".webp",
                    ["Backoffice:ProductImages:AllowedContentTypes:0"] = "image/jpeg",
                    ["Backoffice:ProductImages:AllowedContentTypes:1"] = "image/png",
                    ["Backoffice:ProductImages:AllowedContentTypes:2"] = "image/webp"
                });
            });
        });
    }

    private WebApplicationFactory<Program> CreateFactoryWithOpenApiSecurityEnvironment(string environmentName)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(environmentName);
            builder.ConfigureAppConfiguration((_, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = "Server=localhost,1433;Database=PlataformaECommerceTests;User Id=sa;Password=TestOnly_LocalSql_Password_NotForProduction!;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=True;",
                    ["Jwt:Issuer"] = "PlataformaECommerce.Tests",
                    ["Jwt:Audience"] = "PlataformaECommerce.Tests.Clients",
                    ["Jwt:SigningKey"] = "IntegrationTestsSigningKey_With32Chars!",
                    ["DataProtection:ApplicationName"] = "PlataformaECommerce.Tests",
                    ["Notifications:Smtp:Host"] = "smtp.tests.local",
                    ["Notifications:Smtp:UserName"] = "smtp-user",
                    ["Notifications:Smtp:Password"] = "smtp-password",
                    ["Notifications:Smtp:FromAddress"] = "noreply@tests.local",
                    ["Payments:Wompi:PublicKey"] = "pub_test_123",
                    ["Payments:Wompi:IntegritySecret"] = "int_test_456",
                    ["OpenApiSecurity:EnabledInQualityAssurance"] = bool.TrueString,
                    ["OpenApiSecurity:RequireAuthorizationOutsideDevelopment"] = bool.TrueString,
                    ["OpenApiSecurity:RequiredPolicy"] = "SuperUserOnly"
                });
            });
            builder.ConfigureServices(services =>
            {
                ServiceDescriptor[] startupTasksToRemove = services
                    .Where(descriptor =>
                        string.Equals(descriptor.ImplementationType?.Name, "InfrastructureVerificationStartupTask", StringComparison.Ordinal)
                        || string.Equals(descriptor.ImplementationType?.Name, "TenantCatalogWarmupStartupTask", StringComparison.Ordinal))
                    .ToArray();

                foreach (ServiceDescriptor startupTaskDescriptor in startupTasksToRemove)
                {
                    services.Remove(startupTaskDescriptor);
                }
            });
        });
    }

    private static string CreateUploadsDirectory(string uploadsDirectory)
    {
        string webProjectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "PlataformaECommerce.Web"));
        string physicalDirectory = Path.Combine(webProjectRoot, "wwwroot", uploadsDirectory.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(physicalDirectory);
        return physicalDirectory;
    }

    private static EnvironmentVariableScope CreateProductionSecretsEnvironmentScope()
    {
        return new EnvironmentVariableScope(new Dictionary<string, string?>
        {
            ["ConnectionStrings__DefaultConnection"] = "Server=localhost,1433;Database=PlataformaECommerceTests;User Id=sa;Password=TestOnly_LocalSql_Password_NotForProduction!;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=True;",
            ["Jwt__SigningKey"] = "IntegrationTestsSigningKey_With32Chars!",
            ["Payments__Wompi__PublicKey"] = "pub_test_123",
            ["Payments__Wompi__IntegritySecret"] = "int_test_456",
            ["Notifications__Smtp__Host"] = "smtp.tests.local",
            ["Notifications__Smtp__UserName"] = "smtp-user",
            ["Notifications__Smtp__Password"] = "smtp-password",
            ["Notifications__Smtp__FromAddress"] = "noreply@tests.local"
        });
    }

    private sealed class EnvironmentVariableScope : IDisposable
    {
        private readonly IReadOnlyDictionary<string, string?> _originalValues;

        public EnvironmentVariableScope(IReadOnlyDictionary<string, string?> variables)
        {
            ArgumentNullException.ThrowIfNull(variables);

            Dictionary<string, string?> originalValues = new(StringComparer.Ordinal);
            foreach ((string key, string? value) in variables)
            {
                originalValues[key] = Environment.GetEnvironmentVariable(key);
                Environment.SetEnvironmentVariable(key, value);
            }

            _originalValues = originalValues;
        }

        public void Dispose()
        {
            foreach ((string key, string? value) in _originalValues)
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }

    private sealed record RateLimitPayload(string Mensaje, int Codigo, string TraceId);
}
