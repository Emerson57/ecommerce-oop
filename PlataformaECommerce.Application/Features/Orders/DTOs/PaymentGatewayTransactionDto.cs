namespace PlataformaECommerce.Application.Features.Orders.DTOs;

/// <summary>
/// Representa una transacción externa verificada contra la pasarela de pago.
/// </summary>
public sealed record PaymentGatewayTransactionDto
{
    /// <summary>
    /// Nombre del proveedor externo.
    /// </summary>
    public string Provider { get; init; } = string.Empty;

    /// <summary>
    /// Identificador externo de la transacción.
    /// </summary>
    public string GatewayTransactionId { get; init; } = string.Empty;

    /// <summary>
    /// Referencia de pago emitida por la solución.
    /// </summary>
    public string PaymentReference { get; init; } = string.Empty;

    /// <summary>
    /// Estado verificado de la transacción.
    /// </summary>
    public PaymentGatewayTransactionStatus Status { get; init; }

    /// <summary>
    /// Método de pago reportado por el proveedor.
    /// </summary>
    public string PaymentMethod { get; init; } = string.Empty;

    /// <summary>
    /// Valor pagado verificado.
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    /// Moneda verificada.
    /// </summary>
    public string Currency { get; init; } = string.Empty;

    /// <summary>
    /// Fecha UTC en que el proveedor finalizó la transacción, cuando esté disponible.
    /// </summary>
    public DateTime? PaidAtUtc { get; init; }
}
