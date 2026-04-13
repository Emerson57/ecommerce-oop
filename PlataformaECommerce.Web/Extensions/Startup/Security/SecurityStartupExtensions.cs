using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Web.Authorization;
using PlataformaECommerce.Web.Configuration;
using PlataformaECommerce.Web.Security;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Centraliza la configuración de seguridad HTTP, autenticación y autorización del sitio web.
/// </summary>
public static class SecurityStartupExtensions
{
    /// <summary>
    /// Registra la configuración de seguridad requerida por la aplicación web sin alterar el orden del arranque original.
    /// </summary>
    /// <param name="services">Colección de servicios a configurar.</param>
    /// <param name="configuration">Configuración raíz disponible para las opciones tipadas.</param>
    /// <param name="hostEnvironment">Entorno actual de ejecución.</param>
    /// <returns>La misma colección de servicios para encadenamiento fluido.</returns>
    public static IServiceCollection AddSecurityServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(hostEnvironment);

        services.AddSingleton<IValidateOptions<WebAuthenticationCookiesOptions>, WebAuthenticationCookiesOptionsValidator>();
        services.AddSingleton<IValidateOptions<WebTransportSecurityOptions>, WebTransportSecurityOptionsValidator>();
        services.AddSingleton<IValidateOptions<WebOpenApiSecurityOptions>, WebOpenApiSecurityOptionsValidator>();
        services.AddSingleton<IValidateOptions<WebSecurityHeadersOptions>, WebSecurityHeadersOptionsValidator>();

        WebAuthenticationCookiesOptions authenticationCookiesOptions = configuration
            .GetSection(WebAuthenticationCookiesOptions.SectionName)
            .Get<WebAuthenticationCookiesOptions>()
            ?? new WebAuthenticationCookiesOptions();

        WebTransportSecurityOptions transportSecurityOptions = configuration
            .GetSection(WebTransportSecurityOptions.SectionName)
            .Get<WebTransportSecurityOptions>()
            ?? new WebTransportSecurityOptions();

        services.AddHsts(options =>
        {
            options.Preload = transportSecurityOptions.Preload;
            options.IncludeSubDomains = transportSecurityOptions.IncludeSubDomains;
            options.MaxAge = TimeSpan.FromDays(transportSecurityOptions.HstsMaxAgeDays);
        });

        services
            .AddOptions<WebAuthenticationCookiesOptions>()
            .Bind(configuration.GetSection(WebAuthenticationCookiesOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<WebTransportSecurityOptions>()
            .Bind(configuration.GetSection(WebTransportSecurityOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<WebOpenApiSecurityOptions>()
            .Bind(configuration.GetSection(WebOpenApiSecurityOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<WebSecurityHeadersOptions>()
            .Bind(configuration.GetSection(WebSecurityHeadersOptions.SectionName))
            .ValidateOnStart();

        services.AddScoped<AdminCookieSecurityService>();
        services.AddScoped<AdminCookieAuthenticationEvents>();
        services.AddScoped<CustomerCookieSecurityService>();
        services.AddScoped<CustomerCookieAuthenticationEvents>();

        services.AddAuthentication(options =>
            {
                options.DefaultScheme = AuthorizationPolicies.AppCookieScheme;
                options.DefaultAuthenticateScheme = AuthorizationPolicies.AppCookieScheme;
                options.DefaultChallengeScheme = AuthorizationPolicies.AppCookieScheme;
                options.DefaultSignOutScheme = AuthorizationPolicies.AppCookieScheme;
            })
            .AddPolicyScheme(AuthorizationPolicies.AppCookieScheme, "Application cookie selector", options =>
            {
                options.ForwardDefaultSelector = AuthorizationPolicies.ResolveApplicationCookieScheme;
            })
            .AddCookie(AuthorizationPolicies.AdminCookieScheme, options => AuthorizationPolicies.ConfigureAdminCookie(options, authenticationCookiesOptions))
            .AddCookie(AuthorizationPolicies.CustomerCookieScheme, options => AuthorizationPolicies.ConfigureCustomerCookie(options, authenticationCookiesOptions));

        services.AddAuthorization(AuthorizationPolicies.ConfigureBackofficePolicies);

        return services;
    }
}
