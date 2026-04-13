using Microsoft.AspNetCore.Builder;
using PlataformaECommerce.Web.Middlewares;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Activa los headers defensivos del dominio de presentación.
/// </summary>
public static class PresentationSecurityHeadersRuntimeActivationExtensions
{
    /// <summary>
    /// Activa el middleware que emite headers HTTP defensivos para la UI web.
    /// </summary>
    public static WebApplication UsePresentationSecurityHeadersRuntime(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseMiddleware<SecurityHeadersMiddleware>();
        return app;
    }
}
