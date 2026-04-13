using System.Text.RegularExpressions;

namespace PlataformaECommerce.Web.Extensions.Startup;

internal static class BrandingHexColorValidator
{
    private static readonly Regex HexColorRegex = new("^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6})$", RegexOptions.Compiled, TimeSpan.FromSeconds(1));

    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return HexColorRegex.IsMatch(value.Trim());
    }
}
