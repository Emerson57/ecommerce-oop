using FluentValidation;
using PlataformaECommerce.Application.Features.Admin.Commands;

namespace PlataformaECommerce.Application.Features.Admin.Validators;

/// <summary>
/// Validador de aplicación para el comando <see cref="RegisterAdminCommand"/>.
/// </summary>
/// <remarks>
/// Esta clase centraliza las reglas de validación de entrada necesarias
/// antes de ejecutar el caso de uso de registro de un administrador.
///
/// Su responsabilidad es proteger la capa Application frente a solicitudes
/// incompletas, inconsistentes o mal formadas, permitiendo que el handler
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
    #region Constantes de validación

    /// <summary>
    /// Longitud mínima permitida para el nombre del administrador.
    /// </summary>
    private const int MinNameLength = 3;

    /// <summary>
    /// Longitud máxima permitida para el nombre del administrador.
    /// </summary>
    private const int MaxNameLength = 100;

    /// <summary>
    /// Longitud máxima permitida para el correo electrónico.
    /// </summary>
    private const int MaxEmailLength = 256;

    /// <summary>
    /// Longitud mínima permitida para la contraseña.
    /// </summary>
    private const int MinPasswordLength = 8;

    /// <summary>
    /// Longitud máxima permitida para la contraseña.
    /// </summary>
    private const int MaxPasswordLength = 100;

    /// <summary>
    /// Longitud mínima permitida para el área organizacional.
    /// </summary>
    private const int MinAreaLength = 3;

    /// <summary>
    /// Longitud máxima permitida para el área organizacional.
    /// </summary>
    private const int MaxAreaLength = 60;

    /// <summary>
    /// Longitud máxima permitida para la dirección IP.
    /// </summary>
    private const int MaxIpAddressLength = 64;

    /// <summary>
    /// Longitud máxima permitida para el canal de origen.
    /// </summary>
    private const int MaxSourceLength = 50;

    /// <summary>
    /// Longitud máxima permitida para la referencia externa.
    /// </summary>
    private const int MaxExternalReferenceLength = 100;

    /// <summary>
    /// Longitud máxima permitida para el motivo funcional del registro.
    /// </summary>
    private const int MaxReasonLength = 300;

    #endregion

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
            .MinimumLength(MinNameLength)
                .WithMessage($"El nombre del administrador debe tener al menos {MinNameLength} caracteres.")
            .MaximumLength(MaxNameLength)
                .WithMessage($"El nombre del administrador no puede superar los {MaxNameLength} caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty()
                .WithMessage("El correo electrónico del administrador es obligatorio.")
            .MaximumLength(MaxEmailLength)
                .WithMessage($"El correo electrónico del administrador no puede superar los {MaxEmailLength} caracteres.")
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
            .MinimumLength(MinPasswordLength)
                .WithMessage($"La contraseña debe tener al menos {MinPasswordLength} caracteres.")
            .MaximumLength(MaxPasswordLength)
                .WithMessage($"La contraseña no puede superar los {MaxPasswordLength} caracteres.")
            .Matches(@"[A-Z]")
                .WithMessage("La contraseña debe contener al menos una letra mayúscula.")
            .Matches(@"[a-z]")
                .WithMessage("La contraseña debe contener al menos una letra minúscula.")
            .Matches(@"\d")
                .WithMessage("La contraseña debe contener al menos un número.")
            .Matches(@"[^a-zA-Z0-9]")
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
            .MinimumLength(MinAreaLength)
                .WithMessage($"El área del administrador debe tener al menos {MinAreaLength} caracteres.")
            .MaximumLength(MaxAreaLength)
                .WithMessage($"El área del administrador no puede superar los {MaxAreaLength} caracteres.");

        RuleFor(x => x.RequestedByUserId)
            .Must(id => !id.HasValue || id.Value != Guid.Empty)
                .WithMessage("El identificador del usuario solicitante no puede ser un valor vacío.");
    }

    /// <summary>
    /// Configura las reglas relacionadas con el contexto y trazabilidad del registro.
    /// </summary>
    private void ConfigureContextRules()
    {
        RuleFor(x => x.IpAddress)
            .MaximumLength(MaxIpAddressLength)
                .WithMessage($"La dirección IP no puede superar los {MaxIpAddressLength} caracteres.")
            .Must(BeAValidIpAddress)
                .When(x => !string.IsNullOrWhiteSpace(x.IpAddress))
                .WithMessage("La dirección IP informada no es válida.");

        RuleFor(x => x.Source)
            .MaximumLength(MaxSourceLength)
                .WithMessage($"El canal de origen no puede superar los {MaxSourceLength} caracteres.");

        RuleFor(x => x.ExternalReference)
            .MaximumLength(MaxExternalReferenceLength)
                .WithMessage($"La referencia externa no puede superar los {MaxExternalReferenceLength} caracteres.");

        RuleFor(x => x.Reason)
            .MaximumLength(MaxReasonLength)
                .WithMessage($"El motivo funcional no puede superar los {MaxReasonLength} caracteres.");
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