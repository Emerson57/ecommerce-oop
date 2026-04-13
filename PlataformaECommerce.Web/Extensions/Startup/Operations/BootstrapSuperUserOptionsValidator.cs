using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Valida la configuración de bootstrap del super usuario y emite mensajes explícitos con la ruta de configuración afectada.
/// </summary>
internal sealed class BootstrapSuperUserOptionsValidator : IValidateOptions<BootstrapSuperUserOptions>
{
    public ValidateOptionsResult Validate(string? name, BootstrapSuperUserOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        ValidationContext validationContext = new(options);
        List<ValidationResult> validationResults = [];
        bool isValid = Validator.TryValidateObject(options, validationContext, validationResults, validateAllProperties: true);

        if (isValid)
        {
            return ValidateOptionsResult.Success;
        }

        string[] failures = validationResults
            .Select(result => BuildFailureMessage(result))
            .ToArray();

        return ValidateOptionsResult.Fail(failures);
    }

    private static string BuildFailureMessage(ValidationResult validationResult)
    {
        ArgumentNullException.ThrowIfNull(validationResult);

        string memberPath = validationResult.MemberNames.Any()
            ? string.Join(", ", validationResult.MemberNames.Select(memberName => $"{BootstrapSuperUserOptions.SectionName}:{memberName}"))
            : BootstrapSuperUserOptions.SectionName;

        string message = string.IsNullOrWhiteSpace(validationResult.ErrorMessage)
            ? "La configuración es inválida."
            : validationResult.ErrorMessage;

        return $"La configuración '{memberPath}' es inválida. {message}";
    }
}
