using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Centraliza la configuración antiforgery para formularios Razor Pages y endpoints protegidos por cookies.
/// </summary>
public static class AntiforgeryStartupExtensions
{
    /// <summary>
    /// Registra opciones y servicios antiforgery con una política homogénea para web interactiva.
    /// </summary>
    public static IServiceCollection AddAntiforgeryProtection(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(hostEnvironment);

        services.AddSingleton<IValidateOptions<WebAntiforgeryOptions>, WebAntiforgeryOptionsValidator>();

        services
            .AddOptions<WebAntiforgeryOptions>()
            .Bind(configuration.GetSection(WebAntiforgeryOptions.SectionName))
            .ValidateOnStart();

        WebAntiforgeryOptions configuredOptions = configuration
            .GetSection(WebAntiforgeryOptions.SectionName)
            .Get<WebAntiforgeryOptions>()
            ?? new WebAntiforgeryOptions();

        services.AddAntiforgery(options =>
        {
            options.Cookie.Name = configuredOptions.CookieName.Trim();
            options.Cookie.Path = "/";
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = hostEnvironment.IsDevelopment()
                ? CookieSecurePolicy.SameAsRequest
                : CookieSecurePolicy.Always;
            options.FormFieldName = configuredOptions.FormFieldName.Trim();
            options.HeaderName = configuredOptions.HeaderName.Trim();
            options.SuppressXFrameOptionsHeader = configuredOptions.SuppressXFrameOptionsHeader;
        });

        return services;
    }
}
