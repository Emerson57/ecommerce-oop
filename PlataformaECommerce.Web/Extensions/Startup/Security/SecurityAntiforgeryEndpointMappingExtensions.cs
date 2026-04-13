namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Expone el mapeo de endpoints auxiliares de antiforgery del dominio de seguridad.
/// </summary>
public static class SecurityAntiforgeryEndpointMappingExtensions
{
    /// <summary>
    /// Mapea los endpoints auxiliares de antiforgery requeridos por la aplicación web.
    /// </summary>
    public static IEndpointRouteBuilder MapSecurityAntiforgeryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapAntiforgeryTokenEndpoints();
        return endpoints;
    }
}
