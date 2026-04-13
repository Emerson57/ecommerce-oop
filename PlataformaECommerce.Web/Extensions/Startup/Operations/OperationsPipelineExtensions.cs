using Microsoft.AspNetCore.Builder;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Coordina la activación de capacidades operativas del host durante el pipeline HTTP.
/// </summary>
public static class OperationsPipelineExtensions
{
    /// <summary>
    /// Activa las capacidades operativas visibles en tiempo de ejecución para entornos de desarrollo.
    /// </summary>
    public static WebApplication UseOperationsModule(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseOperationsOpenApiRuntime();

        return app;
    }
}
