using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Web.Configuration;
using PlataformaECommerce.Web.Initialization;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Agrupa los servicios y procesos de inicialización crítica del arranque web.
/// </summary>
public static class InitializationStartupExtensions
{
    /// <summary>
    /// Registra opciones y servicios usados durante la inicialización controlada de la plataforma.
    /// </summary>
    /// <param name="services">Colección de servicios a configurar.</param>
    /// <param name="configuration">Configuración raíz utilizada por la inicialización.</param>
    /// <returns>La misma colección de servicios para encadenamiento fluido.</returns>
    public static IServiceCollection AddInitializationServices(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddInitializationConfigurationValidation(configuration)
            .AddInitializationInfrastructureVerificationTasks()
            .AddInitializationBootstrapTasks()
            .AddInitializationWarmupTasks();

        return services;
    }

    /// <summary>
    /// Ejecuta la inicialización crítica de la plataforma inmediatamente después de construir la aplicación.
    /// </summary>
    /// <param name="app">Aplicación web ya construida.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    public static async Task RunCriticalApplicationInitializationAsync(this WebApplication app, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(app);

        await using AsyncServiceScope initializationScope = app.Services.CreateAsyncScope();
        StartupInitializationOrchestrator initializationOrchestrator = initializationScope.ServiceProvider.GetRequiredService<StartupInitializationOrchestrator>();

        await initializationOrchestrator.RunAsync(cancellationToken).ConfigureAwait(false);
    }

    private static IServiceCollection AddInitializationConfigurationValidation(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddSingleton<IValidateOptions<BootstrapSuperUserOptions>, BootstrapSuperUserOptionsValidator>();

        services
            .AddOptions<BootstrapSuperUserOptions>()
            .Bind(configuration.GetSection(BootstrapSuperUserOptions.SectionName))
            .ValidateOnStart();

        return services;
    }

    private static IServiceCollection AddInitializationInfrastructureVerificationTasks(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<StartupInitializationOrchestrator>();
        services.AddScoped<IStartupInitializationTask, InfrastructureVerificationStartupTask>();

        return services;
    }

    private static IServiceCollection AddInitializationBootstrapTasks(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<ConfiguredTenantProvisioningService>();
        services.AddScoped<SuperUserBootstrapService>();
        services.AddScoped<IStartupInitializationTask, TenantCatalogSynchronizationStartupTask>();
        services.AddScoped<IStartupInitializationTask, TenantProvisioningStartupTask>();
        services.AddScoped<IStartupInitializationTask, SuperUserBootstrapStartupTask>();

        return services;
    }

    private static IServiceCollection AddInitializationWarmupTasks(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IStartupInitializationTask, TenantCatalogWarmupStartupTask>();

        return services;
    }
}
