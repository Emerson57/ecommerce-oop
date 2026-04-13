using System.Globalization;
using Microsoft.AspNetCore.Http;
using PlataformaECommerce.Web.Middlewares;

namespace PlataformaECommerce.Web.Extensions.Startup;

internal static class RequestCorrelationContextResolver
{
    public static string Resolve(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        return httpContext.Items.TryGetValue(RequestCorrelationMiddleware.CorrelationIdItemKey, out object? correlationIdValue)
            ? Convert.ToString(correlationIdValue, CultureInfo.InvariantCulture) ?? httpContext.TraceIdentifier
            : httpContext.TraceIdentifier;
    }
}
