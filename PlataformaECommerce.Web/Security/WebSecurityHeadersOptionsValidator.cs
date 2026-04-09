using Microsoft.Extensions.Options;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Web.Security;

/// <summary>
/// Valida la configuración de headers de seguridad y de Content Security Policy antes del arranque.
/// </summary>
internal sealed class WebSecurityHeadersOptionsValidator : IValidateOptions<WebSecurityHeadersOptions>
{
    public ValidateOptionsResult Validate(string? name, WebSecurityHeadersOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        List<string> failures = [];
        ValidateRequiredText(failures, options.PermissionsPolicy, $"{WebSecurityHeadersOptions.SectionName}:PermissionsPolicy");
        ValidateRequiredText(failures, options.ReferrerPolicy, $"{WebSecurityHeadersOptions.SectionName}:ReferrerPolicy");
        ValidateRequiredText(failures, options.FrameOptions, $"{WebSecurityHeadersOptions.SectionName}:FrameOptions");
        ValidateRequiredText(failures, options.ContentTypeOptions, $"{WebSecurityHeadersOptions.SectionName}:ContentTypeOptions");
        ValidateRequiredText(failures, options.CrossOriginOpenerPolicy, $"{WebSecurityHeadersOptions.SectionName}:CrossOriginOpenerPolicy");
        ValidateRequiredText(failures, options.CrossOriginResourcePolicy, $"{WebSecurityHeadersOptions.SectionName}:CrossOriginResourcePolicy");

        if (options.ContentSecurityPolicy is null)
        {
            failures.Add($"La configuración '{WebSecurityHeadersOptions.SectionName}:ContentSecurityPolicy' es obligatoria.");
        }
        else if (options.ContentSecurityPolicy.Enabled)
        {
            ValidateSourceList(failures, options.ContentSecurityPolicy.DefaultSources, $"{WebSecurityHeadersOptions.SectionName}:ContentSecurityPolicy:DefaultSources");
            ValidateSourceList(failures, options.ContentSecurityPolicy.BaseUriSources, $"{WebSecurityHeadersOptions.SectionName}:ContentSecurityPolicy:BaseUriSources");
            ValidateSourceList(failures, options.ContentSecurityPolicy.ObjectSources, $"{WebSecurityHeadersOptions.SectionName}:ContentSecurityPolicy:ObjectSources");
            ValidateSourceList(failures, options.ContentSecurityPolicy.FrameAncestorSources, $"{WebSecurityHeadersOptions.SectionName}:ContentSecurityPolicy:FrameAncestorSources");
            ValidateSourceList(failures, options.ContentSecurityPolicy.ImageSources, $"{WebSecurityHeadersOptions.SectionName}:ContentSecurityPolicy:ImageSources");
            ValidateSourceList(failures, options.ContentSecurityPolicy.StyleSources, $"{WebSecurityHeadersOptions.SectionName}:ContentSecurityPolicy:StyleSources");
            ValidateSourceList(failures, options.ContentSecurityPolicy.ScriptSources, $"{WebSecurityHeadersOptions.SectionName}:ContentSecurityPolicy:ScriptSources");
            ValidateSourceList(failures, options.ContentSecurityPolicy.FontSources, $"{WebSecurityHeadersOptions.SectionName}:ContentSecurityPolicy:FontSources");
            ValidateSourceList(failures, options.ContentSecurityPolicy.ConnectSources, $"{WebSecurityHeadersOptions.SectionName}:ContentSecurityPolicy:ConnectSources");
            ValidateSourceList(failures, options.ContentSecurityPolicy.FormActionSources, $"{WebSecurityHeadersOptions.SectionName}:ContentSecurityPolicy:FormActionSources");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static void ValidateRequiredText(List<string> failures, string? value, string optionPath)
    {
        ArgumentNullException.ThrowIfNull(failures);
        if (!string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        failures.Add($"La configuración '{optionPath}' es obligatoria.");
    }

    private static void ValidateSourceList(List<string> failures, IReadOnlyCollection<string>? sources, string optionPath)
    {
        ArgumentNullException.ThrowIfNull(failures);

        if (sources is null || sources.Count == 0)
        {
            failures.Add($"La configuración '{optionPath}' debe contener al menos una fuente permitida.");
            return;
        }

        if (sources.Any(source => string.IsNullOrWhiteSpace(source)))
        {
            failures.Add($"La configuración '{optionPath}' no puede contener fuentes vacías.");
        }
    }
}
