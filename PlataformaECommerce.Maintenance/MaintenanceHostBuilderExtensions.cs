using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PlataformaECommerce.Application.DependencyInjection;
using PlataformaECommerce.Infrastructure.DependencyInjection;
using PlataformaECommerce.Web.Extensions.Startup;
using PlataformaECommerce.Web.Initialization;

namespace PlataformaECommerce.Maintenance;

internal static class MaintenanceHostBuilderExtensions
{
    public static HostApplicationBuilder ConfigureMaintenanceHost(this HostApplicationBuilder builder, string[] args)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(args);

        ConfigureMaintenanceConfiguration(builder, args);
        builder.Services.AddMaintenanceServices(builder.Configuration, builder.Environment);

        return builder;
    }

    private static void ConfigureMaintenanceConfiguration(HostApplicationBuilder builder, string[] args)
    {
        string webProjectPath = ResolveWebProjectPath(builder.Environment.ContentRootPath);
        builder.Configuration.Sources.Clear();
        builder.Configuration.SetBasePath(webProjectPath);
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
        builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false);
        builder.Configuration
            .AddModularConfigurationSources(builder.Environment)
            .AddLocalDevelopmentConfigurationSources(builder.Environment)
            .AddSecretAliasConfigurationSources()
            .AddRuntimeOverrideConfigurationSources(args);
    }

    private static IServiceCollection AddMaintenanceServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(hostEnvironment);

        services
            .AddApplicationServices()
            .AddInfrastructure(configuration, hostEnvironment)
            .AddScoped<DevelopmentLegacyTenantDataNormalizer>()
            .AddScoped<MaintenanceCommandDispatcher>();

        return services;
    }

    private static string ResolveWebProjectPath(string maintenanceProjectPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(maintenanceProjectPath);

        string webProjectPath = Path.GetFullPath(Path.Combine(maintenanceProjectPath, "..", "PlataformaECommerce.Web"));
        if (!Directory.Exists(webProjectPath))
        {
            throw new DirectoryNotFoundException($"No se encontró la ruta del proyecto web requerida por el proceso de mantenimiento: '{webProjectPath}'.");
        }

        return webProjectPath;
    }
}
