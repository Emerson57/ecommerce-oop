using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PlataformaECommerce.Application.Features.Admin.Services;
using PlataformaECommerce.Application.Features.Orders.Services;
using PlataformaECommerce.Application.Features.Products.Services;
using PlataformaECommerce.Application.Interfaces.Services.Admin;
using PlataformaECommerce.Application.Interfaces.Services.Orders;
using PlataformaECommerce.Application.Interfaces.Services.Products;

namespace PlataformaECommerce.Application.DependencyInjection;

/// <summary>
/// Proporciona métodos de extensión para registrar los servicios
/// de la capa Application dentro del contenedor de dependencias.
/// </summary>
/// <remarks>
/// Esta clase centraliza la configuración de inyección de dependencias
/// correspondiente a la capa de aplicación, permitiendo que la capa
/// de composición raíz registre de forma consistente:
/// - servicios de aplicación,
/// - validadores,
/// - y demás componentes propios del ensamblado Application.
///
/// Su propósito es mantener una única puerta de entrada para el registro
/// de dependencias de esta capa, reduciendo acoplamiento y mejorando
/// la mantenibilidad de la configuración.
///
/// La implementación actual registra automáticamente:
/// - clases concretas terminadas en <c>ApplicationService</c>,
/// - y validadores de FluentValidation.
///
/// En etapas posteriores, esta misma clase puede evolucionar para incluir:
/// - validadores,
/// - behaviors de pipeline,
/// - AutoMapper,
/// - MediatR,
/// - FluentValidation,
/// - u otras piezas transversales de Application.
/// </remarks>
public static class ApplicationServiceRegistration
{
    #region Métodos públicos

    /// <summary>
    /// Registra los servicios de la capa Application en el contenedor de dependencias.
    /// </summary>
    /// <param name="services">Colección de servicios a configurar.</param>
    /// <returns>
    /// La misma colección de servicios para permitir encadenamiento fluido.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Se produce cuando la colección de servicios es nula.
    /// </exception>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        Assembly applicationAssembly = typeof(ApplicationServiceRegistration).Assembly;

        RegisterSpecializedApplicationServices(services);
        RegisterApplicationServices(services, applicationAssembly);
        RegisterValidators(services, applicationAssembly);

        return services;
    }

    #endregion

    #region Métodos privados auxiliares

    /// <summary>
    /// Registra explícitamente los servicios especializados introducidos por la FASE 1.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    private static void RegisterSpecializedApplicationServices(IServiceCollection services)
    {
        services.AddScoped<IProductCommandService, ProductCommandService>();
        services.AddScoped<IProductQueryService, ProductQueryService>();
        services.AddScoped<IProductStockService, ProductStockService>();
        services.AddScoped<IProductPromotionService, ProductPromotionService>();
        services.AddScoped<IProductApplicationService>(serviceProvider =>
            new ProductApplicationService(
                serviceProvider.GetRequiredService<IProductCommandService>(),
                serviceProvider.GetRequiredService<IProductQueryService>(),
                serviceProvider.GetRequiredService<IProductStockService>(),
                serviceProvider.GetRequiredService<IProductPromotionService>()));

        services.AddScoped<IOrderCreationService, OrderCreationService>();
        services.AddScoped<IOrderLifecycleService, OrderLifecycleService>();
        services.AddScoped<IOrderQueryService, OrderQueryService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IOrderPaymentCheckoutService, OrderPaymentCheckoutService>();
        services.AddScoped<IOrderApplicationService>(serviceProvider =>
            new OrderApplicationService(
                serviceProvider.GetRequiredService<IOrderCreationService>(),
                serviceProvider.GetRequiredService<IOrderLifecycleService>(),
                serviceProvider.GetRequiredService<IOrderQueryService>(),
                serviceProvider.GetRequiredService<IPaymentService>()));

        services.AddScoped<AdminAuthService>();
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddScoped<IAdminDashboardService, AdminDashboardService>();
        services.AddScoped<IAdminApplicationService>(serviceProvider =>
            new AdminApplicationService(
                serviceProvider.GetRequiredService<IAdminUserService>(),
                serviceProvider.GetRequiredService<IAdminDashboardService>()));
    }

    /// <summary>
    /// Registra automáticamente los servicios de aplicación concretos del ensamblado.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="assembly">Ensamblado Application a inspeccionar.</param>
    private static void RegisterApplicationServices(IServiceCollection services, Assembly assembly)
    {
        IEnumerable<Type> serviceTypes = assembly
            .GetTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false } &&
                type.Name.EndsWith("ApplicationService", StringComparison.Ordinal) &&
                !IsExplicitlyRegisteredFacade(type));

        foreach (Type implementationType in serviceTypes)
        {
            Type[] serviceInterfaces = implementationType
                .GetInterfaces()
                .Where(@interface => !IsValidatorInterface(@interface))
                .ToArray();

            if (serviceInterfaces.Length == 0)
            {
                services.AddScoped(implementationType);
                continue;
            }

            foreach (Type serviceInterface in serviceInterfaces)
            {
                services.AddScoped(serviceInterface, implementationType);
            }
        }
    }

    /// <summary>
    /// Registra automáticamente los validadores de FluentValidation definidos en el ensamblado.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="assembly">Ensamblado Application a inspeccionar.</param>
    private static void RegisterValidators(IServiceCollection services, Assembly assembly)
    {
        IEnumerable<Type> implementationTypes = assembly
            .GetTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false });

        foreach (Type implementationType in implementationTypes)
        {
            Type[] validatorInterfaces = implementationType
                .GetInterfaces()
                .Where(IsValidatorInterface)
                .ToArray();

            foreach (Type validatorInterface in validatorInterfaces)
            {
                services.AddScoped(validatorInterface, implementationType);
            }
        }
    }

    /// <summary>
    /// Determina si una interfaz corresponde a un validador de FluentValidation.
    /// </summary>
    /// <param name="interfaceType">Interfaz a evaluar.</param>
    /// <returns>
    /// <see langword="true"/> si la interfaz corresponde a un validador;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    private static bool IsValidatorInterface(Type interfaceType)
    {
        if (!interfaceType.IsGenericType)
        {
            return false;
        }

        Type genericTypeDefinition = interfaceType.GetGenericTypeDefinition();

        return genericTypeDefinition == typeof(IValidator<>);
    }

    /// <summary>
    /// Determina si la implementación corresponde a una fachada registrada manualmente.
    /// </summary>
    /// <param name="implementationType">Tipo concreto a evaluar.</param>
    /// <returns><see langword="true"/> cuando la fachada ya se registra explícitamente.</returns>
    private static bool IsExplicitlyRegisteredFacade(Type implementationType)
    {
        return implementationType == typeof(ProductApplicationService)
            || implementationType == typeof(OrderApplicationService)
            || implementationType == typeof(AdminApplicationService);
    }

    #endregion
}