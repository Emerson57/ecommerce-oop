using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Centraliza la incorporación de fuentes de configuración adicionales del arranque web.
/// </summary>
public static class ConfigurationExtensions
{
    private static readonly string[] ModularConfigurationNames =
    [
        "Observability",
        "Security",
        "Branding",
        "Backoffice",
        "SaaS",
        "Payments",
        "Infrastructure"
    ];

    /// <summary>
    /// Agrega archivos de configuración especializados por dominio para desacoplar responsabilidades del `appsettings.json` principal.
    /// </summary>
    /// <param name="configuration">Administrador de configuración de la aplicación.</param>
    /// <param name="hostEnvironment">Entorno actual de ejecución.</param>
    /// <returns>El mismo administrador de configuración para encadenamiento fluido.</returns>
    public static ConfigurationManager AddModularConfigurationSources(this ConfigurationManager configuration, IHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(hostEnvironment);

        foreach (string moduleName in ModularConfigurationNames)
        {
            configuration
                .AddJsonFile($"appsettings.{moduleName}.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{moduleName}.{hostEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
        }

        return configuration;
    }

    /// <summary>
    /// Agrega configuración local exclusiva de desarrollo sin alterar el comportamiento de otros entornos.
    /// </summary>
    /// <param name="configuration">Administrador de configuración de la aplicación.</param>
    /// <param name="hostEnvironment">Entorno actual de ejecución.</param>
    /// <returns>El mismo administrador de configuración para encadenamiento fluido.</returns>
    public static ConfigurationManager AddLocalDevelopmentConfigurationSources(this ConfigurationManager configuration, IHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(hostEnvironment);

        if (!hostEnvironment.IsDevelopment())
        {
            return configuration;
        }

        configuration
            .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{hostEnvironment.EnvironmentName}.local.json", optional: true, reloadOnChange: true)
            .AddUserSecrets<global::Program>(optional: true, reloadOnChange: true);

        return configuration;
    }

    /// <summary>
    /// Reaplica las fuentes de override seguras del host para que variables de entorno y argumentos
    /// mantengan la máxima precedencia sobre archivos JSON locales o modulares.
    /// </summary>
    /// <param name="configuration">Administrador de configuración de la aplicación.</param>
    /// <param name="args">Argumentos recibidos por el proceso.</param>
    /// <returns>El mismo administrador de configuración para encadenamiento fluido.</returns>
    public static ConfigurationManager AddRuntimeOverrideConfigurationSources(this ConfigurationManager configuration, string[] args)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(args);

        configuration.AddEnvironmentVariables();

        if (args.Length > 0)
        {
            configuration.AddCommandLine(args);
        }

        return configuration;
    }
}
