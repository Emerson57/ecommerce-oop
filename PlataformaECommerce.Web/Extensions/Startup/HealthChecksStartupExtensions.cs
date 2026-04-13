using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PlataformaECommerce.Infrastructure.Mongo;
using PlataformaECommerce.Infrastructure.Persistence.Context;
using PlataformaECommerce.Web.HealthChecks;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Agrupa la configuración de health checks requeridos por el host web.
/// </summary>
public static class HealthChecksStartupExtensions
{
    /// <summary>
    /// Registra los health checks de vida y alistamiento manteniendo las dependencias actuales.
    /// </summary>
    /// <param name="services">Colección de servicios a configurar.</param>
    /// <param name="configuration">Configuración raíz para health checks condicionados.</param>
    /// <returns>La misma colección de servicios para encadenamiento fluido.</returns>
    public static IServiceCollection AddHealthChecksServices(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        IHealthChecksBuilder healthChecksBuilder = services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy("La aplicación web se encuentra operativa."), tags: ["live"])
            .AddDbContextCheck<ECommerceDbContext>(name: "sql-server", tags: ["ready"]);

        MongoDbSettings mongoDbSettings = configuration.GetSection(MongoDbSettings.SectionName).Get<MongoDbSettings>() ?? new MongoDbSettings();
        if (mongoDbSettings.Enabled)
        {
            healthChecksBuilder.AddCheck<MongoDbHealthCheck>("mongo-audit", tags: ["ready"]);
        }

        return services;
    }
}
