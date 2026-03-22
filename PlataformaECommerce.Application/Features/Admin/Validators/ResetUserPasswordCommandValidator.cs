using FluentValidation;
using PlataformaECommerce.Application.Common.Security;
using PlataformaECommerce.Application.Features.Admin.Commands;

namespace PlataformaECommerce.Application.Features.Admin.Validators;

/// <summary>
/// Valida la entrada estructural del restablecimiento administrativo de contraseña.
/// </summary>
/// <remarks>
/// Este validador protege el caso de uso del backoffice asegurando consistencia mínima
/// sobre el usuario objetivo, la nueva credencial y los metadatos de trazabilidad.
/// </remarks>
public sealed class ResetUserPasswordCommandValidator : AbstractValidator<ResetUserPasswordCommand>
{
    /// <summary>
    /// Inicializa una nueva instancia del validador.
    /// </summary>
    public ResetUserPasswordCommandValidator()
    {
        ConfigureTargetRules();
        ConfigureCredentialRules();
        ConfigureContextRules();
    }

    /// <summary>
    /// Configura las reglas del usuario objetivo y del solicitante.
    /// </summary>
    private void ConfigureTargetRules()
    {
        RuleFor(command => command.TargetUserId)
            .NotEmpty()
            .WithMessage("El usuario objetivo es obligatorio.");

        RuleFor(command => command.RequestedByUserId)
            .Must(value => !value.HasValue || value.Value != Guid.Empty)
            .WithMessage("El identificador del usuario solicitante no puede ser un valor vacío.");
    }

    /// <summary>
    /// Configura las reglas de la nueva contraseña.
    /// </summary>
    private void ConfigureCredentialRules()
    {
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
                .WithMessage("La nueva contraseña debe contener al menos un carácter especial.");

        RuleFor(command => command.ConfirmPassword)
            .NotEmpty()
                .WithMessage("La confirmación de la nueva contraseña es obligatoria.")
            .Equal(command => command.NewPassword)
                .WithMessage("La confirmación de la nueva contraseña no coincide con la contraseña ingresada.");
    }

    /// <summary>
    /// Configura las reglas de trazabilidad de la operación.
    /// </summary>
    private void ConfigureContextRules()
    {
        RuleFor(command => command.IpAddress)
            .MaximumLength(AdminRegistrationPolicies.MaxIpAddressLength)
                .WithMessage($"La dirección IP no puede superar los {AdminRegistrationPolicies.MaxIpAddressLength} caracteres.")
            .Must(BeAValidIpAddress)
                .When(command => !string.IsNullOrWhiteSpace(command.IpAddress))
                .WithMessage("La dirección IP informada no es válida.");

        RuleFor(command => command.Source)
            .MaximumLength(AdminRegistrationPolicies.MaxSourceLength)
                .WithMessage($"El canal de origen no puede superar los {AdminRegistrationPolicies.MaxSourceLength} caracteres.");

        RuleFor(command => command.ExternalReference)
            .MaximumLength(AdminRegistrationPolicies.MaxExternalReferenceLength)
                .WithMessage($"La referencia externa no puede superar los {AdminRegistrationPolicies.MaxExternalReferenceLength} caracteres.");

        RuleFor(command => command.Reason)
            .MaximumLength(AdminRegistrationPolicies.MaxReasonLength)
                .WithMessage($"El motivo funcional no puede superar los {AdminRegistrationPolicies.MaxReasonLength} caracteres.");
    }

    /// <summary>
    /// Determina si el valor suministrado corresponde a una dirección IP válida.
    /// </summary>
    /// <param name="value">Valor a validar.</param>
    /// <returns><see langword="true"/> cuando el valor es nulo, vacío o una IP válida.</returns>
    private static bool BeAValidIpAddress(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        return System.Net.IPAddress.TryParse(value.Trim(), out _);
    }
}
