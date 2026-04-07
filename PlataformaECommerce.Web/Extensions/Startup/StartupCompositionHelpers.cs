using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PlataformaECommerce.Web.Configuration;
using PlataformaECommerce.Web.Middlewares;

namespace PlataformaECommerce.Web.Extensions.Startup;

internal static partial class StartupCompositionHelpers
{
    public static void PopulateProblemDetails(HttpContext httpContext, ProblemDetails problemDetails)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(problemDetails);

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
        problemDetails.Extensions["correlationId"] = ResolveCorrelationId(httpContext);
        problemDetails.Extensions["timestampUtc"] = DateTime.UtcNow;
    }

    public static string ResolveCorrelationId(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        return httpContext.Items.TryGetValue(RequestCorrelationMiddleware.CorrelationIdItemKey, out object? correlationIdValue)
            ? Convert.ToString(correlationIdValue, CultureInfo.InvariantCulture) ?? httpContext.TraceIdentifier
            : httpContext.TraceIdentifier;
    }

    public static bool IsValidHexColor(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return HexColorRegex().IsMatch(value.Trim());
    }

    public static bool AreValidRateLimitingOptions(WebRateLimitingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return IsValidPolicy(options.AuthFlow)
            && IsValidPolicy(options.SensitiveApi)
            && IsValidPolicy(options.PublicApi);
    }

    public static void AddFixedWindowPolicy(
        RateLimiterOptions options,
        string policyName,
        WebRateLimitingOptions.FixedWindowPolicyOptions policy)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(policy);

        options.AddPolicy(policyName, httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: ResolveRateLimitPartitionKey(httpContext),
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = policy.PermitLimit,
                    Window = TimeSpan.FromSeconds(policy.WindowSeconds),
                    QueueLimit = policy.QueueLimit,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    AutoReplenishment = true
                }));
    }

    private static bool IsValidPolicy(WebRateLimitingOptions.FixedWindowPolicyOptions? options)
    {
        return options is not null
            && options.PermitLimit > 0
            && options.WindowSeconds > 0
            && options.QueueLimit >= 0;
    }

    private static string ResolveRateLimitPartitionKey(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        string routeBase = httpContext.Request.Path.HasValue
            ? httpContext.Request.Path.Value!
            : "unknown";

        string identity = httpContext.User.Identity?.IsAuthenticated == true
            ? httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous"
            : httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

        return $"{routeBase}:{identity}";
    }

    [GeneratedRegex("^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6})$")]
    private static partial Regex HexColorRegex();
}
