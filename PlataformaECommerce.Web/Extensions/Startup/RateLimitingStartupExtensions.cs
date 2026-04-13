using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Centraliza la configuración de rate limiting del front web y de los endpoints HTTP.
/// </summary>
public static class RateLimitingStartupExtensions
{
    /// <summary>
    /// Registra las políticas actuales de limitación de tráfico manteniendo la misma respuesta de rechazo.
    /// </summary>
    /// <param name="services">Colección de servicios a configurar.</param>
    /// <param name="configuration">Configuración raíz desde la cual se leen las opciones.</param>
    /// <returns>La misma colección de servicios para encadenamiento fluido.</returns>
    public static IServiceCollection AddRateLimitingPolicies(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<RateLimitingOptions>()
            .Bind(configuration.GetSection(RateLimitingOptions.SectionName))
            .Validate(RateLimitingOptionsValidator.AreValid, "La configuración de rate limiting contiene valores inválidos.")
            .ValidateOnStart();

        services.AddScoped<RateLimitPartitionKeyResolver>();

        RateLimitingOptions configuredRateLimitingOptions = configuration
            .GetSection(RateLimitingOptions.SectionName)
            .Get<RateLimitingOptions>()
            ?? new RateLimitingOptions();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = static async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = Math.Ceiling(retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                }

                context.HttpContext.Response.ContentType = "application/json";

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    mensaje = "Se alcanzó el límite temporal de solicitudes para este recurso.",
                    codigo = StatusCodes.Status429TooManyRequests,
                    traceId = context.HttpContext.TraceIdentifier
                }, cancellationToken: cancellationToken);
            };

            RateLimitPolicyBuilder.AddFixedWindowPolicy(options, RateLimitingOptions.AuthenticationPolicyName, configuredRateLimitingOptions.Authentication);
            RateLimitPolicyBuilder.AddFixedWindowPolicy(options, RateLimitingOptions.PublicApiPolicyName, configuredRateLimitingOptions.PublicApi);
            RateLimitPolicyBuilder.AddFixedWindowPolicy(options, RateLimitingOptions.AdministrationPolicyName, configuredRateLimitingOptions.Administration);
            RateLimitPolicyBuilder.AddFixedWindowPolicy(options, RateLimitingOptions.SensitiveEndpointsPolicyName, configuredRateLimitingOptions.SensitiveEndpoints);
        });

        return services;
    }
}
