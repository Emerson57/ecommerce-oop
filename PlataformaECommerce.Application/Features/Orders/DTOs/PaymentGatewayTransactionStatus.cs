namespace PlataformaECommerce.Application.Features.Orders.DTOs;

/// <summary>
/// Define los estados relevantes devueltos por una pasarela de pago externa.
/// </summary>
public enum PaymentGatewayTransactionStatus
{
    Pending = 1,
    Approved = 2,
    Declined = 3,
    Error = 4
}
