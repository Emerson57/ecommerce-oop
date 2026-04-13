using Microsoft.AspNetCore.Builder;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Centraliza la construcción del pipeline HTTP en fases explícitas de diagnóstico, seguridad, routing y autorización.
/// </summary>
public static class PipelineExtensions
{
    /// <summary>
    /// Configura el pipeline HTTP completo de la aplicación web.
    /// </summary>
    /// <param name="app">Aplicación web a configurar.</param>
    /// <returns>La misma aplicación web para encadenamiento fluido.</returns>
    public static WebApplication UseApplicationRequestPipeline(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseSecurityPerimeterModule();
        app.UseObservabilityModule();
        app.UseOperationsModule();
        app.UsePresentationModule();
        app.UseSecurityAccessControlModule();

        return app;
    }
}
