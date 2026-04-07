using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Web.Extensions.Startup;

internal static class RateLimitingOptionsValidator
{
    public static bool AreValid(WebRateLimitingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return HasValidPolicy(options.AuthFlow)
            && HasValidPolicy(options.SensitiveApi)
            && HasValidPolicy(options.PublicApi);
    }

    private static bool HasValidPolicy(WebRateLimitingOptions.FixedWindowPolicyOptions? options)
    {
        return options is not null
            && options.PermitLimit > 0
            && options.WindowSeconds > 0
            && options.QueueLimit >= 0;
    }
}
