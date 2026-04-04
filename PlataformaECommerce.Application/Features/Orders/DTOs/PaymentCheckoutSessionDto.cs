namespace PlataformaECommerce.Application.Features.Orders.DTOs;

/// <summary>
/// Representa la sesión de checkout creada para redirigir al cliente a una pasarela de pago.
/// </summary>
public sealed record PaymentCheckoutSessionDto
{
    /// <summary>
    /// Nombre del proveedor externo utilizado.
    /// </summary>
    public string Provider { get; init; } = string.Empty;

    /// <summary>
    /// URL absoluta del checkout hospedado por la pasarela.
    /// </summary>
    public string CheckoutUrl { get; init; } = string.Empty;

    /// <summary>
    /// Referencia única del pago asociada al pedido.
    /// </summary>
    public string PaymentReference { get; init; } = string.Empty;

    /// <summary>
    /// Identificador del pedido relacionado con la sesión.
    /// </summary>
    public Guid OrderId { get; init; }
}
