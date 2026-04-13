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
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.Response.OnStarting(static state =>
        {
            (HttpContext httpContext, WebSecurityHeadersOptions options, IWebHostEnvironment environment) = ((HttpContext, WebSecurityHeadersOptions, IWebHostEnvironment))state;
            ApplyHeaders(httpContext, options, environment);
            return Task.CompletedTask;
        }, (context, _options, _environment));

        await _next(context).ConfigureAwait(false);

        if (!context.Response.HasStarted)
        {
            ApplyHeaders(context, _options, _environment);
        }
    }

    private static void ApplyHeaders(HttpContext context, WebSecurityHeadersOptions options, IWebHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(environment);

        IHeaderDictionary headers = context.Response.Headers;

        if (options.ContentSecurityPolicy.Enabled)
        {
            string nonce = ContentSecurityPolicyNonceAccessor.GetOrCreateNonce(context);
            string contentSecurityPolicy = ContentSecurityPolicyBuilder.Build(options.ContentSecurityPolicy, environment, nonce);
            string contentSecurityPolicyHeaderName = options.ContentSecurityPolicy.UseReportOnlyInDevelopment && environment.IsDevelopment()
                ? "Content-Security-Policy-Report-Only"
                : "Content-Security-Policy";

            headers[contentSecurityPolicyHeaderName] = contentSecurityPolicy;
        }

        headers["Permissions-Policy"] = options.PermissionsPolicy;
        headers["Referrer-Policy"] = options.ReferrerPolicy;
        headers["X-Frame-Options"] = options.FrameOptions;
        headers["X-Content-Type-Options"] = options.ContentTypeOptions;
        headers["Cross-Origin-Opener-Policy"] = options.CrossOriginOpenerPolicy;
        headers["Cross-Origin-Resource-Policy"] = options.CrossOriginResourcePolicy;
        headers["X-Permitted-Cross-Domain-Policies"] = "none";
        headers["Origin-Agent-Cluster"] = "?1";
        headers.Remove("Server");
    }
}
