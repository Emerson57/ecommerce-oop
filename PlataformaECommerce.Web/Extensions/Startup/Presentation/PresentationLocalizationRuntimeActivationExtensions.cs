using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Activa la localización runtime del dominio de presentación.
/// </summary>
public static class PresentationLocalizationRuntimeActivationExtensions
{
    /// <summary>
    /// Activa la cultura y localización configuradas para la UI web.
    /// </summary>
    public static WebApplication UsePresentationLocalizationRuntime(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        RequestLocalizationOptions requestLocalizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
        app.UseRequestLocalization(requestLocalizationOptions);

        return app;
    }
}
