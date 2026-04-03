using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Orders.Commands;
using PlataformaECommerce.Application.Features.Orders.DTOs;

namespace PlataformaECommerce.Application.Interfaces.Services.Orders;

/// <summary>
/// Define la frontera de creación del módulo de pedidos.
/// </summary>
public interface IOrderCreationService
{
    /// <summary>
    /// Crea un nuevo pedido a partir de un carrito existente.
    /// </summary>
    Task<Result<OrderDetailDto>> CreateOrderFromCartAsync(
        CreateOrderFromCartCommand command,
        CancellationToken cancellationToken = default);
}
