using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Web.Extensions.Startup;

internal sealed class ForwardedHeadersOptionsSetup : IConfigureOptions<ForwardedHeadersOptions>
{
    private readonly IOptions<ForwardedHeadersSecurityOptions> _securityOptions;

    public ForwardedHeadersOptionsSetup(IOptions<ForwardedHeadersSecurityOptions> securityOptions)
    {
        _securityOptions = securityOptions ?? throw new ArgumentNullException(nameof(securityOptions));
    }

    public void Configure(ForwardedHeadersOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        ForwardedHeadersSecurityOptions securityOptions = _securityOptions.Value;
        if (!securityOptions.Enabled)
        {
            return;
        }

        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.ForwardLimit = securityOptions.ForwardLimit;
        options.RequireHeaderSymmetry = securityOptions.RequireHeaderSymmetry;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();

        foreach (string trustedProxy in securityOptions.TrustedProxies)
        {
            if (ForwardedHeadersConfigurationParser.TryParseProxy(trustedProxy, out System.Net.IPAddress? proxyAddress))
            {
                options.KnownProxies.Add(proxyAddress);
            }
        }

        foreach (string trustedNetwork in securityOptions.TrustedNetworks)
        {
            if (ForwardedHeadersConfigurationParser.TryParseNetwork(trustedNetwork, out IPNetwork network))
            {
                options.KnownNetworks.Add(network);
            }
        }
    }
}
