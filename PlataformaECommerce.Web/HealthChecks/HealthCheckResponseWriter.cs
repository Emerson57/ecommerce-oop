using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PlataformaECommerce.Web.Extensions.Startup;

namespace PlataformaECommerce.Web.HealthChecks;

/// <summary>
/// Escribe respuestas JSON homogéneas para endpoints de health checks.
/// </summary>
public static class HealthCheckResponseWriter
{
    /// <summary>
    /// Serializa el resultado consolidado de health checks como JSON.
    /// </summary>
    public static Task WriteJsonAsync(HttpContext context, HealthReport report)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(report);

        context.Response.ContentType = "application/json";

        string correlationId = RequestCorrelationContextResolver.Resolve(context);

        var payload = new
        {
            status = report.Status.ToString(),
            traceId = context.TraceIdentifier,
            correlationId,
            totalDuration = report.TotalDuration.TotalMilliseconds,
            results = report.Entries.ToDictionary(
                entry => entry.Key,
                entry => new
                {
                    status = entry.Value.Status.ToString(),
                    description = entry.Value.Description,
                    duration = entry.Value.Duration.TotalMilliseconds,
                    tags = entry.Value.Tags,
                    data = entry.Value.Data.ToDictionary(pair => pair.Key, pair => pair.Value)
                })
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
