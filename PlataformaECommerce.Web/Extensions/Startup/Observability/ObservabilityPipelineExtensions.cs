using Microsoft.AspNetCore.Builder;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Coordina la activación de trazabilidad y diagnóstico operativo del pipeline HTTP.
/// </summary>
public static class ObservabilityPipelineExtensions
{
    /// <summary>
    /// Activa correlación, manejo global de excepciones y logging estructurado de solicitudes.
    /// </summary>
    public static WebApplication UseObservabilityModule(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseObservabilityCorrelationRuntime();
        app.UseObservabilityExceptionHandlingRuntime();
        app.UseObservabilityRequestLoggingRuntime();

        return app;
    }
}
