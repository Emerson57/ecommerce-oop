namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Agrupa la inicialización runtime que debe completarse antes de aceptar tráfico HTTP.
/// </summary>
public static class PlatformRuntimeInitializationExtensions
{
    /// <summary>
    /// Ejecuta la inicialización crítica del host web antes de activar el pipeline HTTP.
    /// </summary>
    public static Task InitializeWebApplicationRuntimeAsync(this WebApplication app, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.RunCriticalApplicationInitializationAsync(cancellationToken);
    }
}
