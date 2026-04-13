using Microsoft.Extensions.Options;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Valida la configuración endurecida de cookies autenticadas antes del arranque.
/// </summary>
internal sealed class WebAuthenticationCookiesOptionsValidator : IValidateOptions<WebAuthenticationCookiesOptions>
{
    public ValidateOptionsResult Validate(string? name, WebAuthenticationCookiesOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        List<string> failures = [];

        if (options.SessionIdleTimeoutMinutes < 5)
        {
            failures.Add($"La configuración '{WebAuthenticationCookiesOptions.SectionName}:SessionIdleTimeoutMinutes' debe ser igual o mayor a 5.");
        }

        if (options.PersistentSessionAbsoluteLifetimeHours * 60 < options.SessionIdleTimeoutMinutes)
        {
            failures.Add($"La configuración '{WebAuthenticationCookiesOptions.SectionName}:PersistentSessionAbsoluteLifetimeHours' debe ser mayor o igual que la expiración por inactividad.");
        }

        if (options.SameSite == Microsoft.AspNetCore.Http.SameSiteMode.None
            && options.SecurePolicy != Microsoft.AspNetCore.Http.CookieSecurePolicy.Always)
        {
            failures.Add($"La configuración '{WebAuthenticationCookiesOptions.SectionName}:SecurePolicy' debe ser 'Always' cuando SameSite es 'None'.");
        }

        if (!string.IsNullOrWhiteSpace(options.SharedCookieDomain)
            && !IsValidSharedCookieDomain(options.SharedCookieDomain))
        {
            failures.Add($"La configuración '{WebAuthenticationCookiesOptions.SectionName}:SharedCookieDomain' debe ser un dominio DNS compartible válido sin esquema, puerto ni rutas locales.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static bool IsValidSharedCookieDomain(string value)
    {
        string candidate = value.Trim();
        if (candidate.Length == 0)
        {
            return false;
        }

        if (candidate.Contains("://", StringComparison.Ordinal)
            || candidate.Contains('/', StringComparison.Ordinal)
            || candidate.Contains('\\', StringComparison.Ordinal)
            || candidate.Contains(':', StringComparison.Ordinal))
        {
            return false;
        }

        string normalizedHost = candidate.TrimStart('.');
        if (normalizedHost.Length == 0
            || string.Equals(normalizedHost, "localhost", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return Uri.CheckHostName(normalizedHost) == UriHostNameType.Dns;
    }
}
