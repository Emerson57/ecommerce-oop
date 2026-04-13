using Microsoft.AspNetCore.Builder;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Activa la exposición runtime de archivos estáticos del dominio de presentación.
/// </summary>
public static class PresentationStaticFilesRuntimeActivationExtensions
{
    /// <summary>
    /// Activa la exposición controlada de uploads y otros archivos estáticos de presentación.
    /// </summary>
    public static WebApplication UsePresentationStaticFilesRuntime(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseUploadStaticFiles();
        return app;
    }
}
