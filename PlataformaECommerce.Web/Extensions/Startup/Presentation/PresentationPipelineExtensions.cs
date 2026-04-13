using Microsoft.AspNetCore.Builder;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Coordina la activación de experiencia web, headers defensivos y recursos estáticos del pipeline HTTP.
/// </summary>
public static class PresentationPipelineExtensions
{
    /// <summary>
    /// Activa localización, headers de seguridad, archivos estáticos controlados y routing de la UI web.
    /// </summary>
    public static WebApplication UsePresentationModule(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UsePresentationLocalizationRuntime();
        app.UsePresentationSecurityHeadersRuntime();
        app.UsePresentationStaticFilesRuntime();
        app.UsePresentationRoutingRuntime();

        return app;
    }
}
