using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Centraliza el registro de configuración comercial y de branding del storefront y backoffice.
/// </summary>
public static class BrandingStartupExtensions
{
    /// <summary>
    /// Registra y valida la configuración de experiencia comercial activa.
    /// </summary>
    public static IServiceCollection AddBrandingConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<ClientExperienceOptions>()
            .Bind(configuration.GetSection(ClientExperienceOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(options => BrandingHexColorValidator.IsValid(options.PrimaryColor), "La configuración comercial requiere un color primario hexadecimal válido.")
            .Validate(options => BrandingHexColorValidator.IsValid(options.AccentColor), "La configuración comercial requiere un color de acento hexadecimal válido.")
            .Validate(options => BrandingHexColorValidator.IsValid(options.AdminSidebarStartColor), "La configuración comercial requiere un color inicial válido para el sidebar administrativo.")
            .Validate(options => BrandingHexColorValidator.IsValid(options.AdminSidebarEndColor), "La configuración comercial requiere un color final válido para el sidebar administrativo.")
            .ValidateOnStart();

        return services;
    }
}
