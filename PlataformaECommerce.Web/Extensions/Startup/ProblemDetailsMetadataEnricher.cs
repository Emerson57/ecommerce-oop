using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PlataformaECommerce.Web.Extensions.Startup;

internal static class ProblemDetailsMetadataEnricher
{
    public static void Enrich(HttpContext httpContext, ProblemDetails problemDetails)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(problemDetails);

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
        problemDetails.Extensions["correlationId"] = CorrelationIdResolver.Resolve(httpContext);
        problemDetails.Extensions["timestampUtc"] = DateTime.UtcNow;
    }
}
