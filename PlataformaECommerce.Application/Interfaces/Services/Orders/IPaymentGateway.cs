using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Orders.DTOs;

namespace PlataformaECommerce.Application.Interfaces.Services.Orders;

/// <summary>
/// Define el adaptador requerido para interactuar con una pasarela de pago externa.
/// </summary>
public interface IPaymentGateway
{
    /// <summary>
    /// Crea una sesión de checkout externa para un pedido.
    /// </summary>
    Task<Result<PaymentCheckoutSessionDto>> CreateCheckoutSessionAsync(
        PaymentGatewayCheckoutRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica una transacción externa reportada por la pasarela.
    /// </summary>
    Task<Result<PaymentGatewayTransactionDto>> VerifyTransactionAsync(
        string gatewayTransactionId,
        CancellationToken cancellationToken = default);
}
