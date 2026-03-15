using Microsoft.Extensions.DependencyInjection;
using PlataformaECommerce.Application.Features.Products.Services;
using PlataformaECommerce.Application.Interfaces.Services.Products;

namespace PlataformaECommerce.Application.DependencyInjection
{
    public static class ApplicationServiceRegistration
    {
        /// Registra los servicios de aplicación de la solución.
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Registro de servicios de aplicación
            services.AddScoped<IProductoService, ProductService>();

            return services;
        }
    }
}