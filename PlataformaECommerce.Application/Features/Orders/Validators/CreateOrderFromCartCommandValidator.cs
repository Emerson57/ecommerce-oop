using FluentValidation;
using PlataformaECommerce.Application.Features.Orders.Commands;

namespace PlataformaECommerce.Application.Features.Orders.Validators;

/// <summary>
/// Define las reglas de validación para el comando <see cref="CreateOrderFromCartCommand"/>.
/// </summary>
/// <remarks>
/// Este validador pertenece a la capa de aplicación y tiene como objetivo
/// garantizar que la solicitud de creación de pedido a partir de un carrito
/// cumpla con las validaciones estructurales mínimas antes de que el caso de uso
/// sea ejecutado por el servicio de aplicación correspondiente.
///
/// Las reglas aquí definidas se enfocan en:
/// - integridad básica de identificadores,
/// - consistencia de campos de trazabilidad,
/// - control de longitudes máximas,
/// - y saneamiento de datos de entrada.
///
/// Este componente no reemplaza las validaciones del dominio. Las reglas del dominio
/// deben mantenerse dentro de las entidades, value objects y servicios de dominio,
/// garantizando así una arquitectura robusta y correctamente separada.
/// </remarks>
public sealed class CreateOrderFromCartCommandValidator : AbstractValidator<CreateOrderFromCartCommand>
{
    #region Constantes de validación

    /// <summary>
    /// Longitud máxima permitida para el campo de notas.
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

    /// <summary>
    /// Longitud máxima permitida para cada componente de dirección de envío.
    /// </summary>
    private const int ShippingFieldMaxLength = 150;

    #endregion

    #region Constructor

    /// <summary>
    /// Inicializa una nueva instancia del validador y configura
    /// todas las reglas aplicables al comando de creación de pedido
    /// a partir de carrito.
    /// </summary>
    public CreateOrderFromCartCommandValidator()
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
        RuleFor(command => command.CartId)
            .NotEmpty()
            .WithMessage("El identificador del carrito es obligatorio.");

        RuleFor(command => command.CustomerId)
            .NotEmpty()
            .WithMessage("El identificador del cliente es obligatorio.");

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

        RuleFor(command => command.ShippingStreet)
            .MaximumLength(ShippingFieldMaxLength)
            .WithMessage($"La calle de envío no puede superar los {ShippingFieldMaxLength} caracteres.")
            .Must(BeNullOrContainMeaningfulContent)
            .WithMessage("Si se informa la calle de envío, esta no puede contener únicamente espacios en blanco.");

        RuleFor(command => command.ShippingCity)
            .MaximumLength(ShippingFieldMaxLength)
            .WithMessage($"La ciudad de envío no puede superar los {ShippingFieldMaxLength} caracteres.")
            .Must(BeNullOrContainMeaningfulContent)
            .WithMessage("Si se informa la ciudad de envío, esta no puede contener únicamente espacios en blanco.");

        RuleFor(command => command.ShippingDepartment)
            .MaximumLength(ShippingFieldMaxLength)
            .WithMessage($"El departamento de envío no puede superar los {ShippingFieldMaxLength} caracteres.")
            .Must(BeNullOrContainMeaningfulContent)
            .WithMessage("Si se informa el departamento de envío, este no puede contener únicamente espacios en blanco.");

        RuleFor(command => command.ShippingCountry)
            .MaximumLength(ShippingFieldMaxLength)
            .WithMessage($"El país de envío no puede superar los {ShippingFieldMaxLength} caracteres.")
            .Must(BeNullOrContainMeaningfulContent)
            .WithMessage("Si se informa el país de envío, este no puede contener únicamente espacios en blanco.");

        RuleFor(command => command.ShippingPostalCode)
            .MaximumLength(ShippingFieldMaxLength)
            .WithMessage($"El código postal de envío no puede superar los {ShippingFieldMaxLength} caracteres.")
            .Must(BeNullOrContainMeaningfulContent)
            .WithMessage("Si se informa el código postal de envío, este no puede contener únicamente espacios en blanco.");

        When(command => command.HasAnyShippingAddressComponent, () =>
        {
            RuleFor(command => command.ShippingStreet)
                .NotEmpty()
                .WithMessage("La calle de envío es obligatoria cuando se informa una dirección de envío.");

            RuleFor(command => command.ShippingCity)
                .NotEmpty()
                .WithMessage("La ciudad de envío es obligatoria cuando se informa una dirección de envío.");

            RuleFor(command => command.ShippingDepartment)
                .NotEmpty()
                .WithMessage("El departamento de envío es obligatorio cuando se informa una dirección de envío.");

            RuleFor(command => command.ShippingCountry)
                .NotEmpty()
                .WithMessage("El país de envío es obligatorio cuando se informa una dirección de envío.");

            RuleFor(command => command.ShippingPostalCode)
                .NotEmpty()
                .WithMessage("El código postal de envío es obligatorio cuando se informa una dirección de envío.");
        });
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