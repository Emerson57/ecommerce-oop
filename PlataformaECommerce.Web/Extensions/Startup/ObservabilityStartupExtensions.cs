using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Centraliza la configuración de observabilidad HTTP y trazabilidad operativa de la capa web.
/// </summary>
public static class ObservabilityStartupExtensions
{
    /// <summary>
    /// Registra opciones tipadas de observabilidad requeridas por el pipeline web.
    /// </summary>
    public static IServiceCollection AddObservabilityConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<RequestCorrelationOptions>()
            .Bind(configuration.GetSection(RequestCorrelationOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(options => !string.IsNullOrWhiteSpace(options.CorrelationHeaderName), "La configuración de observabilidad requiere un header de correlación válido.")
            .ValidateOnStart();

        return services;
    }
}
