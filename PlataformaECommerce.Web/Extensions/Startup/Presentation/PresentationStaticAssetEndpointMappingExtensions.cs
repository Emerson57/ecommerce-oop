namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Expone el mapeo de activos estáticos del dominio de presentación.
/// </summary>
public static class PresentationStaticAssetEndpointMappingExtensions
{
    /// <summary>
    /// Mapea los endpoints de activos estáticos del host web para la experiencia de presentación.
    /// </summary>
    public static IEndpointRouteBuilder MapPresentationStaticAssetEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapStaticAssets();
        return endpoints;
    }
}
