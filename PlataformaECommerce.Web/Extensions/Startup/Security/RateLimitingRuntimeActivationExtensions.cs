using Microsoft.AspNetCore.Builder;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Activa las piezas runtime de rate limiting del dominio de seguridad.
/// </summary>
public static class RateLimitingRuntimeActivationExtensions
{
    /// <summary>
    /// Activa el middleware de rate limiting para las superficies protegidas del host.
    /// </summary>
    public static WebApplication UseSecurityRateLimitingRuntime(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseRateLimiter();
        return app;
    }
}
