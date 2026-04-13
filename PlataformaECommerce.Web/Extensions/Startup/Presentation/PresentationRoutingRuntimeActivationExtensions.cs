using Microsoft.AspNetCore.Builder;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Activa el routing runtime del dominio de presentación.
/// </summary>
public static class PresentationRoutingRuntimeActivationExtensions
{
    /// <summary>
    /// Activa el sistema de routing necesario para la UI web y sus endpoints asociados.
    /// </summary>
    public static WebApplication UsePresentationRoutingRuntime(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseRouting();
        return app;
    }
}
