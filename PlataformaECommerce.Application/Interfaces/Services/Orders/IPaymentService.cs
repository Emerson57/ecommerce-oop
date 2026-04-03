using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Orders.Commands;
using PlataformaECommerce.Application.Features.Orders.DTOs;

namespace PlataformaECommerce.Application.Interfaces.Services.Orders;

/// <summary>
/// Define la frontera de registro de pagos del módulo de pedidos.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Registra el pago exitoso de un pedido.
    /// </summary>
    Task<Result<OrderDetailDto>> RegisterOrderPaymentAsync(
        RegisterOrderPaymentCommand command,
        CancellationToken cancellationToken = default);
}
