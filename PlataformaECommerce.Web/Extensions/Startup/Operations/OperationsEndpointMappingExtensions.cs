namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Coordina el mapeo de endpoints operativos del host web.
/// </summary>
public static class OperationsEndpointMappingExtensions
{
    /// <summary>
    /// Mapea los endpoints de salud y operación necesarios para monitoreo y readiness del host.
    /// </summary>
    public static IEndpointRouteBuilder MapOperationsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapOperationsHealthEndpoints();

        return endpoints;
    }
}
