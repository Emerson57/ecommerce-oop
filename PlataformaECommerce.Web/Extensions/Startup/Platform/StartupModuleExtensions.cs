using PlataformaECommerce.Application.DependencyInjection;
using PlataformaECommerce.Infrastructure.DependencyInjection;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Expone módulos de startup consolidados por dominio para mantener una convención estable en la composition root.
/// </summary>
public static class StartupModuleExtensions
{
    /// <summary>
    /// Registra el módulo base de plataforma con la capa de aplicación y la infraestructura compartida.
    /// </summary>
    public static IServiceCollection AddPlatformModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(hostEnvironment);

        services
            .AddApplicationServices()
            .AddInfrastructure(configuration, hostEnvironment);

        return services;
    }

    /// <summary>
    /// Registra el módulo de observabilidad del host web con correlación, problem details y telemetría HTTP.
    /// </summary>
    public static IServiceCollection AddObservabilityModule(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddProblemDetailsHandling()
            .AddObservabilityConfiguration(configuration);

        return services;
    }

    /// <summary>
    /// Registra el módulo de seguridad del host web con headers reenviados, antiforgery, autenticación y rate limiting.
    /// </summary>
    public static IServiceCollection AddSecurityModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(hostEnvironment);

        services
            .AddForwardedHeadersSupport(configuration, hostEnvironment)
            .AddAntiforgeryProtection(configuration, hostEnvironment)
            .AddSecurityServices(configuration, hostEnvironment)
            .AddRateLimitingPolicies(configuration);

        return services;
    }

    /// <summary>
    /// Registra el módulo de presentación del storefront y del backoffice con sus opciones visuales y servicios web.
    /// </summary>
    public static IServiceCollection AddPresentationModule(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddBrandingConfiguration(configuration)
            .AddBackofficeConfiguration(configuration)
            .AddPresentationServices();

        return services;
    }

    /// <summary>
    /// Registra el módulo operativo del host web con inicialización controlada, health checks y OpenAPI.
    /// </summary>
    public static IServiceCollection AddOperationsModule(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddHealthChecksServices(configuration)
            .AddOpenApiDocumentation()
            .AddInitializationServices(configuration);

        return services;
    }
}
