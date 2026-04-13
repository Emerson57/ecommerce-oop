using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Web.Extensions.Startup;

internal static class RateLimitingOptionsValidator
{
    public static bool AreValid(RateLimitingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return HasValidPolicy(options.Authentication)
            && HasValidPolicy(options.PublicApi)
            && HasValidPolicy(options.Administration)
            && HasValidPolicy(options.SensitiveEndpoints);
    }

    private static bool HasValidPolicy(RateLimitingOptions.FixedWindowPolicyOptions? options)
    {
        return options is not null
            && options.PermitLimit > 0
            && options.WindowSeconds > 0
            && options.QueueLimit >= 0;
    }
}
