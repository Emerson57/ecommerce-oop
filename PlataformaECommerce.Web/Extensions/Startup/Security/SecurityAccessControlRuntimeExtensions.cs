using Microsoft.AspNetCore.Builder;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Agrupa la activación del control de acceso del dominio de seguridad.
/// </summary>
public static class SecurityAccessControlRuntimeExtensions
{
    /// <summary>
    /// Activa autenticación, rate limiting y autorización para las superficies protegidas.
    /// </summary>
    public static WebApplication UseSecurityAccessControlRuntime(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseAuthentication();
        app.UseSecurityRateLimitingRuntime();
        app.UseAuthorization();

        return app;
    }
}
