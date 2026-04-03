using System.Globalization;
using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using PlataformaECommerce.Application.DependencyInjection;
using PlataformaECommerce.Infrastructure.DependencyInjection;
using PlataformaECommerce.Infrastructure.Mongo;
using PlataformaECommerce.Infrastructure.Persistence.Context;
using PlataformaECommerce.Web.Authorization;
using PlataformaECommerce.Web.Configuration;
using PlataformaECommerce.Web.HealthChecks;
using PlataformaECommerce.Web.Middlewares;
using PlataformaECommerce.Web.OpenApi;
using PlataformaECommerce.Web.Services.Products;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "PlataformaECommerce.Web");
});

if (builder.Environment.IsDevelopment())
{
    builder.Configuration
        .AddUserSecrets<Program>(optional: true, reloadOnChange: true)
        .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.local.json", optional: true, reloadOnChange: true);
}

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", AuthorizationPolicies.AdminOnly);
    options.Conventions.AuthorizeFolder("/Admin/Users", AuthorizationPolicies.SuperUserOnly);
});

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errores = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            var respuesta = new
            {
                mensaje = "La solicitud contiene errores de validación.",
                errores
            };

            return new BadRequestObjectResult(respuesta);
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(SwaggerGroups.Public, new OpenApiInfo
    {
        Title = "PlataformaECommerce API Pública",
        Version = "v1",
        Description = "Endpoints públicos de consulta del catálogo de productos."
    });

    options.SwaggerDoc(SwaggerGroups.Admin, new OpenApiInfo
    {
        Title = "PlataformaECommerce API Administrativa",
        Version = "v1",
        Description = "Endpoints administrativos protegidos para gestión integral del catálogo."
    });

    options.DocInclusionPredicate((documentName, apiDescription) =>
    {
        string? groupName = apiDescription.GroupName;
        return string.Equals(groupName, documentName, StringComparison.OrdinalIgnoreCase);
    });
});

builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "__Host-PlataformaECommerce.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.SuppressXFrameOptionsHeader = true;
});

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

builder.Services
    .AddOptions<AdminUsersBackofficeOptions>()
    .Bind(builder.Configuration.GetSection(AdminUsersBackofficeOptions.SectionName));

builder.Services
    .AddOptions<ProductImagesOptions>()
    .Bind(builder.Configuration.GetSection(ProductImagesOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.UploadsDirectory), "La configuración de imágenes de productos requiere un directorio de almacenamiento válido.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.RequestPath) && options.RequestPath.StartsWith('/'), "La configuración de imágenes de productos requiere una ruta pública válida que comience con '/'.")
    .Validate(options => options.MaxFileSizeInBytes > 0, "La configuración de imágenes de productos requiere un tamaño máximo de archivo mayor que cero.")
    .Validate(options => options.AllowedExtensions.Count > 0, "La configuración de imágenes de productos requiere al menos una extensión permitida.")
    .ValidateOnStart();

builder.Services
    .AddOptions<WebSecurityHeadersOptions>()
    .Bind(builder.Configuration.GetSection(WebSecurityHeadersOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(options => !string.IsNullOrWhiteSpace(options.ContentSecurityPolicy), "La configuración de headers de seguridad requiere una política CSP válida.")
    .ValidateOnStart();

builder.Services
    .AddOptions<WebRateLimitingOptions>()
    .Bind(builder.Configuration.GetSection(WebRateLimitingOptions.SectionName))
    .Validate(options => AreValidRateLimitingOptions(options), "La configuración de rate limiting contiene valores inválidos.")
    .ValidateOnStart();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

IHealthChecksBuilder healthChecksBuilder = builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("La aplicación web se encuentra operativa."), tags: ["live"])
    .AddDbContextCheck<ECommerceDbContext>(name: "sql-server", tags: ["ready"]);

MongoDbSettings mongoDbSettings = builder.Configuration.GetSection(MongoDbSettings.SectionName).Get<MongoDbSettings>() ?? new MongoDbSettings();
if (mongoDbSettings.Enabled)
{
    healthChecksBuilder.AddCheck<MongoDbHealthCheck>("mongo-audit", tags: ["ready"]);
}

WebRateLimitingOptions configuredRateLimitingOptions = builder.Configuration
    .GetSection(WebRateLimitingOptions.SectionName)
    .Get<WebRateLimitingOptions>()
    ?? new WebRateLimitingOptions();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = static async (context, cancellationToken) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = Math.Ceiling(retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        context.HttpContext.Response.ContentType = "application/json";

        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            mensaje = "Se alcanzó el límite temporal de solicitudes para este recurso.",
            codigo = StatusCodes.Status429TooManyRequests,
            traceId = context.HttpContext.TraceIdentifier
        }, cancellationToken: cancellationToken);
    };

    AddFixedWindowPolicy(options, WebRateLimitingOptions.AuthFlowPolicyName, configuredRateLimitingOptions.AuthFlow);
    AddFixedWindowPolicy(options, WebRateLimitingOptions.SensitiveApiPolicyName, configuredRateLimitingOptions.SensitiveApi);
    AddFixedWindowPolicy(options, WebRateLimitingOptions.PublicApiPolicyName, configuredRateLimitingOptions.PublicApi);
});

builder.Services.AddScoped<AdminCookieSecurityService>();
builder.Services.AddScoped<AdminCookieAuthenticationEvents>();
builder.Services.AddScoped<CustomerCookieSecurityService>();
builder.Services.AddScoped<CustomerCookieAuthenticationEvents>();
builder.Services.AddScoped<IProductImageStorageService, ProductImageStorageService>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = AuthorizationPolicies.AppCookieScheme;
        options.DefaultAuthenticateScheme = AuthorizationPolicies.AppCookieScheme;
        options.DefaultChallengeScheme = AuthorizationPolicies.AppCookieScheme;
        options.DefaultSignOutScheme = AuthorizationPolicies.AppCookieScheme;
    })
    .AddPolicyScheme(AuthorizationPolicies.AppCookieScheme, "Application cookie selector", options =>
    {
        options.ForwardDefaultSelector = AuthorizationPolicies.ResolveApplicationCookieScheme;
    })
    .AddCookie(AuthorizationPolicies.AdminCookieScheme, AuthorizationPolicies.ConfigureAdminCookie)
    .AddCookie(AuthorizationPolicies.CustomerCookieScheme, AuthorizationPolicies.ConfigureCustomerCookie);

builder.Services.AddAuthorization(AuthorizationPolicies.ConfigureBackofficePolicies);

CultureInfo[] supportedCultures =
[
    CultureInfo.GetCultureInfo("es-CO"),
    CultureInfo.GetCultureInfo("es"),
    CultureInfo.GetCultureInfo("en-US")
];

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("es-CO");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.ApplyCurrentCultureToResponseHeaders = true;
    options.FallBackToParentCultures = true;
    options.FallBackToParentUICultures = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint($"/swagger/{SwaggerGroups.Public}/swagger.json", "API Pública v1");
        options.SwaggerEndpoint($"/swagger/{SwaggerGroups.Admin}/swagger.json", "API Administrativa v1");
        options.DocumentTitle = "PlataformaECommerce Swagger";
    });
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = static (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("TraceIdentifier", httpContext.TraceIdentifier);
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("RemoteIp", httpContext.Connection.RemoteIpAddress?.ToString());
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
        diagnosticContext.Set("EndpointName", httpContext.GetEndpoint()?.DisplayName);
    };
});

RequestLocalizationOptions requestLocalizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(requestLocalizationOptions);
app.UseMiddleware<SecurityHeadersMiddleware>();

ProductImagesOptions productImagesOptions = app.Services.GetRequiredService<IOptions<ProductImagesOptions>>().Value;
string webRootPath = string.IsNullOrWhiteSpace(app.Environment.WebRootPath)
    ? Path.Combine(app.Environment.ContentRootPath, "wwwroot")
    : app.Environment.WebRootPath;
string productImagesPhysicalPath = Path.Combine(webRootPath, productImagesOptions.UploadsDirectory.Replace('/', Path.DirectorySeparatorChar));
Directory.CreateDirectory(productImagesPhysicalPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(productImagesPhysicalPath),
    RequestPath = productImagesOptions.RequestPath
});

app.UseRouting();
app.UseRateLimiter();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("live"),
    ResponseWriter = HealthCheckResponseWriter.WriteJsonAsync
}).AllowAnonymous();

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready"),
    ResponseWriter = HealthCheckResponseWriter.WriteJsonAsync
}).AllowAnonymous();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();
app.MapControllers();

app.Run();

static bool AreValidRateLimitingOptions(WebRateLimitingOptions options)
{
    ArgumentNullException.ThrowIfNull(options);

    return IsValidPolicy(options.AuthFlow)
        && IsValidPolicy(options.SensitiveApi)
        && IsValidPolicy(options.PublicApi);
}

static bool IsValidPolicy(WebRateLimitingOptions.FixedWindowPolicyOptions? options)
{
    return options is not null
        && options.PermitLimit > 0
        && options.WindowSeconds > 0
        && options.QueueLimit >= 0;
}

static void AddFixedWindowPolicy(
    Microsoft.AspNetCore.RateLimiting.RateLimiterOptions options,
    string policyName,
    WebRateLimitingOptions.FixedWindowPolicyOptions policy)
{
    ArgumentNullException.ThrowIfNull(options);
    ArgumentNullException.ThrowIfNull(policy);

    options.AddPolicy(policyName, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ResolveRateLimitPartitionKey(httpContext),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = policy.PermitLimit,
                Window = TimeSpan.FromSeconds(policy.WindowSeconds),
                QueueLimit = policy.QueueLimit,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            }));
}

static string ResolveRateLimitPartitionKey(HttpContext httpContext)
{
    ArgumentNullException.ThrowIfNull(httpContext);

    string routeBase = httpContext.Request.Path.HasValue
        ? httpContext.Request.Path.Value!
        : "unknown";

    string identity = httpContext.User.Identity?.IsAuthenticated == true
        ? httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous"
        : httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

    return $"{routeBase}:{identity}";
}
