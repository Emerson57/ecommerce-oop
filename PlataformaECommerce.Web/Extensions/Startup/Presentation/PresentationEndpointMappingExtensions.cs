namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Coordina el mapeo de endpoints de presentación del storefront y del backoffice.
/// </summary>
public static class PresentationEndpointMappingExtensions
{
    /// <summary>
    /// Mapea los endpoints de presentación del host web y sus activos estáticos asociados.
    /// </summary>
    public static IEndpointRouteBuilder MapPresentationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapPresentationStaticAssetEndpoints();
        endpoints.MapPresentationPageEndpoints();

        return endpoints;
    }
}
