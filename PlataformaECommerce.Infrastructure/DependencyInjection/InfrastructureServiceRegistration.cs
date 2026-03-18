using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using PlataformaECommerce.Application.Interfaces.Persistence;
using PlataformaECommerce.Application.Interfaces.Repositories.Audit;
using PlataformaECommerce.Application.Interfaces.Repositories.Products;
using PlataformaECommerce.Infrastructure.Mongo;
using PlataformaECommerce.Infrastructure.Mongo.Repositories;
using PlataformaECommerce.Infrastructure.Persistence.Context;
using PlataformaECommerce.Infrastructure.Repositories.Common;
using PlataformaECommerce.Infrastructure.Repositories.Products;

namespace PlataformaECommerce.Infrastructure.DependencyInjection;

/// <summary>
/// Proporciona extensiones para registrar los servicios de infraestructura del sistema.
/// </summary>
public static class InfrastructureServiceRegistration
{
    /// <summary>
    /// Registra los servicios de infraestructura, incluyendo SQL Server, MongoDB,
    /// repositorios y unidad de trabajo.
    /// </summary>
    /// <param name="services">Colección de servicios de la aplicación.</param>
    /// <param name="configuration">Configuración raíz del entorno.</param>
    /// <returns>La colección de servicios para encadenar registro.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        string connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");

        services.AddDbContext<ECommerceDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        MongoDbSettings mongoSettings = configuration
            .GetSection("MongoDb")
            .Get<MongoDbSettings>()
            ?? throw new InvalidOperationException("No se encontró la configuración de MongoDb.");

        services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoSettings.ConnectionString));

        services.AddScoped<IMongoDatabase>(serviceProvider =>
        {
            IMongoClient client = serviceProvider.GetRequiredService<IMongoClient>();
            return client.GetDatabase(mongoSettings.DatabaseName);
        });

        services.AddScoped<IProductoAuditRepository, MongoProductAuditRepository>();

        return services;
    }
}