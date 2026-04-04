namespace PlataformaECommerce.Application.Features.Orders.DTOs;

/// <summary>
/// Representa el resultado funcional de confirmar el retorno de una pasarela de pago.
/// </summary>
public sealed record PaymentReturnResultDto
{
    /// <summary>
    /// Identificador del pedido evaluado.
    /// </summary>
    public Guid OrderId { get; init; }

    /// <summary>
    /// Nombre del proveedor externo utilizado.
    /// </summary>
    public string Provider { get; init; } = string.Empty;

    /// <summary>
    /// Identificador de la transacción externa verificada.
    /// </summary>
    public string GatewayTransactionId { get; init; } = string.Empty;

    /// <summary>
    /// Indica si el pago fue aprobado por la pasarela.
    /// </summary>
    public bool IsApproved { get; init; }

    /// <summary>
    /// Indica si el pago ya había sido registrado previamente en el pedido.
    /// </summary>
    public bool WasAlreadyRegistered { get; init; }

    /// <summary>
    /// Mensaje funcional que debe mostrarse al cliente tras el retorno.
    /// </summary>
    public string UserMessage { get; init; } = string.Empty;
}
