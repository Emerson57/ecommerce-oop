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
            // Apply runtime overrides (environment variables, command line) before resolving secret aliases
            .AddRuntimeOverrideConfigurationSources(args)
            .AddSecretAliasConfigurationSources();

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
