using Microsoft.AspNetCore.Builder;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Coordina la activación del runtime de seguridad del host web.
/// </summary>
public static class SecurityPipelineExtensions
{
    /// <summary>
    /// Activa la capa perimetral de transporte seguro una vez normalizado el contexto de proxy.
    /// </summary>
    public static WebApplication UseSecurityPerimeterModule(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseSecurityTransportRuntime();

        return app;
    }

    /// <summary>
    /// Activa autenticación, limitación de tráfico y autorización para las superficies protegidas.
    /// </summary>
    public static WebApplication UseSecurityAccessControlModule(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseSecurityAccessControlRuntime();

        return app;
    }
}
