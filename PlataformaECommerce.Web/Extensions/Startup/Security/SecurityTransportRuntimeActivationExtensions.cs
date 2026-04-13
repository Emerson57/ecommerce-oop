using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Agrupa la activación del perímetro de transporte seguro posterior a la normalización de proxy del dominio de seguridad.
/// </summary>
public static class SecurityTransportRuntimeActivationExtensions
{
    /// <summary>
    /// Activa HSTS y redirección HTTPS una vez resueltos los headers reenviados confiables.
    /// </summary>
    public static WebApplication UseSecurityTransportRuntime(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        WebTransportSecurityOptions transportSecurityOptions = app.Services
            .GetRequiredService<IOptions<WebTransportSecurityOptions>>()
            .Value;

        if (!app.Environment.IsDevelopment() && transportSecurityOptions.HstsEnabled)
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        return app;
    }
}
