using Microsoft.Extensions.Hosting;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Web.Extensions.Startup;

internal static class ForwardedHeadersSecurityOptionsValidator
{
    public static bool IsValid(ForwardedHeadersSecurityOptions options, IHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(hostEnvironment);

        if (!options.Enabled)
        {
            return true;
        }

        if (options.ForwardLimit <= 0)
        {
            return false;
        }

        bool hasTrustedProxies = options.TrustedProxies.Count > 0;
        bool hasTrustedNetworks = options.TrustedNetworks.Count > 0;
        if (!hasTrustedProxies && !hasTrustedNetworks)
        {
            return false;
        }

        bool proxiesAreValid = options.TrustedProxies.All(proxy =>
            ForwardedHeadersConfigurationParser.TryParseProxy(proxy, out _));
        if (!proxiesAreValid)
        {
            return false;
        }

        if (options.TrustForwardedHost)
        {
            if (options.AllowedHosts.Count == 0)
            {
                return false;
            }

            bool allowedHostsAreValid = options.AllowedHosts.All(IsValidAllowedHost);
            if (!allowedHostsAreValid)
            {
                return false;
            }
        }

        return options.TrustedNetworks.All(network =>
            ForwardedHeadersConfigurationParser.TryParseNetwork(network, out _));
    }

    public static string BuildValidationMessage(IHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(hostEnvironment);

        return hostEnvironment.IsDevelopment()
            ? "La configuración de ForwardedHeadersSecurity es inválida. Cuando Enabled=true debes definir proxies o redes confiables válidas para el entorno local y, si habilitas TrustForwardedHost, también AllowedHosts válidos."
            : "La configuración de ForwardedHeadersSecurity es inválida. En entornos no locales, cuando Enabled=true debes definir explícitamente proxies o redes confiables válidas antes de confiar en headers reenviados y, si habilitas TrustForwardedHost, también AllowedHosts válidos.";
    }

    private static bool IsValidAllowedHost(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string candidate = value.Trim();
        if (candidate.Contains("://", StringComparison.Ordinal)
            || candidate.Contains('/', StringComparison.Ordinal)
            || candidate.Contains('\\', StringComparison.Ordinal)
            || candidate.Contains(':', StringComparison.Ordinal))
        {
            return false;
        }

        return Uri.CheckHostName(candidate) == UriHostNameType.Dns;
    }
}
