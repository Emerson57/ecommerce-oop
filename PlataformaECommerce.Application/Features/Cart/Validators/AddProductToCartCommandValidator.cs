using FluentValidation;
using PlataformaECommerce.Application.Features.Cart.Commands;

namespace PlataformaECommerce.Application.Features.Cart.Validators;

/// <summary>
/// Validador de aplicación para el comando <see cref="AddProductToCartCommand"/>.
/// </summary>
/// <remarks>
/// Esta clase centraliza las reglas de validación de entrada necesarias
/// antes de ejecutar el caso de uso de agregado de un producto a un carrito.
///
/// Su responsabilidad es proteger la capa Application frente a solicitudes
/// incompletas, inconsistentes o mal formadas, permitiendo que el handler
/// reciba un comando previamente saneado desde el punto de vista estructural.
///
/// Las validaciones aquí definidas no reemplazan las reglas del dominio,
/// sino que actúan como una primera barrera de entrada para:
/// - endpoints HTTP,
/// - servicios de aplicación,
/// - procesos de carrito,
/// - integraciones externas,
/// - y flujos de compra.
///
/// Este validador verifica únicamente consistencia estructural.
/// La existencia real del carrito, del producto, la disponibilidad comercial,
/// el inventario y demás reglas de negocio deben resolverse en el handler,
/// el servicio de aplicación y el dominio.
/// </remarks>
public sealed class AddProductToCartCommandValidator : AbstractValidator<AddProductToCartCommand>
{
    #region Constantes de validación

    /// <summary>
    /// Cantidad mínima permitida para agregar un producto al carrito.
    /// </summary>
    private const int MinQuantity = 1;

    /// <summary>
    /// Cantidad máxima permitida por operación de agregado al carrito.
    /// </summary>
    /// <remarks>
    /// Este valor actúa como barrera defensiva a nivel de Application
    /// para evitar entradas desproporcionadas o probablemente erróneas.
    /// La validación definitiva de límites comerciales debe reforzarse
    /// en el dominio.
    /// </remarks>
    private const int MaxQuantity = 999;

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
    /// <see cref="AddProductToCartCommandValidator"/>.
    /// </summary>
    public AddProductToCartCommandValidator()
    {
        ConfigureIdentityRules();
        ConfigureOperationRules();
        ConfigureContextRules();
        ConfigureCrossFieldRules();
    }

    #endregion

    #region Métodos privados de configuración

    /// <summary>
    /// Configura las reglas relacionadas con la identificación
    /// del carrito, del producto y del solicitante.
    /// </summary>
    private void ConfigureIdentityRules()
    {
        RuleFor(x => x.CartId)
            .NotEmpty()
            .WithMessage("El identificador del carrito es obligatorio.");

        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("El identificador del producto es obligatorio.");

        RuleFor(x => x.RequestedByUserId)
            .Must(id => !id.HasValue || id.Value != Guid.Empty)
            .WithMessage("El identificador del usuario solicitante no puede ser un valor vacío.");
    }

    /// <summary>
    /// Configura las reglas relacionadas con la operación de agregado.
    /// </summary>
    private void ConfigureOperationRules()
    {
        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(MinQuantity)
            .WithMessage($"La cantidad a agregar al carrito debe ser al menos {MinQuantity}.")
            .LessThanOrEqualTo(MaxQuantity)
            .WithMessage($"La cantidad a agregar al carrito no puede superar {MaxQuantity} unidades.");
    }

    /// <summary>
    /// Configura las reglas relacionadas con el contexto y trazabilidad de la operación.
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

    /// <summary>
    /// Configura reglas cruzadas de consistencia entre campos.
    /// </summary>
    private void ConfigureCrossFieldRules()
    {
        RuleFor(x => x)
            .Must(command => command.CartId != command.ProductId)
            .WithMessage("El identificador del carrito y el identificador del producto no pueden coincidir.");
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