using Microsoft.AspNetCore.Builder;
using PlataformaECommerce.Web.Middlewares;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Activa la correlación runtime del dominio de observabilidad.
/// </summary>
public static class ObservabilityCorrelationRuntimeActivationExtensions
{
    /// <summary>
    /// Activa el middleware que establece un identificador de correlación estable por solicitud.
    /// </summary>
    public static WebApplication UseObservabilityCorrelationRuntime(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseMiddleware<RequestCorrelationMiddleware>();
        return app;
    }
}
