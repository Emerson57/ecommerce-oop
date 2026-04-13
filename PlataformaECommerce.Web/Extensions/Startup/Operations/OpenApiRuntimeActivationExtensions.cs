using Microsoft.AspNetCore.Builder;
using PlataformaECommerce.Web.OpenApi;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Activa la superficie OpenAPI del host web en tiempo de ejecución según el entorno actual.
/// </summary>
public static class OpenApiRuntimeActivationExtensions
{
    /// <summary>
    /// Activa Swagger y Swagger UI para capacidades operativas visibles en tiempo de ejecución.
    /// </summary>
    public static WebApplication UseOperationsOpenApiRuntime(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint($"/swagger/{SwaggerGroups.Public}/swagger.json", "API Pública v1");
                options.SwaggerEndpoint($"/swagger/{SwaggerGroups.Admin}/swagger.json", "API Administrativa v1");
                options.DocumentTitle = "PlataformaECommerce Swagger";
            });
        }

        return app;
    }
}
