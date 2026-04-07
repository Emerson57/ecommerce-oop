using System.Text.RegularExpressions;

namespace PlataformaECommerce.Web.Extensions.Startup;

internal static partial class HexColorValidator
{
    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return HexColorRegex().IsMatch(value.Trim());
    }

    [GeneratedRegex("^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6})$")]
    private static partial Regex HexColorRegex();
}
