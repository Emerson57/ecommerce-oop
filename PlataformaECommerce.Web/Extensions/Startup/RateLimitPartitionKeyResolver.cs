using Microsoft.AspNetCore.Http;

namespace PlataformaECommerce.Web.Extensions.Startup;

internal static class RateLimitPartitionKeyResolver
{
    public static string Resolve(HttpContext httpContext)
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
}
