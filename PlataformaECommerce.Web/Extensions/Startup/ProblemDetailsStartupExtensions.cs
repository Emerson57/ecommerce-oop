using Microsoft.Extensions.DependencyInjection;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Centraliza la configuración de Problem Details usada por la aplicación web.
/// </summary>
public static class ProblemDetailsStartupExtensions
{
    /// <summary>
    /// Registra Problem Details con el enriquecimiento actual de trazabilidad y correlación.
    /// </summary>
    /// <param name="services">Colección de servicios a configurar.</param>
    /// <returns>La misma colección de servicios para encadenamiento fluido.</returns>
    public static IServiceCollection AddProblemDetailsHandling(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
                ProblemDetailsMetadataEnricher.Enrich(context.HttpContext, context.ProblemDetails);
        });

        return services;
    }
}
