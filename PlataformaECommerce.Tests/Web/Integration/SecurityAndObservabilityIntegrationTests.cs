using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

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

    private sealed record RateLimitPayload(string Mensaje, int Codigo, string TraceId);
}
