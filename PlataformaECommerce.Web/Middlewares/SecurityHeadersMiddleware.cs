using Microsoft.Extensions.Options;
using PlataformaECommerce.Web.Configuration;
using PlataformaECommerce.Web.Security;

namespace PlataformaECommerce.Web.Middlewares;

/// <summary>
/// Emite headers HTTP defensivos para reducir superficie de ataque del front web.
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly WebSecurityHeadersOptions _options;
    private readonly IWebHostEnvironment _environment;

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
        _environment = environment;
    }

    /// <summary>
    /// Ejecuta el middleware y agrega headers antes de enviar la respuesta.
    /// </summary>
    public Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        IHeaderDictionary headers = context.Response.Headers;

        if (_options.ContentSecurityPolicy.Enabled)
        {
            string nonce = ContentSecurityPolicyNonceAccessor.GetOrCreateNonce(context);
            string contentSecurityPolicy = ContentSecurityPolicyBuilder.Build(_options.ContentSecurityPolicy, _environment, nonce);
            string contentSecurityPolicyHeaderName = _options.ContentSecurityPolicy.UseReportOnlyInDevelopment && _environment.IsDevelopment()
                ? "Content-Security-Policy-Report-Only"
                : "Content-Security-Policy";

            headers[contentSecurityPolicyHeaderName] = contentSecurityPolicy;
        }

        headers["Permissions-Policy"] = _options.PermissionsPolicy;
        headers["Referrer-Policy"] = _options.ReferrerPolicy;
        headers["X-Frame-Options"] = _options.FrameOptions;
        headers["X-Content-Type-Options"] = _options.ContentTypeOptions;
        headers["Cross-Origin-Opener-Policy"] = _options.CrossOriginOpenerPolicy;
        headers["Cross-Origin-Resource-Policy"] = _options.CrossOriginResourcePolicy;
        headers["X-Permitted-Cross-Domain-Policies"] = "none";

        return _next(context);
    }
}
