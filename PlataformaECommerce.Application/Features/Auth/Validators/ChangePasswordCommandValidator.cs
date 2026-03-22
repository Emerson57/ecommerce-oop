using FluentValidation;
using PlataformaECommerce.Application.Common.Security;
using PlataformaECommerce.Application.Features.Auth.Commands;

namespace PlataformaECommerce.Application.Features.Auth.Validators;

/// <summary>
/// Valida la entrada estructural del cambio autenticado de contraseña.
/// </summary>
public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    private const int IpAddressMaxLength = 64;
    private const int UserAgentMaxLength = 512;
    private const int SourceMaxLength = 50;
    private const int ExternalReferenceMaxLength = 150;

    /// <summary>
    /// Inicializa una nueva instancia del validador.
    /// </summary>
    public ChangePasswordCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("El identificador del usuario es obligatorio.");

        RuleFor(command => command.CurrentPassword)
            .NotEmpty()
            .WithMessage("La contraseña actual es obligatoria.");

        RuleFor(command => command.NewPassword)
            .NotEmpty()
                .WithMessage("La nueva contraseña es obligatoria.")
            .MinimumLength(PasswordPolicyRules.MinLength)
                .WithMessage($"La nueva contraseña debe tener al menos {PasswordPolicyRules.MinLength} caracteres.")
            .MaximumLength(PasswordPolicyRules.MaxLength)
                .WithMessage($"La nueva contraseña no puede superar los {PasswordPolicyRules.MaxLength} caracteres.")
            .Must(PasswordPolicyRules.HasUppercase)
                .WithMessage("La nueva contraseña debe contener al menos una letra mayúscula.")
            .Must(PasswordPolicyRules.HasLowercase)
                .WithMessage("La nueva contraseña debe contener al menos una letra minúscula.")
            .Must(PasswordPolicyRules.HasDigit)
                .WithMessage("La nueva contraseña debe contener al menos un número.")
            .Must(PasswordPolicyRules.HasSpecialCharacter)
                .WithMessage("La nueva contraseña debe contener al menos un carácter especial.")
            .NotEqual(command => command.CurrentPassword)
                .WithMessage("La nueva contraseña debe ser diferente de la contraseña actual.");

        RuleFor(command => command.ConfirmPassword)
            .NotEmpty()
                .WithMessage("La confirmación de la nueva contraseña es obligatoria.")
            .Equal(command => command.NewPassword)
                .WithMessage("La confirmación de la nueva contraseña no coincide con la contraseña ingresada.");

        RuleFor(command => command.IpAddress)
            .MaximumLength(IpAddressMaxLength)
                .WithMessage($"La dirección IP no puede superar los {IpAddressMaxLength} caracteres.")
            .Must(BeNullOrContainMeaningfulContent)
                .WithMessage("Si se informa la dirección IP, esta no puede contener únicamente espacios en blanco.");

        RuleFor(command => command.UserAgent)
            .MaximumLength(UserAgentMaxLength)
                .WithMessage($"El User-Agent no puede superar los {UserAgentMaxLength} caracteres.")
            .Must(BeNullOrContainMeaningfulContent)
                .WithMessage("Si se informa el User-Agent, este no puede contener únicamente espacios en blanco.");

        RuleFor(command => command.Source)
            .MaximumLength(SourceMaxLength)
                .WithMessage($"El canal de origen no puede superar los {SourceMaxLength} caracteres.")
            .Must(BeNullOrContainMeaningfulContent)
                .WithMessage("Si se informa el canal de origen, este no puede contener únicamente espacios en blanco.");

        RuleFor(command => command.ExternalReference)
            .MaximumLength(ExternalReferenceMaxLength)
                .WithMessage($"La referencia externa no puede superar los {ExternalReferenceMaxLength} caracteres.")
            .Must(BeNullOrContainMeaningfulContent)
                .WithMessage("Si se informa la referencia externa, esta no puede contener únicamente espacios en blanco.");

        RuleFor(command => command.RequestedAtUtc)
            .Must(BeNullOrUtcDate)
            .WithMessage("La fecha de solicitud debe estar expresada en UTC cuando sea informada.");
    }

    private static bool BeNullOrContainMeaningfulContent(string? value)
    {
        return value is null || !string.IsNullOrWhiteSpace(value);
    }

    private static bool BeNullOrUtcDate(DateTime? value)
    {
        return !value.HasValue || value.Value.Kind == DateTimeKind.Utc;
    }
}
