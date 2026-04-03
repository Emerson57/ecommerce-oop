using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Orders.DTOs;
using PlataformaECommerce.Application.Features.Orders.Queries;

namespace PlataformaECommerce.Application.Interfaces.Services.Orders;

/// <summary>
/// Define la frontera de lectura del módulo de pedidos.
/// </summary>
public interface IOrderQueryService
{
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
