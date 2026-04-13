using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Activa el procesamiento runtime de forwarded headers del dominio de seguridad.
/// </summary>
public static class ForwardedHeadersRuntimeActivationExtensions
{
    /// <summary>
    /// Activa el middleware de forwarded headers solo cuando existe una configuración segura y explícita.
    /// </summary>
    public static WebApplication UseForwardedHeadersRuntimeActivation(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        ForwardedHeadersSecurityOptions options = app.Services.GetRequiredService<IOptions<ForwardedHeadersSecurityOptions>>().Value;
        ILogger logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(ForwardedHeadersRuntimeActivationExtensions));

        if (!options.Enabled)
        {
            if (app.Environment.IsDevelopment())
            {
                logger.LogInformation(
                    "El procesamiento de forwarded headers está deshabilitado en {EnvironmentName}. La aplicación confiará únicamente en la conexión directa actual.",
                    app.Environment.EnvironmentName);
            }
            else
            {
                logger.LogWarning(
                    "El procesamiento de forwarded headers está deshabilitado en {EnvironmentName}. Si la aplicación se ejecuta detrás de un proxy o balanceador, configura explícitamente la sección {SectionName} antes de habilitarlo.",
                    app.Environment.EnvironmentName,
                    ForwardedHeadersSecurityOptions.SectionName);
            }

            return app;
        }

        logger.LogInformation(
            "Forwarded headers habilitado con {TrustedProxyCount} proxies confiables, {TrustedNetworkCount} redes confiables y ForwardLimit {ForwardLimit}.",
            options.TrustedProxies.Count,
            options.TrustedNetworks.Count,
            options.ForwardLimit);

        app.UseForwardedHeaders();
        return app;
    }
}
