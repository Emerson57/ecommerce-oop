using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Orders.Commands;
using PlataformaECommerce.Application.Features.Orders.DTOs;

namespace PlataformaECommerce.Application.Interfaces.Services.Orders;

/// <summary>
/// Define la frontera de transiciones operativas del ciclo de vida de pedidos.
/// </summary>
public interface IOrderLifecycleService
{
    /// <summary>
    /// Confirma un pedido existente.
    /// </summary>
    Task<Result<OrderDetailDto>> ConfirmOrderAsync(
        ConfirmOrderCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca un pedido como en proceso operativo.
    /// </summary>
    Task<Result<OrderDetailDto>> ProcessOrderAsync(
        ProcessOrderCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca un pedido como enviado o despachado.
    /// </summary>
    Task<Result<OrderDetailDto>> ShipOrderAsync(
        ShipOrderCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca un pedido como entregado.
    /// </summary>
    Task<Result<OrderDetailDto>> DeliverOrderAsync(
        DeliverOrderCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancela un pedido existente.
    /// </summary>
    Task<Result<OrderDetailDto>> CancelOrderAsync(
        CancelOrderCommand command,
        CancellationToken cancellationToken = default);
}
