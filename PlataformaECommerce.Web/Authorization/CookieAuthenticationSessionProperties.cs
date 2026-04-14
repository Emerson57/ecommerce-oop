using System.Globalization;
using Microsoft.AspNetCore.Authentication;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Web.Authorization;

internal static class CookieAuthenticationSessionProperties
{
    private const string AbsoluteExpirationItemKey = "auth:absolute-expiration-utc";

    public static AuthenticationProperties Create(
        WebAuthenticationCookieProfileOptions profile,
        bool isPersistent,
        bool allowRefresh)
    {
        ArgumentNullException.ThrowIfNull(profile);

        DateTimeOffset issuedUtc = DateTimeOffset.UtcNow;
        TimeSpan absoluteLifetime = isPersistent
            ? TimeSpan.FromHours(profile.PersistentSessionAbsoluteLifetimeHours)
            : TimeSpan.FromMinutes(profile.SessionIdleTimeoutMinutes);
        DateTimeOffset absoluteExpirationUtc = issuedUtc.Add(absoluteLifetime);

        AuthenticationProperties properties = new()
        {
            AllowRefresh = allowRefresh,
            IsPersistent = isPersistent,
            IssuedUtc = issuedUtc
        };

        if (isPersistent)
        {
            properties.ExpiresUtc = absoluteExpirationUtc;
        }

        properties.Items[AbsoluteExpirationItemKey] = absoluteExpirationUtc.ToString("O", CultureInfo.InvariantCulture);
        return properties;
    }

    public static bool HasValidAbsoluteLifetime(AuthenticationProperties? properties)
    {
        if (properties is null)
        {
            return false;
        }

        if (!properties.Items.TryGetValue(AbsoluteExpirationItemKey, out string? rawAbsoluteExpirationUtc)
            || string.IsNullOrWhiteSpace(rawAbsoluteExpirationUtc))
        {
            return false;
        }

        if (!DateTimeOffset.TryParse(
                rawAbsoluteExpirationUtc,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out DateTimeOffset absoluteExpirationUtc))
        {
            return false;
        }

        if (properties.ExpiresUtc is { } expiresUtc && expiresUtc <= DateTimeOffset.UtcNow)
        {
            return false;
        }

        return absoluteExpirationUtc > DateTimeOffset.UtcNow;
    }
}
