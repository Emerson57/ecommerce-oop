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

        services.AddSingleton<IValidateOptions<BootstrapSuperUserOptions>, BootstrapSuperUserOptionsValidator>();

        services
            .AddOptions<BootstrapSuperUserOptions>()
            .Bind(configuration.GetSection(BootstrapSuperUserOptions.SectionName))
            .ValidateOnStart();

        services.AddScoped<DevelopmentLegacyTenantDataNormalizer>();
        services.AddScoped<SuperUserBootstrapService>();
        services.AddScoped<SaaSPlatformInitializationService>();

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
        DevelopmentLegacyTenantDataNormalizer legacyTenantDataNormalizer = initializationScope.ServiceProvider.GetRequiredService<DevelopmentLegacyTenantDataNormalizer>();
        SaaSPlatformInitializationService initializationService = initializationScope.ServiceProvider.GetRequiredService<SaaSPlatformInitializationService>();

        await legacyTenantDataNormalizer.NormalizeAsync(cancellationToken).ConfigureAwait(false);
        await initializationService.InitializeAsync(cancellationToken).ConfigureAwait(false);
    }
}
