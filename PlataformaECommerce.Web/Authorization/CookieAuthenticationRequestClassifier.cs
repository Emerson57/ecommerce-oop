using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace PlataformaECommerce.Web.Authorization;

internal static class CookieAuthenticationRequestClassifier
{
    public static bool ShouldReturnStatusCode(HttpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase)
            || HasJsonAcceptHeader(request.Headers.Accept)
            || IsXmlHttpRequest(request.Headers["X-Requested-With"]);
    }

    private static bool HasJsonAcceptHeader(StringValues values)
    {
        foreach (string? value in values)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            if (value.Contains("application/json", StringComparison.OrdinalIgnoreCase)
                || value.Contains("application/problem+json", StringComparison.OrdinalIgnoreCase)
                || value.Contains("text/json", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsXmlHttpRequest(StringValues values)
    {
        foreach (string? value in values)
        {
            if (string.Equals(value, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
