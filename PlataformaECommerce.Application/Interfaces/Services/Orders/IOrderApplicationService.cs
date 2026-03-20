using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Orders.Commands;
using PlataformaECommerce.Application.Features.Orders.DTOs;
using PlataformaECommerce.Application.Features.Orders.Queries;

namespace PlataformaECommerce.Application.Interfaces.Services.Orders;

/// <summary>
/// Define el contrato del servicio de aplicación encargado de coordinar
/// los casos de uso del módulo de pedidos.
/// </summary>
/// <remarks>
/// Este contrato constituye la frontera pública del módulo de pedidos dentro de
/// <c>Application</c>. Los comandos y consultas que recibe representan solicitudes
/// del caso de uso procesadas por un servicio de aplicación orientado al ciclo de vida del pedido.
/// </remarks>
public interface IOrderApplicationService
{
    /// <summary>
    /// Crea un nuevo pedido a partir de un carrito existente.
    /// </summary>
    Task<Result<OrderDetailDto>> CreateOrderFromCartAsync(
        CreateOrderFromCartCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirma un pedido existente.
    /// </summary>
    Task<Result<OrderDetailDto>> ConfirmOrderAsync(
        ConfirmOrderCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra el pago exitoso de un pedido.
    /// </summary>
    Task<Result<OrderDetailDto>> RegisterOrderPaymentAsync(
        RegisterOrderPaymentCommand command,
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

    /// <summary>
    /// Obtiene el detalle de un pedido a partir de su identificador.
    /// </summary>
    Task<Result<OrderDetailDto>> GetOrderByIdAsync(
        GetOrderByIdQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene los pedidos asociados a un cliente específico.
    /// </summary>
    Task<Result<IReadOnlyCollection<OrderDto>>> GetOrdersByCustomerIdAsync(
        GetOrdersByCustomerIdQuery query,
        CancellationToken cancellationToken = default);
}
