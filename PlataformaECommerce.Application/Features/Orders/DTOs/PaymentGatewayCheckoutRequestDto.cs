using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Application.Features.Orders.DTOs;

/// <summary>
/// Representa los datos requeridos para crear una sesión de checkout externa.
/// </summary>
public sealed record PaymentGatewayCheckoutRequestDto
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public MetodoPagoPedido PaymentMethod { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string PaymentReference { get; init; } = string.Empty;
    public string ReturnUrl { get; init; } = string.Empty;
}
