using FluentValidation;
using PlataformaECommerce.Application.Common.Security;
using PlataformaECommerce.Application.Features.Users;
using PlataformaECommerce.Application.Features.Users.Commands;

namespace PlataformaECommerce.Application.Features.Users.Validators;

/// <summary>
/// Validador de aplicación para el comando <see cref="RegisterCustomerCommand"/>.
/// </summary>
/// <remarks>
/// Esta clase centraliza las reglas de validación de entrada necesarias
/// antes de ejecutar el caso de uso de registro de un cliente.
///
/// Su responsabilidad es proteger la capa Application frente a solicitudes
/// incompletas, inconsistentes o mal formadas, permitiendo que el servicio de aplicación
/// reciba un comando previamente saneado desde el punto de vista estructural.
///
/// Las validaciones aquí definidas no reemplazan las reglas del dominio,
/// sino que actúan como una primera barrera de entrada para:
/// - endpoints HTTP,
/// - servicios de aplicación,
/// - formularios de registro,
/// - procesos de onboarding,
/// - integraciones externas.
///
/// Este validador está orientado específicamente al registro de clientes,
/// por lo que verifica consistencia mínima en identidad, credenciales,
/// consentimiento y metadatos del proceso de alta.
/// </remarks>
public sealed class RegisterCustomerCommandValidator : AbstractValidator<RegisterCustomerCommand>
{
    #region Constantes de validación

    #endregion

    #region Constructor

    /// <summary>
    /// Inicializa una nueva instancia del validador
    /// <see cref="RegisterCustomerCommandValidator"/>.
    /// </summary>
    public RegisterCustomerCommandValidator()
    {
        ConfigureIdentityRules();
        ConfigureCredentialRules();
        ConfigurePreferenceRules();
        ConfigureConsentRules();
        ConfigureContextRules();
    }

    #endregion

    #region Métodos privados de configuración

    /// <summary>
    /// Configura las reglas relacionadas con la identidad básica del cliente.
    /// </summary>
    private void ConfigureIdentityRules()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithMessage("El nombre del cliente es obligatorio.")
            .MinimumLength(CustomerRegistrationPolicies.MinNameLength)
                .WithMessage($"El nombre del cliente debe tener al menos {CustomerRegistrationPolicies.MinNameLength} caracteres.")
            .MaximumLength(CustomerRegistrationPolicies.MaxNameLength)
                .WithMessage($"El nombre del cliente no puede superar los {CustomerRegistrationPolicies.MaxNameLength} caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty()
                .WithMessage("El correo electrónico del cliente es obligatorio.")
            .MaximumLength(CustomerRegistrationPolicies.MaxEmailLength)
                .WithMessage($"El correo electrónico del cliente no puede superar los {CustomerRegistrationPolicies.MaxEmailLength} caracteres.")
            .EmailAddress()
                .WithMessage("El correo electrónico del cliente no tiene un formato válido.");
    }

    /// <summary>
    /// Configura las reglas relacionadas con las credenciales del cliente.
    /// </summary>
    private void ConfigureCredentialRules()
    {
        RuleFor(x => x.Password)
            .NotEmpty()
                .WithMessage("La contraseña es obligatoria.")
            .MinimumLength(CustomerRegistrationPolicies.MinPasswordLength)
                .WithMessage($"La contraseña debe tener al menos {CustomerRegistrationPolicies.MinPasswordLength} caracteres.")
            .MaximumLength(CustomerRegistrationPolicies.MaxPasswordLength)
                .WithMessage($"La contraseña no puede superar los {CustomerRegistrationPolicies.MaxPasswordLength} caracteres.")
            .Must(PasswordPolicyRules.HasUppercase)
                .WithMessage("La contraseña debe contener al menos una letra mayúscula.")
            .Must(PasswordPolicyRules.HasLowercase)
                .WithMessage("La contraseña debe contener al menos una letra minúscula.")
            .Must(PasswordPolicyRules.HasDigit)
                .WithMessage("La contraseña debe contener al menos un número.")
            .Must(PasswordPolicyRules.HasSpecialCharacter)
                .WithMessage("La contraseña debe contener al menos un carácter especial.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
                .WithMessage("La confirmación de la contraseña es obligatoria.")
            .Equal(x => x.Password)
                .WithMessage("La confirmación de la contraseña no coincide con la contraseña ingresada.");
    }

    /// <summary>
    /// Configura las reglas relacionadas con las preferencias iniciales del cliente.
    /// </summary>
    private void ConfigurePreferenceRules()
    {
        RuleFor(x => x.Preferences)
            .Must(preferences => preferences is not null)
                .WithMessage("La colección de preferencias no puede ser nula.")
            .Must(preferences => preferences.Count <= CustomerRegistrationPolicies.MaxPreferencesCount)
                .WithMessage($"No es posible registrar más de {CustomerRegistrationPolicies.MaxPreferencesCount} preferencias iniciales.");

        RuleForEach(x => x.Preferences)
            .NotEmpty()
                .WithMessage("Las preferencias del cliente no pueden estar vacías.")
            .MinimumLength(CustomerRegistrationPolicies.MinPreferenceLength)
                .WithMessage($"Cada preferencia debe tener al menos {CustomerRegistrationPolicies.MinPreferenceLength} caracteres.")
            .MaximumLength(CustomerRegistrationPolicies.MaxPreferenceLength)
                .WithMessage($"Cada preferencia no puede superar los {CustomerRegistrationPolicies.MaxPreferenceLength} caracteres.");
    }

    /// <summary>
    /// Configura las reglas relacionadas con consentimientos obligatorios.
    /// </summary>
    private void ConfigureConsentRules()
    {
        RuleFor(x => x.AcceptTermsAndConditions)
            .Equal(true)
                .WithMessage("El cliente debe aceptar los términos y condiciones.");

        RuleFor(x => x.AcceptPrivacyPolicy)
            .Equal(true)
                .WithMessage("El cliente debe aceptar la política de tratamiento de datos personales.");
    }

    /// <summary>
    /// Configura las reglas relacionadas con el contexto y trazabilidad del registro.
    /// </summary>
    private void ConfigureContextRules()
    {
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

    #endregion

    #region Métodos privados auxiliares

    /// <summary>
    /// Determina si el valor suministrado corresponde a una dirección IP válida.
    /// </summary>
    /// <param name="value">Valor a validar.</param>
    /// <returns>
    /// <see langword="true"/> si el valor corresponde a una IP válida;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    private static bool BeAValidIpAddress(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        return System.Net.IPAddress.TryParse(value.Trim(), out _);
    }

    #endregion
}