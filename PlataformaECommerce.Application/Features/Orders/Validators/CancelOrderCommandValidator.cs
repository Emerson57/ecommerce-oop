using FluentValidation;
using PlataformaECommerce.Application.Features.Orders.Commands;

namespace PlataformaECommerce.Application.Features.Orders.Validators;

/// <summary>
/// Define las reglas de validación para el comando <see cref="CancelOrderCommand"/>.
/// </summary>
/// <remarks>
/// Este validador garantiza que la solicitud de cancelación de un pedido
/// cumpla con las condiciones estructurales mínimas requeridas por la capa
/// de aplicación antes de que el caso de uso sea procesado.
///
/// Las reglas aquí definidas aseguran:
/// - existencia del identificador del pedido,
/// - obligatoriedad y calidad del motivo de cancelación,
/// - consistencia de campos complementarios,
/// - validez básica de metadatos de trazabilidad,
/// - y control de longitudes máximas.
///
/// Las reglas funcionales definitivas relacionadas con la posibilidad real
/// de cancelar un pedido deben permanecer en el dominio, donde se valida el
/// estado de la entidad, sus invariantes y sus reglas de negocio.
/// </remarks>
public sealed class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    #region Constantes de validación

    /// <summary>
    /// Longitud mínima requerida para el motivo de cancelación.
    /// </summary>
    private const int ReasonMinLength = 5;

    /// <summary>
    /// Longitud máxima permitida para el motivo de cancelación.
    /// </summary>
    private const int ReasonMaxLength = 300;

    /// <summary>
    /// Longitud máxima permitida para las notas.
    /// </summary>
    private const int NotesMaxLength = 1000;

    /// <summary>
    /// Longitud máxima permitida para la referencia externa.
    /// </summary>
    private const int ExternalReferenceMaxLength = 150;

    /// <summary>
    /// Longitud máxima permitida para la dirección IP.
    /// </summary>
    private const int IpAddressMaxLength = 64;

    /// <summary>
    /// Longitud máxima permitida para el canal de origen.
    /// </summary>
    private const int SourceMaxLength = 50;

    #endregion

    #region Constructor

    /// <summary>
    /// Inicializa una nueva instancia del validador y configura
    /// todas las reglas aplicables al comando de cancelación de pedido.
    /// </summary>
    public CancelOrderCommandValidator()
    {
        ConfigureMainRules();
        ConfigureTraceabilityRules();
        ConfigureDateRules();
    }

    #endregion

    #region Configuración de reglas

    /// <summary>
    /// Configura las reglas principales del comando.
    /// </summary>
    private void ConfigureMainRules()
    {
        RuleFor(command => command.OrderId)
            .NotEmpty()
            .WithMessage("El identificador del pedido es obligatorio.");

        RuleFor(command => command.Reason)
            .NotEmpty()
            .WithMessage("El motivo de cancelación es obligatorio.")
            .Must(ContainMeaningfulContent)
            .WithMessage("El motivo de cancelación no puede contener únicamente espacios en blanco.")
            .MinimumLength(ReasonMinLength)
            .WithMessage($"El motivo de cancelación debe tener al menos {ReasonMinLength} caracteres.")
            .MaximumLength(ReasonMaxLength)
            .WithMessage($"El motivo de cancelación no puede superar los {ReasonMaxLength} caracteres.");

        RuleFor(command => command.Notes)
            .MaximumLength(NotesMaxLength)
            .WithMessage($"Las notas no pueden superar los {NotesMaxLength} caracteres.")
            .Must(BeNullOrContainMeaningfulContent)
            .WithMessage("Si se informan notas, estas no pueden contener únicamente espacios en blanco.");

        RuleFor(command => command.ExternalReference)
            .MaximumLength(ExternalReferenceMaxLength)
            .WithMessage($"La referencia externa no puede superar los {ExternalReferenceMaxLength} caracteres.")
            .Must(BeNullOrContainMeaningfulContent)
            .WithMessage("Si se informa la referencia externa, esta no puede contener únicamente espacios en blanco.");
    }

    /// <summary>
    /// Configura las reglas relacionadas con trazabilidad y metadatos.
    /// </summary>
    private void ConfigureTraceabilityRules()
    {
        RuleFor(command => command.RequestedByUserId)
            .Must(BeNullOrNotEmptyGuid)
            .WithMessage("Si se informa el usuario solicitante, debe ser un identificador válido.");

        RuleFor(command => command.IpAddress)
            .MaximumLength(IpAddressMaxLength)
            .WithMessage($"La dirección IP no puede superar los {IpAddressMaxLength} caracteres.")
            .Must(BeNullOrContainMeaningfulContent)
            .WithMessage("Si se informa la dirección IP, esta no puede contener únicamente espacios en blanco.");

        RuleFor(command => command.Source)
            .MaximumLength(SourceMaxLength)
            .WithMessage($"El canal de origen no puede superar los {SourceMaxLength} caracteres.")
            .Must(BeNullOrContainMeaningfulContent)
            .WithMessage("Si se informa el canal de origen, este no puede contener únicamente espacios en blanco.");
    }

    /// <summary>
    /// Configura las reglas relacionadas con fechas y trazabilidad temporal.
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
    /// Indica si un texto contiene información significativa.
    /// </summary>
    /// <param name="value">Valor a evaluar.</param>
    /// <returns>
    /// <see langword="true"/> si el texto contiene contenido útil;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    private static bool ContainMeaningfulContent(string value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Indica si un texto es nulo o contiene información significativa.
    /// </summary>
    /// <param name="value">Valor a evaluar.</param>
    /// <returns>
    /// <see langword="true"/> si el valor es nulo o contiene texto útil;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    private static bool BeNullOrContainMeaningfulContent(string? value)
    {
        return value is null || !string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Indica si un identificador opcional es nulo o diferente de <see cref="Guid.Empty"/>.
    /// </summary>
    /// <param name="value">Identificador a evaluar.</param>
    /// <returns>
    /// <see langword="true"/> si el valor es nulo o válido;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    private static bool BeNullOrNotEmptyGuid(Guid? value)
    {
        return !value.HasValue || value.Value != Guid.Empty;
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