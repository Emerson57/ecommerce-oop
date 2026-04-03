using Microsoft.Extensions.Options;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Web.Middlewares;

/// <summary>
/// Emite headers HTTP defensivos para reducir superficie de ataque del front web.
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly WebSecurityHeadersOptions _options;
    private readonly string _contentSecurityPolicy;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="SecurityHeadersMiddleware"/>.
    /// </summary>
    public SecurityHeadersMiddleware(
        RequestDelegate next,
        IOptions<WebSecurityHeadersOptions> options,
        IWebHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(environment);

        _next = next ?? throw new ArgumentNullException(nameof(next));
        _options = options.Value;
        _contentSecurityPolicy = BuildContentSecurityPolicy(_options, environment);
    }

    /// <summary>
    /// Ejecuta el middleware y agrega headers antes de enviar la respuesta.
    /// </summary>
    public Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        IHeaderDictionary headers = context.Response.Headers;
        headers["Content-Security-Policy"] = _contentSecurityPolicy;
        headers["Permissions-Policy"] = _options.PermissionsPolicy;
        headers["Referrer-Policy"] = _options.ReferrerPolicy;
        headers["X-Frame-Options"] = _options.FrameOptions;
        headers["X-Content-Type-Options"] = _options.ContentTypeOptions;
        headers["Cross-Origin-Opener-Policy"] = _options.CrossOriginOpenerPolicy;
        headers["Cross-Origin-Resource-Policy"] = _options.CrossOriginResourcePolicy;
        headers["X-Permitted-Cross-Domain-Policies"] = "none";

        return _next(context);
    }

    private static string BuildContentSecurityPolicy(WebSecurityHeadersOptions options, IWebHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(environment);

        string policy = options.ContentSecurityPolicy.Trim();

        if (options.IncludeUpgradeInsecureRequests && !environment.IsDevelopment() && !policy.Contains("upgrade-insecure-requests", StringComparison.OrdinalIgnoreCase))
        {
            return $"{policy}; upgrade-insecure-requests";
        }

        return policy;
    }
}
