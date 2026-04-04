using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Orders.Commands;
using PlataformaECommerce.Application.Features.Orders.DTOs;

namespace PlataformaECommerce.Application.Interfaces.Services.Orders;

/// <summary>
/// Define la frontera de orquestación del checkout de pagos externos.
/// </summary>
public interface IOrderPaymentCheckoutService
{
    /// <summary>
    /// Crea una sesión de pago externa para un pedido confirmado.
    /// </summary>
    Task<Result<PaymentCheckoutSessionDto>> CreateCheckoutSessionAsync(
        CreateOrderPaymentSessionCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirma el retorno de una pasarela de pago para un pedido.
    /// </summary>
    Task<Result<PaymentReturnResultDto>> ConfirmPaymentReturnAsync(
        ConfirmOrderPaymentReturnCommand command,
        CancellationToken cancellationToken = default);
}
