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

namespace PlataformaECommerce.Infrastructure.DependencyInjection
{
    public static class InfrastructureServiceRegistration
    {
        /// Registra los servicios de infraestructura, incluyendo SQL Server,
        /// MongoDB, repositorios y unidad de trabajo.
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");

            services.AddDbContext<ECommerceDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddScoped<IProductoRepository, ProductRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            MongoDbSettings mongoSettings = configuration
                .GetSection("MongoDb")
                .Get<MongoDbSettings>()
                ?? throw new InvalidOperationException("No se encontró la configuración de MongoDb.");

            services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoSettings.ConnectionString));

            services.AddScoped<IMongoDatabase>(sp =>
            {
                IMongoClient client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(mongoSettings.DatabaseName);
            });

            services.AddScoped<IProductoAuditRepository, MongoProductAuditRepository>();

            return services;
        }
    }
}