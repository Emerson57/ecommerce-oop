using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Centraliza la incorporación de fuentes de configuración adicionales del arranque web.
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Agrega configuración local exclusiva de desarrollo sin alterar el comportamiento de otros entornos.
    /// </summary>
    /// <param name="configuration">Administrador de configuración de la aplicación.</param>
    /// <param name="hostEnvironment">Entorno actual de ejecución.</param>
    /// <returns>El mismo administrador de configuración para encadenamiento fluido.</returns>
    public static ConfigurationManager AddLocalDevelopmentConfiguration(this ConfigurationManager configuration, IHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(hostEnvironment);

        if (!hostEnvironment.IsDevelopment())
        {
            return configuration;
        }

        configuration
            .AddUserSecrets<global::Program>(optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{hostEnvironment.EnvironmentName}.local.json", optional: true, reloadOnChange: true);

        return configuration;
    }
}
