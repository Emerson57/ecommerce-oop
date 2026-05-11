namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Agrupa la configuración base del host web previa al registro de módulos de la solución.
/// </summary>
public static class PlatformHostConfigurationExtensions
{
    /// <summary>
    /// Registra la configuración efectiva consumida por el host web y sus módulos especializados.
    /// </summary>
    public static WebApplicationBuilder ConfigureWebApplicationConfiguration(this WebApplicationBuilder builder, string[] args)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(args);

        builder.Configuration
            .AddModularConfigurationSources(builder.Environment)
            .AddLocalDevelopmentConfigurationSources(builder.Environment)
            // Variables de entorno y línea de comandos antes de proyectar alias Secrets:* -> claves runtime.
            .AddRuntimeOverrideConfigurationSources(args)
            .AddSecretAliasConfigurationSources()
            // Reaplicar env/cmdline para que tengan precedencia sobre los valores proyectados por alias.
            .AddRuntimeOverrideConfigurationSources(args);

        return builder;
    }

    /// <summary>
    /// Activa el proveedor de logging estructurado del host web con la configuración efectiva ya resuelta.
    /// </summary>
    public static WebApplicationBuilder ConfigureWebApplicationLogging(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Host.ConfigureSerilogLogging(builder.Configuration);
        return builder;
    }
}
