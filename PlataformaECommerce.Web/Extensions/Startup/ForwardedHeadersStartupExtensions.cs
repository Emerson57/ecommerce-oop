using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Centraliza la configuración segura de forwarded headers para despliegues detrás de proxies o balanceadores confiables.
/// </summary>
public static class ForwardedHeadersStartupExtensions
{
    /// <summary>
    /// Registra opciones seguras para el procesamiento de headers reenviados y valida su configuración al arranque.
    /// </summary>
    /// <param name="services">Colección de servicios a configurar.</param>
    /// <param name="configuration">Configuración raíz de la aplicación.</param>
    /// <param name="hostEnvironment">Entorno actual de ejecución.</param>
    /// <returns>La misma colección de servicios para encadenamiento fluido.</returns>
    public static IServiceCollection AddConfiguredForwardedHeaders(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(hostEnvironment);

        services
            .AddOptions<ForwardedHeadersSecurityOptions>()
            .Bind(configuration.GetSection(ForwardedHeadersSecurityOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(options => ForwardedHeadersSecurityOptionsValidator.IsValid(options, hostEnvironment), ForwardedHeadersSecurityOptionsValidator.BuildValidationMessage(hostEnvironment))
            .ValidateOnStart();

        services.AddSingleton<IConfigureOptions<ForwardedHeadersOptions>, ForwardedHeadersOptionsSetup>();

        return services;
    }

    /// <summary>
    /// Activa el middleware de forwarded headers solo cuando existe una configuración segura y explícita.
    /// </summary>
    /// <param name="app">Aplicación web a configurar.</param>
    /// <returns>La misma aplicación web para encadenamiento fluido.</returns>
    public static WebApplication UseConfiguredForwardedHeaders(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        ForwardedHeadersSecurityOptions options = app.Services.GetRequiredService<IOptions<ForwardedHeadersSecurityOptions>>().Value;
        ILogger logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(ForwardedHeadersStartupExtensions));

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
