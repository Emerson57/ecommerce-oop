using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using PlataformaECommerce.Web.HealthChecks;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Expone el mapeo de endpoints de salud y readiness del dominio operativo del host web.
/// </summary>
public static class OperationsHealthEndpointMappingExtensions
{
    /// <summary>
    /// Mapea los endpoints de salud requeridos para monitoreo y readiness del host.
    /// </summary>
    public static IEndpointRouteBuilder MapOperationsHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("live"),
            ResponseWriter = HealthCheckResponseWriter.WriteJsonAsync
        }).AllowAnonymous();

        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("ready"),
            ResponseWriter = HealthCheckResponseWriter.WriteJsonAsync
        }).AllowAnonymous();

        return endpoints;
    }
}
