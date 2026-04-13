using System.Net;
using System.Net.Sockets;
namespace PlataformaECommerce.Web.Extensions.Startup;

internal static class ForwardedHeadersConfigurationParser
{
    public static bool TryParseProxy(string? value, out IPAddress? proxyAddress)
    {
        proxyAddress = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return IPAddress.TryParse(value.Trim(), out proxyAddress);
    }

    public static bool TryParseNetwork(string? value, out Microsoft.AspNetCore.HttpOverrides.IPNetwork network)
    {
        network = null!;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string[] parts = value.Trim().Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 || !IPAddress.TryParse(parts[0], out IPAddress? prefix) || !int.TryParse(parts[1], out int prefixLength))
        {
            return false;
        }

        int maxPrefixLength = prefix.AddressFamily switch
        {
            AddressFamily.InterNetwork => 32,
            AddressFamily.InterNetworkV6 => 128,
            _ => 0
        };

        if (maxPrefixLength == 0 || prefixLength < 0 || prefixLength > maxPrefixLength)
        {
            return false;
        }

        network = new Microsoft.AspNetCore.HttpOverrides.IPNetwork(prefix, prefixLength);
        return true;
    }
}
