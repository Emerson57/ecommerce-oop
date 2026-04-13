using Microsoft.AspNetCore.Builder;
using PlataformaECommerce.Web.Middlewares;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Activa el manejo global de excepciones del dominio de observabilidad.
/// </summary>
public static class ObservabilityExceptionHandlingRuntimeActivationExtensions
{
    /// <summary>
    /// Activa el middleware centralizado de manejo de excepciones para respuestas seguras y trazables.
    /// </summary>
    public static WebApplication UseObservabilityExceptionHandlingRuntime(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseMiddleware<ExceptionHandlingMiddleware>();
        return app;
    }
}
