using FluentValidation;
using PlataformaECommerce.Application.Features.Auth.Commands;

namespace PlataformaECommerce.Application.Features.Auth.Validators;

/// <summary>
/// Define las reglas de validación para el comando <see cref="LoginCommand"/>.
/// </summary>
/// <remarks>
/// Este validador garantiza que la solicitud de autenticación contenga
/// la información estructural mínima necesaria para ser procesada
/// por la capa Application.
///
/// Las reglas aquí definidas se enfocan en:
/// - obligatoriedad de credenciales,
/// - calidad mínima de la entrada,
/// - control de longitudes,
/// - consistencia básica de metadatos,
/// - y trazabilidad temporal.
///
/// Este componente no reemplaza las validaciones de seguridad reales,
/// tales como verificación de credenciales, bloqueo de cuenta, MFA
/// o políticas antifraude, las cuales deben ser resueltas por servicios
/// especializados de autenticación y seguridad.
/// </remarks>
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    #region Constantes de validación

    /// <summary>
    /// Longitud mínima permitida para el identificador de acceso.
    /// </summary>
    private const int EmailMinLength = 3;

    /// <summary>
    /// Longitud máxima permitida para el identificador de acceso.
    /// </summary>
    private const int EmailMaxLength = 150;

    /// <summary>
    /// Longitud mínima permitida para la contraseña.
    /// </summary>
    private const int PasswordMinLength = 6;

    /// <summary>
    /// Longitud máxima permitida para la contraseña.
    /// </summary>
    private const int PasswordMaxLength = 200;

    /// <summary>
    /// Longitud máxima permitida para la dirección IP.
    /// </summary>
    private const int IpAddressMaxLength = 64;

    /// <summary>
    /// Longitud máxima permitida para el User-Agent.
    /// </summary>
    private const int UserAgentMaxLength = 512;

    /// <summary>
    /// Longitud máxima permitida para el canal de origen.
    /// </summary>
    private const int SourceMaxLength = 50;

    /// <summary>
    /// Longitud máxima permitida para la referencia externa.
    /// </summary>
    private const int ExternalReferenceMaxLength = 150;

    #endregion

    #region Constructor

    /// <summary>
    /// Inicializa una nueva instancia del validador y configura
    /// todas las reglas aplicables al comando de inicio de sesión.
    /// </summary>
    public LoginCommandValidator()
    {
        ConfigureCredentialRules();
        ConfigureTraceabilityRules();
        ConfigureDateRules();
    }

    #endregion

    #region Configuración de reglas

    /// <summary>
    /// Configura las reglas relacionadas con credenciales de acceso.
    /// </summary>
    private void ConfigureCredentialRules()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .WithMessage("El correo electrónico es obligatorio.")
            .Must(ContainMeaningfulContent)
            .WithMessage("El correo electrónico no puede contener únicamente espacios en blanco.")
            .EmailAddress()
            .WithMessage("El correo electrónico no tiene un formato válido.")
            .MinimumLength(EmailMinLength)
            .WithMessage($"El correo electrónico debe tener al menos {EmailMinLength} caracteres.")
            .MaximumLength(EmailMaxLength)
            .WithMessage($"El correo electrónico no puede superar los {EmailMaxLength} caracteres.");

        RuleFor(command => command.Password)
            .NotEmpty()
            .WithMessage("La contraseña es obligatoria.")
            .Must(ContainMeaningfulContent)
            .WithMessage("La contraseña no puede contener únicamente espacios en blanco.")
            .MinimumLength(PasswordMinLength)
            .WithMessage($"La contraseña debe tener al menos {PasswordMinLength} caracteres.")
            .MaximumLength(PasswordMaxLength)
            .WithMessage($"La contraseña no puede superar los {PasswordMaxLength} caracteres.");
    }

    /// <summary>
    /// Configura las reglas relacionadas con metadatos de trazabilidad.
    /// </summary>
    private void ConfigureTraceabilityRules()
    {
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
    }

    /// <summary>
    /// Configura las reglas relacionadas con trazabilidad temporal.
    /// </summary>
    private void ConfigureDateRules()
    {
        RuleFor(command => command.RequestedAtUtc)
            .Must(BeNullOrUtcDate)
            .WithMessage("La fecha de solicitud debe estar expresada en UTC cuando sea informada.");
    }

    #endregion

    #region Métodos auxiliares

    /// <summary>
    /// Indica si un texto contiene contenido significativo.
    /// </summary>
    /// <param name="value">Valor a evaluar.</param>
    /// <returns>
    /// <see langword="true"/> si contiene información útil;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    private static bool ContainMeaningfulContent(string value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Indica si un texto es nulo o contiene contenido significativo.
    /// </summary>
    /// <param name="value">Valor a evaluar.</param>
    /// <returns>
    /// <see langword="true"/> si es nulo o contiene información útil;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    private static bool BeNullOrContainMeaningfulContent(string? value)
    {
        return value is null || !string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Indica si una fecha opcional es nula o está expresada en UTC.
    /// </summary>
    /// <param name="value">Fecha a evaluar.</param>
    /// <returns>
    /// <see langword="true"/> si el valor es nulo o corresponde a UTC;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    private static bool BeNullOrUtcDate(DateTime? value)
    {
        return !value.HasValue || value.Value.Kind == DateTimeKind.Utc;
    }

    #endregion
}