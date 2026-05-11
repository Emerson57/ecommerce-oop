using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Orquesta la configuración de alto nivel del host web antes de construir la aplicación.
/// </summary>
public static class ApplicationHostBuilderExtensions
{
    /// <summary>
    /// Configura las fuentes de configuración, el logging estructurado y la composición de servicios del host web.
    /// </summary>
    public static WebApplicationBuilder ConfigureWebApplicationHost(this WebApplicationBuilder builder, string[] args)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(args);

        builder.ConfigureWebApplicationConfiguration(args);
        AllowedHostsConfigurationGuard.Validate(builder.Configuration, builder.Environment);
        AdministratorCreationConfigurationGuard.Validate(builder.Configuration, builder.Environment);
        SaaSPlatformProductionGuard.Validate(builder.Configuration, builder.Environment);
        EntityFrameworkProductionGuard.Validate(builder.Configuration, builder.Environment);
        ProductionSecretsConfigurationGuard.Validate(builder.Configuration, builder.Environment);
        builder.ConfigureWebApplicationLogging();
        builder.Services.AddWebApplicationModules(builder.Configuration, builder.Environment);

        return builder;
    }
}
