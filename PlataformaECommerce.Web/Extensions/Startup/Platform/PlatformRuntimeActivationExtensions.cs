namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Agrupa la activación del runtime HTTP del host web una vez finalizada la inicialización crítica.
/// </summary>
public static class PlatformRuntimeActivationExtensions
{
    /// <summary>
    /// Activa el pipeline HTTP y el mapeo de endpoints del host web.
    /// </summary>
    public static WebApplication ActivateWebApplicationRuntime(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseApplicationRequestPipeline();
        app.MapApplicationEndpoints();

        return app;
    }
}
