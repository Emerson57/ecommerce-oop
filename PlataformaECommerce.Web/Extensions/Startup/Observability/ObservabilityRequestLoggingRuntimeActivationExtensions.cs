using Microsoft.AspNetCore.Builder;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Activa el logging estructurado de solicitudes del dominio de observabilidad.
/// </summary>
public static class ObservabilityRequestLoggingRuntimeActivationExtensions
{
    /// <summary>
    /// Activa el logging estructurado de solicitudes HTTP con enriquecimiento diagnóstico.
    /// </summary>
    public static WebApplication UseObservabilityRequestLoggingRuntime(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseSerilogRequestLoggingDiagnostics();
        return app;
    }
}
