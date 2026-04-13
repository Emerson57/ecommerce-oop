using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Web.Extensions.Startup;

internal static class RateLimitPolicyBuilder
{
    public static void AddFixedWindowPolicy(
        RateLimiterOptions options,
        string policyName,
        RateLimitingOptions.FixedWindowPolicyOptions policy)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(policy);

        options.AddPolicy(policyName, httpContext =>
        {
            RateLimitPartitionKeyResolver partitionKeyResolver = httpContext.RequestServices.GetRequiredService<RateLimitPartitionKeyResolver>();
            string partitionKey = partitionKeyResolver.Resolve(httpContext, policyName);

            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: partitionKey,
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = policy.PermitLimit,
                    Window = TimeSpan.FromSeconds(policy.WindowSeconds),
                    QueueLimit = policy.QueueLimit,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    AutoReplenishment = true
                });
        });
    }
}
