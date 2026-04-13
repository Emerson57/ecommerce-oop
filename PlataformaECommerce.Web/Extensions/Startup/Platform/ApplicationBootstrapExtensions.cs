namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Orquesta el bootstrap runtime del host HTTP antes de aceptar tráfico.
/// </summary>
public static class ApplicationBootstrapExtensions
{
    /// <summary>
    /// Ejecuta la inicialización de arranque y deja configurado el pipeline y el mapeo de endpoints.
    /// </summary>
    public static async Task<WebApplication> BootstrapWebApplicationAsync(this WebApplication app, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(app);

        await app.InitializeWebApplicationRuntimeAsync(cancellationToken);
        app.ActivateWebApplicationRuntime();

        return app;
    }
}
