using Microsoft.AspNetCore.Mvc;
using PlataformaECommerce.Web.Extensions.Startup;

namespace PlataformaECommerce.Web.Middlewares;

/// <summary>
/// Escribe respuestas homogéneas RFC 7807 para excepciones no controladas de la aplicación web.
/// </summary>
internal static class ProblemDetailsExceptionResponseWriter
{
    public static Task WriteAsync(
        HttpContext context,
        UnhandledExceptionDescriptor descriptor,
        Exception exception,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(exception);

        context.Response.Clear();
        context.Response.StatusCode = descriptor.StatusCode;
        ProblemDetails problemDetails = new()
        {
            Title = descriptor.Title,
            Detail = descriptor.Detail,
            Status = descriptor.StatusCode,
            Type = $"https://httpstatuses.io/{descriptor.StatusCode}",
            Instance = context.Request.Path
        };

        ProblemDetailsMetadataEnricher.Enrich(context, problemDetails);

        return context.Response.WriteAsJsonAsync(
            problemDetails,
            options: null,
            contentType: "application/problem+json",
            cancellationToken: cancellationToken);
    }
}
