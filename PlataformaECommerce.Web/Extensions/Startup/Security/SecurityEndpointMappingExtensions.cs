namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Coordina el mapeo de endpoints auxiliares del dominio de seguridad del host web.
/// </summary>
public static class SecurityEndpointMappingExtensions
{
    /// <summary>
    /// Mapea los endpoints auxiliares de seguridad requeridos por la aplicación web.
    /// </summary>
    public static IEndpointRouteBuilder MapSecurityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapSecurityAntiforgeryEndpoints();
        return endpoints;
    }
}
