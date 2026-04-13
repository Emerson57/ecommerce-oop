using PlataformaECommerce.Application.DependencyInjection;
using PlataformaECommerce.Infrastructure.DependencyInjection;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Centraliza la composition root del host web para mantener `Program.cs` mínimo y legible.
/// </summary>
public static class CompositionRootExtensions
{
    /// <summary>
    /// Carga las fuentes de configuración requeridas por la aplicación web según el entorno actual.
    /// </summary>
    public static WebApplicationBuilder ConfigureApplicationConfiguration(this WebApplicationBuilder builder, string[] args)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(args);

        builder.Configuration
            .AddModularConfigurationSources(builder.Environment)
            .AddLocalDevelopmentConfigurationSources(builder.Environment)
            .AddRuntimeOverrideConfigurationSources(args);

        return builder;
    }

    /// <summary>
    /// Configura el logging estructurado del host usando la configuración efectiva ya cargada.
    /// </summary>
    public static WebApplicationBuilder ConfigureApplicationLogging(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Host.ConfigureSerilogLogging(builder.Configuration);
        return builder;
    }

    /// <summary>
    /// Registra los servicios de la aplicación web y sus dependencias de infraestructura.
    /// </summary>
    public static IServiceCollection AddApplicationCompositionServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(hostEnvironment);

        services
            .AddProblemDetailsHandling()
            .AddAntiforgeryProtection(configuration, hostEnvironment)
            .AddBrandingConfiguration(configuration)
            .AddBackofficeConfiguration(configuration)
            .AddPresentationServices()
            .AddOpenApiDocumentation()
            .AddObservabilityConfiguration(configuration)
            .AddForwardedHeadersSupport(configuration, hostEnvironment)
            .AddSecurityServices(configuration, hostEnvironment)
            .AddRateLimitingPolicies(configuration)
            .AddHealthChecksServices(configuration)
            .AddInitializationServices(configuration)
            .AddApplicationServices()
            .AddInfrastructure(configuration, hostEnvironment);

        return services;
    }

    /// <summary>
    /// Ejecuta la inicialización crítica que debe completarse antes de atender tráfico.
    /// </summary>
    public static Task RunApplicationStartupInitializationAsync(this WebApplication app, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(app);
        return InitializationStartupExtensions.RunCriticalApplicationInitializationAsync(app, cancellationToken);
    }

    /// <summary>
    /// Configura el pipeline HTTP completo de la aplicación web.
    /// </summary>
    public static WebApplication UseApplicationPipeline(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseApplicationRequestPipeline();
    }

    /// <summary>
    /// Mapea los endpoints HTTP expuestos por la aplicación web.
    /// </summary>
    public static WebApplication MapHttpEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.MapApplicationEndpoints();
    }
}
