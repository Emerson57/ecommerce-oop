using System.Globalization;
using FluentValidation;
using PlataformaECommerce.Application.Common.Execution;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Orders.Commands;
using PlataformaECommerce.Application.Features.Orders.DTOs;
using PlataformaECommerce.Application.Features.Orders.Queries;
using FluentValidation;
using PlataformaECommerce.Application.Interfaces.Persistence;
using PlataformaECommerce.Application.Interfaces.Repositories.Cart;
using PlataformaECommerce.Application.Interfaces.Repositories.Orders;
using PlataformaECommerce.Application.Interfaces.Repositories.Users;
using PlataformaECommerce.Application.Interfaces.Services.Audit;
using PlataformaECommerce.Application.Interfaces.Services.Orders;
using Microsoft.Extensions.DependencyInjection;

namespace PlataformaECommerce.Application.Features.Orders.Services;

/// <summary>
/// Mantiene la frontera pública heredada del módulo de pedidos delegando en servicios especializados.
/// </summary>
public sealed class OrderApplicationService : IOrderApplicationService
{
    private readonly IOrderCreationService _orderCreationService;
    private readonly IOrderLifecycleService _orderLifecycleService;
    private readonly IOrderQueryService _orderQueryService;
    private readonly IPaymentService _paymentService;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="OrderApplicationService"/>.
    /// </summary>
    [ActivatorUtilitiesConstructor]
    public OrderApplicationService(
        IOrderCreationService orderCreationService,
        IOrderLifecycleService orderLifecycleService,
        IOrderQueryService orderQueryService,
        IPaymentService paymentService)
    {
        _orderCreationService = orderCreationService ?? throw new ArgumentNullException(nameof(orderCreationService));
        _orderLifecycleService = orderLifecycleService ?? throw new ArgumentNullException(nameof(orderLifecycleService));
        _orderQueryService = orderQueryService ?? throw new ArgumentNullException(nameof(orderQueryService));
        _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
    }

    /// <summary>
    /// Inicializa una nueva instancia de compatibilidad para pruebas existentes del módulo de pedidos.
    /// </summary>
    public OrderApplicationService(
        IOrderRepository orderRepository,
        ICartRepository cartRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IAuditTrailService auditTrailService,
        IValidator<CreateOrderFromCartCommand> createOrderFromCartCommandValidator,
        IValidator<CancelOrderCommand> cancelOrderCommandValidator)
        : this(
            new OrderCreationService(
                orderRepository,
                cartRepository,
                userRepository,
                unitOfWork,
                auditTrailService,
                createOrderFromCartCommandValidator),
            new OrderLifecycleService(orderRepository, unitOfWork, auditTrailService, cancelOrderCommandValidator),
            new OrderQueryService(orderRepository, userRepository),
            new PaymentService(orderRepository, unitOfWork, auditTrailService))
    {
    }

    /// <inheritdoc />
    public Task<Result<OrderDetailDto>> CreateOrderFromCartAsync(
        CreateOrderFromCartCommand command,
        CancellationToken cancellationToken = default)
    {
        return _orderCreationService.CreateOrderFromCartAsync(command, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<OrderDetailDto>> ConfirmOrderAsync(
        ConfirmOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        return _orderLifecycleService.ConfirmOrderAsync(command, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<OrderDetailDto>> RegisterOrderPaymentAsync(
        RegisterOrderPaymentCommand command,
        CancellationToken cancellationToken = default)
    {
        return _paymentService.RegisterOrderPaymentAsync(command, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<OrderDetailDto>> ProcessOrderAsync(
        ProcessOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        return _orderLifecycleService.ProcessOrderAsync(command, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<OrderDetailDto>> ShipOrderAsync(
        ShipOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        return _orderLifecycleService.ShipOrderAsync(command, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<OrderDetailDto>> DeliverOrderAsync(
        DeliverOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        return _orderLifecycleService.DeliverOrderAsync(command, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<OrderDetailDto>> CancelOrderAsync(
        CancelOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        return _orderLifecycleService.CancelOrderAsync(command, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<OrderDetailDto>> GetOrderByIdAsync(
        GetOrderByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        return _orderQueryService.GetOrderByIdAsync(query, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<IReadOnlyCollection<OrderDto>>> GetOrdersByCustomerIdAsync(
        GetOrdersByCustomerIdQuery query,
        CancellationToken cancellationToken = default)
    {
        return _orderQueryService.GetOrdersByCustomerIdAsync(query, cancellationToken);
    }
}