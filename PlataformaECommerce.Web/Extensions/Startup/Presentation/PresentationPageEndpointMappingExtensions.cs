namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Expone el mapeo de páginas Razor del dominio de presentación.
/// </summary>
public static class PresentationPageEndpointMappingExtensions
{
    /// <summary>
    /// Mapea las Razor Pages del host web junto con sus activos estáticos asociados.
    /// </summary>
    public static IEndpointRouteBuilder MapPresentationPageEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapRazorPages().WithStaticAssets();
        return endpoints;
    }
}
