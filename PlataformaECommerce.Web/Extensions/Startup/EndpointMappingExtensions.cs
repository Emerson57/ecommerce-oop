using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using PlataformaECommerce.Web.HealthChecks;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Agrupa el mapeo de endpoints del host web preservando la composición actual.
/// </summary>
public static class EndpointMappingExtensions
{
    /// <summary>
    /// Mapea los endpoints HTTP configurados por la aplicación web.
    /// </summary>
    /// <param name="app">Aplicación web a configurar.</param>
    /// <returns>La misma aplicación web para encadenamiento fluido.</returns>
    public static WebApplication MapConfiguredEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("live"),
            ResponseWriter = HealthCheckResponseWriter.WriteJsonAsync
        }).AllowAnonymous();

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("ready"),
            ResponseWriter = HealthCheckResponseWriter.WriteJsonAsync
        }).AllowAnonymous();

        app.MapStaticAssets();
        app.MapRazorPages()
            .WithStaticAssets();
        app.MapControllers();

        return app;
    }
}
