using FluentValidation;
using PlataformaECommerce.Application.Common.Security;
using PlataformaECommerce.Application.Features.Admin;
using PlataformaECommerce.Application.Features.Admin.Commands;
using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Application.Features.Admin.Validators;

/// <summary>
/// Validador de aplicación para el comando <see cref="RegisterAdminCommand"/>.
/// </summary>
/// <remarks>
/// Esta clase centraliza las reglas de validación de entrada necesarias
/// antes de ejecutar el caso de uso de registro de un administrador.
///
/// Su responsabilidad es proteger la capa Application frente a solicitudes
/// incompletas, inconsistentes o mal formadas, permitiendo que el servicio de aplicación
/// reciba un comando previamente saneado desde el punto de vista estructural.
///
/// Las validaciones aquí definidas no reemplazan las reglas del dominio,
/// sino que actúan como una primera barrera de entrada para:
/// - endpoints HTTP administrativos,
/// - servicios de aplicación,
/// - formularios internos de aprovisionamiento,
/// - procesos de onboarding organizacional,
/// - integraciones controladas.
///
/// Este validador está orientado específicamente al registro de administradores,
/// por lo que verifica consistencia mínima en identidad, credenciales,
/// contexto organizacional y metadatos del proceso de alta.
/// </remarks>
public sealed class RegisterAdminCommandValidator : AbstractValidator<RegisterAdminCommand>
{
    #region Constructor

    /// <summary>
    /// Inicializa una nueva instancia del validador
    /// <see cref="RegisterAdminCommandValidator"/>.
    /// </summary>
    public RegisterAdminCommandValidator()
    {
        ConfigureIdentityRules();
        ConfigureCredentialRules();
        ConfigureOrganizationalRules();
        ConfigureContextRules();
    }

    #endregion

    #region Métodos privados de configuración

    /// <summary>
    /// Configura las reglas relacionadas con la identidad básica del administrador.
    /// </summary>
    private void ConfigureIdentityRules()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithMessage("El nombre del administrador es obligatorio.")
            .MinimumLength(AdminRegistrationPolicies.MinNameLength)
                .WithMessage($"El nombre del administrador debe tener al menos {AdminRegistrationPolicies.MinNameLength} caracteres.")
            .MaximumLength(AdminRegistrationPolicies.MaxNameLength)
                .WithMessage($"El nombre del administrador no puede superar los {AdminRegistrationPolicies.MaxNameLength} caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty()
                .WithMessage("El correo electrónico del administrador es obligatorio.")
            .MaximumLength(AdminRegistrationPolicies.MaxEmailLength)
                .WithMessage($"El correo electrónico del administrador no puede superar los {AdminRegistrationPolicies.MaxEmailLength} caracteres.")
            .EmailAddress()
                .WithMessage("El correo electrónico del administrador no tiene un formato válido.");
    }

    /// <summary>
    /// Configura las reglas relacionadas con las credenciales del administrador.
    /// </summary>
    private void ConfigureCredentialRules()
    {
        RuleFor(x => x.Password)
            .NotEmpty()
                .WithMessage("La contraseña es obligatoria.")
            .MinimumLength(AdminRegistrationPolicies.MinPasswordLength)
                .WithMessage($"La contraseña debe tener al menos {AdminRegistrationPolicies.MinPasswordLength} caracteres.")
            .MaximumLength(AdminRegistrationPolicies.MaxPasswordLength)
                .WithMessage($"La contraseña no puede superar los {AdminRegistrationPolicies.MaxPasswordLength} caracteres.")
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
    /// Configura las reglas relacionadas con el contexto organizacional del administrador.
    /// </summary>
    private void ConfigureOrganizationalRules()
    {
        RuleFor(x => x.Area)
            .NotEmpty()
                .WithMessage("El área del administrador es obligatoria.")
            .MinimumLength(AdminRegistrationPolicies.MinAreaLength)
                .WithMessage($"El área del administrador debe tener al menos {AdminRegistrationPolicies.MinAreaLength} caracteres.")
            .MaximumLength(AdminRegistrationPolicies.MaxAreaLength)
                .WithMessage($"El área del administrador no puede superar los {AdminRegistrationPolicies.MaxAreaLength} caracteres.");

        RuleFor(x => x.Role)
            .Equal(RolUsuario.Administrador)
                .When(x => !x.IsBootstrap)
                .WithMessage("La creación administrativa desde el backoffice solo puede generar cuentas con rol Administrador.");

        RuleFor(x => x.RequestedByUserId)
            .Must(id => !id.HasValue || id.Value != Guid.Empty)
                .WithMessage("El identificador del usuario solicitante no puede ser un valor vacío.");

        RuleFor(x => x.Role)
            .Equal(RolUsuario.SuperUsuario)
                .When(x => x.IsBootstrap)
                .WithMessage("El bootstrap inicial solo puede crear una cuenta con rol SuperUsuario.");

        RuleFor(x => x.IsActive)
            .Equal(true)
                .When(x => x.IsBootstrap)
                .WithMessage("El bootstrap inicial debe crear una cuenta administrativa activa.");

        RuleFor(x => x.IsEmailConfirmed)
            .Equal(true)
                .When(x => x.IsBootstrap)
                .WithMessage("El bootstrap inicial debe crear una cuenta administrativa con correo confirmado.");
    }

    /// <summary>
    /// Configura las reglas relacionadas con el contexto y trazabilidad del registro.
    /// </summary>
    private void ConfigureContextRules()
    {
        RuleFor(x => x.IpAddress)
            .MaximumLength(AdminRegistrationPolicies.MaxIpAddressLength)
                .WithMessage($"La dirección IP no puede superar los {AdminRegistrationPolicies.MaxIpAddressLength} caracteres.")
            .Must(BeAValidIpAddress)
                .When(x => !string.IsNullOrWhiteSpace(x.IpAddress))
                .WithMessage("La dirección IP informada no es válida.");

        RuleFor(x => x.Source)
            .MaximumLength(AdminRegistrationPolicies.MaxSourceLength)
                .WithMessage($"El canal de origen no puede superar los {AdminRegistrationPolicies.MaxSourceLength} caracteres.");

        RuleFor(x => x.ExternalReference)
            .MaximumLength(AdminRegistrationPolicies.MaxExternalReferenceLength)
                .WithMessage($"La referencia externa no puede superar los {AdminRegistrationPolicies.MaxExternalReferenceLength} caracteres.");

        RuleFor(x => x.Reason)
            .MaximumLength(AdminRegistrationPolicies.MaxReasonLength)
                .WithMessage($"El motivo funcional no puede superar los {AdminRegistrationPolicies.MaxReasonLength} caracteres.");
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