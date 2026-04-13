using Microsoft.Extensions.Options;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Valida la configuración antiforgery requerida por la aplicación web.
/// </summary>
internal sealed class WebAntiforgeryOptionsValidator : IValidateOptions<WebAntiforgeryOptions>
{
    public ValidateOptionsResult Validate(string? name, WebAntiforgeryOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        List<string> failures = [];
        ValidateRequiredValue(failures, options.CookieName, $"{WebAntiforgeryOptions.SectionName}:CookieName");
        ValidateRequiredValue(failures, options.FormFieldName, $"{WebAntiforgeryOptions.SectionName}:FormFieldName");
        ValidateRequiredValue(failures, options.HeaderName, $"{WebAntiforgeryOptions.SectionName}:HeaderName");

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static void ValidateRequiredValue(ICollection<string> failures, string? value, string optionPath)
    {
        ArgumentNullException.ThrowIfNull(failures);

        if (!string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        failures.Add($"La configuración '{optionPath}' es obligatoria.");
    }
}
