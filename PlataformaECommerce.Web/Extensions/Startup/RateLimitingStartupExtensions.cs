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
    public static IServiceCollection AddConfiguredRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<WebRateLimitingOptions>()
            .Bind(configuration.GetSection(WebRateLimitingOptions.SectionName))
            .Validate(options => StartupCompositionHelpers.AreValidRateLimitingOptions(options), "La configuración de rate limiting contiene valores inválidos.")
            .ValidateOnStart();

        WebRateLimitingOptions configuredRateLimitingOptions = configuration
            .GetSection(WebRateLimitingOptions.SectionName)
            .Get<WebRateLimitingOptions>()
            ?? new WebRateLimitingOptions();

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

            StartupCompositionHelpers.AddFixedWindowPolicy(options, WebRateLimitingOptions.AuthFlowPolicyName, configuredRateLimitingOptions.AuthFlow);
            StartupCompositionHelpers.AddFixedWindowPolicy(options, WebRateLimitingOptions.SensitiveApiPolicyName, configuredRateLimitingOptions.SensitiveApi);
            StartupCompositionHelpers.AddFixedWindowPolicy(options, WebRateLimitingOptions.PublicApiPolicyName, configuredRateLimitingOptions.PublicApi);
        });

        return services;
    }
}
