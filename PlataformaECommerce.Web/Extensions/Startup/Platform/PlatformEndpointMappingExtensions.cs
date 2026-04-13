namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Expone el mapeo de endpoints de plataforma compartidos por la aplicación web.
/// </summary>
public static class PlatformEndpointMappingExtensions
{
    /// <summary>
    /// Mapea la superficie HTTP de plataforma expuesta mediante controladores.
    /// </summary>
    public static IEndpointRouteBuilder MapPlatformEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapControllers();
        return endpoints;
    }
}
