using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using PlataformaECommerce.Web.OpenApi;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Agrupa la configuración OpenAPI y Swagger de la aplicación web.
/// </summary>
public static class OpenApiStartupExtensions
{
    /// <summary>
    /// Registra el explorador de endpoints y la generación de documentos Swagger agrupados.
    /// </summary>
    /// <param name="services">Colección de servicios a configurar.</param>
    /// <returns>La misma colección de servicios para encadenamiento fluido.</returns>
    public static IServiceCollection AddConfiguredOpenApi(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(SwaggerGroups.Public, new OpenApiInfo
            {
                Title = "PlataformaECommerce API Pública",
                Version = "v1",
                Description = "Endpoints públicos de consulta del catálogo de productos."
            });

            options.SwaggerDoc(SwaggerGroups.Admin, new OpenApiInfo
            {
                Title = "PlataformaECommerce API Administrativa",
                Version = "v1",
                Description = "Endpoints administrativos protegidos para gestión integral del catálogo."
            });

            options.DocInclusionPredicate((documentName, apiDescription) =>
            {
                string? groupName = apiDescription.GroupName;
                return string.Equals(groupName, documentName, StringComparison.OrdinalIgnoreCase);
            });
        });

        return services;
    }
}
