namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Coordina el registro de módulos de startup del host web siguiendo la organización por dominios.
/// </summary>
public static class PlatformModuleRegistrationExtensions
{
    /// <summary>
    /// Registra los módulos de startup del host web en la secuencia oficial de composición.
    /// </summary>
    public static IServiceCollection AddWebApplicationModules(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(hostEnvironment);

        services
            .AddObservabilityModule(configuration)
            .AddSecurityModule(configuration, hostEnvironment)
            .AddPresentationModule(configuration)
            .AddOperationsModule(configuration)
            .AddPlatformModule(configuration, hostEnvironment);

        return services;
    }
}
