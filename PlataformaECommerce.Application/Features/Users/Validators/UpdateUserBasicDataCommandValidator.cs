using FluentValidation;
using PlataformaECommerce.Application.Features.Users.Commands;

namespace PlataformaECommerce.Application.Features.Users.Validators;

/// <summary>
/// Validador de aplicación para el comando <see cref="UpdateUserBasicDataCommand"/>.
/// </summary>
/// <remarks>
/// Esta clase centraliza las reglas de validación de entrada necesarias
/// antes de ejecutar el caso de uso de actualización de los datos básicos
/// de un usuario existente.
///
/// Su responsabilidad es proteger la capa Application frente a solicitudes
/// incompletas, inconsistentes o mal formadas, permitiendo que el servicio de aplicación
/// reciba un comando previamente saneado desde el punto de vista estructural.
///
/// Las validaciones aquí definidas no reemplazan las reglas del dominio,
/// sino que actúan como una primera barrera de entrada para:
/// - endpoints HTTP,
/// - servicios de aplicación,
/// - paneles administrativos,
/// - procesos internos,
/// - integraciones externas.
///
/// Este validador está orientado específicamente a la modificación
/// del nombre y correo electrónico del usuario, además de validar
/// los metadatos opcionales asociados a trazabilidad y observabilidad.
/// </remarks>
public sealed class UpdateUserBasicDataCommandValidator : AbstractValidator<UpdateUserBasicDataCommand>
{
    #region Constantes de validación

    /// <summary>
    /// Longitud mínima permitida para el nombre del usuario.
    /// </summary>
    private const int MinNameLength = 3;

    /// <summary>
    /// Longitud máxima permitida para el nombre del usuario.
    /// </summary>
    private const int MaxNameLength = 100;

    /// <summary>
    /// Longitud máxima permitida para el correo electrónico.
    /// </summary>
    private const int MaxEmailLength = 256;

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
    /// Longitud máxima permitida para el motivo funcional.
    /// </summary>
    private const int MaxReasonLength = 300;

    #endregion

    #region Constructor

    /// <summary>
    /// Inicializa una nueva instancia del validador
    /// <see cref="UpdateUserBasicDataCommandValidator"/>.
    /// </summary>
    public UpdateUserBasicDataCommandValidator()
    {
        ConfigureIdentityRules();
        ConfigureBasicDataRules();
        ConfigureContextRules();
    }

    #endregion

    #region Métodos privados de configuración

    /// <summary>
    /// Configura las reglas relacionadas con la identificación del usuario
    /// y del solicitante de la operación.
    /// </summary>
    private void ConfigureIdentityRules()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
                .WithMessage("El identificador del usuario es obligatorio.");

        RuleFor(x => x.RequestedByUserId)
            .Must(id => !id.HasValue || id.Value != Guid.Empty)
                .WithMessage("El identificador del usuario solicitante no puede ser un valor vacío.");
    }

    /// <summary>
    /// Configura las reglas relacionadas con los datos básicos del usuario.
    /// </summary>
    private void ConfigureBasicDataRules()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithMessage("El nombre del usuario es obligatorio.")
            .MinimumLength(MinNameLength)
                .WithMessage($"El nombre del usuario debe tener al menos {MinNameLength} caracteres.")
            .MaximumLength(MaxNameLength)
                .WithMessage($"El nombre del usuario no puede superar los {MaxNameLength} caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty()
                .WithMessage("El correo electrónico del usuario es obligatorio.")
            .MaximumLength(MaxEmailLength)
                .WithMessage($"El correo electrónico del usuario no puede superar los {MaxEmailLength} caracteres.")
            .EmailAddress()
                .WithMessage("El correo electrónico del usuario no tiene un formato válido.");
    }

    /// <summary>
    /// Configura las reglas relacionadas con el contexto y trazabilidad de la solicitud.
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