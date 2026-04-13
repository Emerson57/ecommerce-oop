using Microsoft.AspNetCore.Http;

namespace PlataformaECommerce.Web.Extensions.Startup;

internal static class RateLimitEndpointGroupResolver
{
    public static string Resolve(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        if (httpContext.Request.RouteValues.ContainsKey("page"))
        {
            return "page";
        }

        if (httpContext.Request.RouteValues.ContainsKey("controller"))
        {
            return "api";
        }

        return "endpoint";
    }
}
