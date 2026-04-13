using Microsoft.AspNetCore.Builder;
namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Agrupa el mapeo de endpoints del host web por dominios de operaciones, seguridad, presentación y plataforma.
/// </summary>
public static class EndpointMappingExtensions
{
    /// <summary>
    /// Mapea los endpoints HTTP configurados por la aplicación web coordinando las superficies por dominio.
    /// </summary>
    /// <param name="app">Aplicación web a configurar.</param>
    /// <returns>La misma aplicación web para encadenamiento fluido.</returns>
    public static WebApplication MapApplicationEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapOperationsEndpoints();
        app.MapSecurityEndpoints();
        app.MapPresentationEndpoints();
        app.MapPlatformEndpoints();

        return app;
    }

}
