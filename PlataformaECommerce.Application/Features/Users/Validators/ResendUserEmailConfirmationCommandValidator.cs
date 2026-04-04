using FluentValidation;
using PlataformaECommerce.Application.Features.Users;
using PlataformaECommerce.Application.Features.Users.Commands;

namespace PlataformaECommerce.Application.Features.Users.Validators;

/// <summary>
/// Validador de aplicación para el comando <see cref="ResendUserEmailConfirmationCommand"/>.
/// </summary>
public sealed class ResendUserEmailConfirmationCommandValidator : AbstractValidator<ResendUserEmailConfirmationCommand>
{
    /// <summary>
    /// Inicializa una nueva instancia del validador <see cref="ResendUserEmailConfirmationCommandValidator"/>.
    /// </summary>
    public ResendUserEmailConfirmationCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
                .WithMessage("El correo electrónico es obligatorio.")
            .MaximumLength(CustomerRegistrationPolicies.MaxEmailLength)
                .WithMessage($"El correo electrónico no puede superar los {CustomerRegistrationPolicies.MaxEmailLength} caracteres.")
            .EmailAddress()
                .WithMessage("El correo electrónico no tiene un formato válido.");

        RuleFor(x => x.EmailConfirmationUrl)
            .NotEmpty()
                .WithMessage("La URL de confirmación de correo es obligatoria.")
            .MaximumLength(2000)
                .WithMessage("La URL de confirmación de correo supera la longitud permitida.");

        RuleFor(x => x.IpAddress)
            .MaximumLength(CustomerRegistrationPolicies.MaxIpAddressLength)
                .WithMessage($"La dirección IP no puede superar los {CustomerRegistrationPolicies.MaxIpAddressLength} caracteres.")
            .Must(BeAValidIpAddress)
                .When(x => !string.IsNullOrWhiteSpace(x.IpAddress))
                .WithMessage("La dirección IP informada no es válida.");

        RuleFor(x => x.Source)
            .MaximumLength(CustomerRegistrationPolicies.MaxSourceLength)
                .WithMessage($"El canal de origen no puede superar los {CustomerRegistrationPolicies.MaxSourceLength} caracteres.");

        RuleFor(x => x.ExternalReference)
            .MaximumLength(CustomerRegistrationPolicies.MaxExternalReferenceLength)
                .WithMessage($"La referencia externa no puede superar los {CustomerRegistrationPolicies.MaxExternalReferenceLength} caracteres.");
    }

    private static bool BeAValidIpAddress(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        return System.Net.IPAddress.TryParse(value.Trim(), out _);
    }
}
