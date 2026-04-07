using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Web.Extensions.Startup;

internal static class RateLimitPolicyBuilder
{
    public static void AddFixedWindowPolicy(
        RateLimiterOptions options,
        string policyName,
        WebRateLimitingOptions.FixedWindowPolicyOptions policy)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(policy);

        options.AddPolicy(policyName, httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: RateLimitPartitionKeyResolver.Resolve(httpContext),
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = policy.PermitLimit,
                    Window = TimeSpan.FromSeconds(policy.WindowSeconds),
                    QueueLimit = policy.QueueLimit,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    AutoReplenishment = true
                }));
    }
}
