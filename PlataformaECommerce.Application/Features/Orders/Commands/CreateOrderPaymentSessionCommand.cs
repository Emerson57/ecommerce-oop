using PlataformaECommerce.Application.Common.Results;

namespace PlataformaECommerce.Application.Features.Orders.Commands;

/// <summary>
/// Solicita la creación de una sesión de pago externa para un pedido confirmado.
/// </summary>
public sealed class CreateOrderPaymentSessionCommand
{
    /// <summary>
    /// Identificador del pedido que iniciará el flujo de pago.
    /// </summary>
    public Guid OrderId { get; init; }

    /// <summary>
    /// Identificador esperado del cliente propietario del pedido.
    /// </summary>
    public Guid? ExpectedCustomerId { get; init; }

    /// <summary>
    /// URL absoluta a la que la pasarela debe redirigir al finalizar el flujo.
    /// </summary>
    public string ReturnUrl { get; init; } = string.Empty;

    /// <summary>
    /// Identificador del usuario que origina la solicitud.
    /// </summary>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>
    /// Dirección IP desde la que se inicia la solicitud.
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// Origen funcional de la solicitud.
    /// </summary>
    public string? Source { get; init; }

    /// <summary>
    /// Fecha UTC en que la capa superior registró la solicitud.
    /// </summary>
    public DateTime? RequestedAtUtc { get; init; }
}
