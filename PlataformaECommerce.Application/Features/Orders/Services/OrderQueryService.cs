using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Orders.DTOs;
using PlataformaECommerce.Application.Features.Orders.Mappings;
using PlataformaECommerce.Application.Features.Orders.Queries;
using PlataformaECommerce.Application.Interfaces.Repositories.Orders;
using PlataformaECommerce.Application.Interfaces.Repositories.Users;
using PlataformaECommerce.Application.Interfaces.Services.Orders;
using PlataformaECommerce.Domain.Entities.Orders;
using PlataformaECommerce.Domain.Entities.Users;

namespace PlataformaECommerce.Application.Features.Orders.Services;

/// <summary>
/// Orquesta las operaciones de lectura del módulo de pedidos.
/// </summary>
public sealed class OrderQueryService : IOrderQueryService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="OrderQueryService"/>.
    /// </summary>
    public OrderQueryService(
        IOrderRepository orderRepository,
        IUserRepository userRepository)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    /// <inheritdoc />
    public async Task<Result<OrderDetailDto>> GetOrderByIdAsync(
        GetOrderByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.OrderId == Guid.Empty)
        {
            return Result.Failure<OrderDetailDto>(
                Error.Validation("Orders.InvalidId", "El identificador del pedido es obligatorio."));
        }

        Pedido? order = await _orderRepository.GetByIdAsync(query.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure<OrderDetailDto>(
                Error.NotFound("Orders.NotFound", $"No se encontró un pedido con identificador '{query.OrderId}'."));
        }

        if (query.ExpectedCustomerId.HasValue && order.ClienteId != query.ExpectedCustomerId.Value)
        {
            return Result.Failure<OrderDetailDto>(
                Error.Unauthorized("Orders.CustomerOwnershipMismatch", "El pedido consultado no pertenece al cliente esperado."));
        }

        OrderDetailDto orderDto = query.IncludeItems || query.IncludeExtendedData
            ? order.ToOrderDetailDto()
            : order.ToOrderDetailDtoWithoutItems();

        return Result.Success(orderDto);
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyCollection<OrderDto>>> GetOrdersByCustomerIdAsync(
        GetOrdersByCustomerIdQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.CustomerId == Guid.Empty)
        {
            return Result.Failure<IReadOnlyCollection<OrderDto>>(
                Error.Validation("Orders.InvalidCustomerId", "El identificador del cliente es obligatorio."));
        }

        Cliente? customer = await _userRepository.GetCustomerByIdAsync(query.CustomerId, cancellationToken);
        if (customer is null)
        {
            return Result.Failure<IReadOnlyCollection<OrderDto>>(
                Error.NotFound("Orders.CustomerNotFound", $"No se encontró un cliente con identificador '{query.CustomerId}'."));
        }

        IReadOnlyCollection<Pedido> orders = query.Status.HasValue
            ? await _orderRepository.GetByCustomerIdAndStatusAsync(query.CustomerId, query.Status.Value, cancellationToken)
            : await _orderRepository.GetByCustomerIdAsync(query.CustomerId, cancellationToken);

        IEnumerable<Pedido> filteredOrders = OrderServiceSupport.ApplyOrdersFilter(orders, query);
        IEnumerable<Pedido> orderedOrders = OrderServiceSupport.ApplyOrdersSorting(filteredOrders, query);

        IReadOnlyCollection<OrderDto> result = orderedOrders
            .Skip(query.Offset)
            .Take(query.NormalizedPageSize)
            .ToOrderDtos(query.IncludeItems)
            .ToArray();

        return Result.Success(result);
    }
}
