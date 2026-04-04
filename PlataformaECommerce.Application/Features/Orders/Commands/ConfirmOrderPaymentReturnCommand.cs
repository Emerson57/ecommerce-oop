namespace PlataformaECommerce.Application.Features.Orders.Commands;

/// <summary>
/// Solicita confirmar el resultado devuelto por una pasarela de pago externa.
/// </summary>
public sealed class ConfirmOrderPaymentReturnCommand
{
    /// <summary>
    /// Identificador del pedido relacionado con el pago.
    /// </summary>
    public Guid OrderId { get; init; }

    /// <summary>
    /// Identificador externo de la transacción reportado por la pasarela.
    /// </summary>
    public string GatewayTransactionId { get; init; } = string.Empty;

    /// <summary>
    /// Identificador opcional del usuario autenticado que recibe el retorno.
    /// </summary>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>
    /// Dirección IP desde la cual se procesa el retorno.
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// Origen funcional de la confirmación.
    /// </summary>
    public string? Source { get; init; }

    /// <summary>
    /// Fecha UTC en que se recibió la confirmación en la capa superior.
    /// </summary>
    public DateTime? RequestedAtUtc { get; init; }
}
