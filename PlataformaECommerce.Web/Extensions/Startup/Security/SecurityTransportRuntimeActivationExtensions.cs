using Microsoft.AspNetCore.Builder;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Agrupa la activación del perímetro de transporte seguro del dominio de seguridad.
/// </summary>
public static class SecurityTransportRuntimeActivationExtensions
{
    /// <summary>
    /// Activa forwarded headers, HSTS y redirección HTTPS como perímetro de seguridad del host.
    /// </summary>
    public static WebApplication UseSecurityTransportRuntime(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseForwardedHeadersRuntimeActivation();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        return app;
    }
}
