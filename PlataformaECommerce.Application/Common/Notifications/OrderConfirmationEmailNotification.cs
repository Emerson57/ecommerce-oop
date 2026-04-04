using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Application.Common.Notifications;

/// <summary>
/// Representa los datos necesarios para enviar la confirmación de compra por correo.
/// </summary>
public sealed record OrderConfirmationEmailNotification
{
    /// <summary>
    /// Correo electrónico del cliente.
    /// </summary>
    public string ToEmail { get; init; } = string.Empty;

    /// <summary>
    /// Nombre visible del cliente.
    /// </summary>
    public string RecipientName { get; init; } = string.Empty;

    /// <summary>
    /// Identificador del pedido confirmado.
    /// </summary>
    public Guid OrderId { get; init; }

    /// <summary>
    /// Total monetario del pedido.
    /// </summary>
    public decimal TotalAmount { get; init; }

    /// <summary>
    /// Moneda del total del pedido.
    /// </summary>
    public string Currency { get; init; } = string.Empty;

    /// <summary>
    /// Método de pago seleccionado durante el checkout.
    /// </summary>
    public MetodoPagoPedido? PaymentMethod { get; init; }

    /// <summary>
    /// Dirección resumida de envío cuando aplica.
    /// </summary>
    public string? ShippingAddressSummary { get; init; }

    /// <summary>
    /// Ítems incluidos en la confirmación.
    /// </summary>
    public IReadOnlyCollection<OrderConfirmationEmailItem> Items { get; init; } = Array.Empty<OrderConfirmationEmailItem>();
}

/// <summary>
/// Representa una línea incluida en la confirmación de compra por correo.
/// </summary>
public sealed record OrderConfirmationEmailItem
{
    public string ProductName { get; init; } = string.Empty;
    public string ProductSku { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal Subtotal { get; init; }
    public string Currency { get; init; } = string.Empty;
}
