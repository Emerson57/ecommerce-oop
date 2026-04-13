using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
    public static IServiceCollection AddForwardedHeadersSupport(
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

}
