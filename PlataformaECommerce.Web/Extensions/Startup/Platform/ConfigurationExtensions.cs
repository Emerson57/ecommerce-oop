using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using PlataformaECommerce.Web.Configuration;

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

        // Load local overlay files only in Development and mark them optional to avoid startup failures
        configuration
            .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{hostEnvironment.EnvironmentName}.local.json", optional: true, reloadOnChange: true)
            .AddUserSecrets<global::Program>(optional: true, reloadOnChange: true);

        return configuration;
    }

    /// <summary>
    /// Proyecta alias de secretos con nombres profesionales hacia las claves runtime esperadas por la aplicación,
    /// permitiendo consumir User Secrets, variables de entorno o secret managers sin acoplar la lógica funcional.
    /// </summary>
    /// <param name="configuration">Administrador de configuración de la aplicación.</param>
    /// <returns>El mismo administrador de configuración para encadenamiento fluido.</returns>
    public static ConfigurationManager AddSecretAliasConfigurationSources(this ConfigurationManager configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        Dictionary<string, string?> secretOverrides = new(StringComparer.OrdinalIgnoreCase);

        foreach ((string sourcePath, string destinationPath) in SecretConfigurationAliases.Mappings)
        {
            string? secretValue = configuration[sourcePath];
            if (string.IsNullOrWhiteSpace(secretValue))
            {
                continue;
            }

            secretOverrides[destinationPath] = secretValue;
        }

        if (secretOverrides.Count > 0)
        {
            configuration.AddInMemoryCollection(secretOverrides);
        }

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
