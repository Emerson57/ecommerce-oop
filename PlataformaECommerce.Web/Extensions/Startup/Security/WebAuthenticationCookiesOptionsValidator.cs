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

        ValidateCookieProfile(failures, "Administrative", options.Administrative);
        ValidateCookieProfile(failures, "Customer", options.Customer);

        if (!options.HttpOnly)
        {
            failures.Add($"La configuración '{WebAuthenticationCookiesOptions.SectionName}:HttpOnly' debe permanecer habilitada para cookies autenticadas.");
        }

        if (options.SecurePolicy != Microsoft.AspNetCore.Http.CookieSecurePolicy.Always)
        {
            failures.Add($"La configuración '{WebAuthenticationCookiesOptions.SectionName}:SecurePolicy' debe ser 'Always' para evitar transporte inseguro de credenciales.");
        }

        if (options.SameSite == Microsoft.AspNetCore.Http.SameSiteMode.Unspecified)
        {
            failures.Add($"La configuración '{WebAuthenticationCookiesOptions.SectionName}:SameSite' debe ser explícita para evitar comportamiento ambiguo entre navegadores.");
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

    private static void ValidateCookieProfile(
        ICollection<string> failures,
        string profileName,
        WebAuthenticationCookieProfileOptions? profile)
    {
        if (profile is null)
        {
            failures.Add($"La configuración '{WebAuthenticationCookiesOptions.SectionName}:{profileName}' es obligatoria.");
            return;
        }

        if (profile.SessionIdleTimeoutMinutes < 5)
        {
            failures.Add($"La configuración '{WebAuthenticationCookiesOptions.SectionName}:{profileName}:SessionIdleTimeoutMinutes' debe ser igual o mayor a 5.");
        }

        if (profile.PersistentSessionAbsoluteLifetimeHours * 60 < profile.SessionIdleTimeoutMinutes)
        {
            failures.Add($"La configuración '{WebAuthenticationCookiesOptions.SectionName}:{profileName}:PersistentSessionAbsoluteLifetimeHours' debe ser mayor o igual que la expiración por inactividad.");
        }
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
