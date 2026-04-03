using System.Globalization;
using FluentValidation;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Orders.Commands;
using PlataformaECommerce.Application.Features.Orders.DTOs;
using PlataformaECommerce.Application.Features.Orders.Mappings;
using PlataformaECommerce.Application.Interfaces.Persistence;
using PlataformaECommerce.Application.Interfaces.Repositories.Orders;
using PlataformaECommerce.Application.Interfaces.Services.Audit;
using PlataformaECommerce.Application.Interfaces.Services.Orders;
using PlataformaECommerce.Domain.Entities.Orders;

namespace PlataformaECommerce.Application.Features.Orders.Services;

/// <summary>
/// Orquesta las transiciones operativas del ciclo de vida del pedido.
/// </summary>
public sealed class OrderLifecycleService : IOrderLifecycleService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditTrailService _auditTrailService;
    private readonly IValidator<CancelOrderCommand> _cancelOrderCommandValidator;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="OrderLifecycleService"/>.
    /// </summary>
    public OrderLifecycleService(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IAuditTrailService auditTrailService,
        IValidator<CancelOrderCommand> cancelOrderCommandValidator)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _auditTrailService = auditTrailService ?? throw new ArgumentNullException(nameof(auditTrailService));
        _cancelOrderCommandValidator = cancelOrderCommandValidator ?? throw new ArgumentNullException(nameof(cancelOrderCommandValidator));
    }

    /// <inheritdoc />
    public Task<Result<OrderDetailDto>> ConfirmOrderAsync(ConfirmOrderCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.OrderId == Guid.Empty)
        {
            return Task.FromResult(Result.Failure<OrderDetailDto>(
                Error.Validation("Orders.InvalidId", "El identificador del pedido es obligatorio.")));
        }

        return ExecuteLifecycleChangeAsync(
            command.OrderId,
            order => order.Confirmar(),
            "order.confirmed",
            order => $"Se confirmó el pedido '{order.Id}'.",
            order => new Dictionary<string, string>
            {
                ["status"] = order.Estado.ToString(),
                ["totalAmount"] = order.Total.Amount.ToString(CultureInfo.InvariantCulture),
                ["currency"] = order.Total.Currency
            },
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<OrderDetailDto>> ProcessOrderAsync(ProcessOrderCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.OrderId == Guid.Empty)
        {
            return Task.FromResult(Result.Failure<OrderDetailDto>(
                Error.Validation("Orders.InvalidId", "El identificador del pedido es obligatorio.")));
        }

        return ExecuteLifecycleChangeAsync(
            command.OrderId,
            order => order.MarcarEnProceso(),
            "order.processing.started",
            order => $"El pedido '{order.Id}' pasó a estado en proceso.",
            order => new Dictionary<string, string>
            {
                ["status"] = order.Estado.ToString()
            },
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<OrderDetailDto>> ShipOrderAsync(ShipOrderCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.OrderId == Guid.Empty)
        {
            return Task.FromResult(Result.Failure<OrderDetailDto>(
                Error.Validation("Orders.InvalidId", "El identificador del pedido es obligatorio.")));
        }

        if (string.IsNullOrWhiteSpace(command.CarrierName))
        {
            return Task.FromResult(Result.Failure<OrderDetailDto>(
                Error.Validation("Orders.InvalidCarrierName", "El nombre del transportador es obligatorio para despachar el pedido.")));
        }

        if (string.IsNullOrWhiteSpace(command.TrackingNumber))
        {
            return Task.FromResult(Result.Failure<OrderDetailDto>(
                Error.Validation("Orders.InvalidTrackingNumber", "El número de guía o seguimiento es obligatorio para despachar el pedido.")));
        }

        return ExecuteLifecycleChangeAsync(
            command.OrderId,
            order => order.MarcarEnviado(),
            "order.shipped",
            order => $"Se despachó el pedido '{order.Id}'.",
            order => new Dictionary<string, string>
            {
                ["carrierName"] = command.CarrierName.Trim(),
                ["trackingNumber"] = command.TrackingNumber.Trim(),
                ["status"] = order.Estado.ToString(),
                ["hasShippingAddress"] = order.TieneDireccionEnvio().ToString()
            },
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<OrderDetailDto>> DeliverOrderAsync(DeliverOrderCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.OrderId == Guid.Empty)
        {
            return Task.FromResult(Result.Failure<OrderDetailDto>(
                Error.Validation("Orders.InvalidId", "El identificador del pedido es obligatorio.")));
        }

        return ExecuteLifecycleChangeAsync(
            command.OrderId,
            order => order.MarcarEntregado(),
            "order.delivered",
            order => $"Se entregó el pedido '{order.Id}'.",
            order => new Dictionary<string, string>
            {
                ["status"] = order.Estado.ToString()
            },
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<OrderDetailDto>> CancelOrderAsync(CancelOrderCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = await OrderServiceSupport.ValidateAsync(command, _cancelOrderCommandValidator, cancellationToken);
        if (validationError is not null)
        {
            return Result.Failure<OrderDetailDto>(validationError);
        }

        return await ExecuteLifecycleChangeAsync(
            command.OrderId,
            order => order.Cancelar(command.Reason),
            "order.cancelled",
            order => $"Se canceló el pedido '{order.Id}'.",
            order => new Dictionary<string, string>
            {
                ["reason"] = command.Reason.Trim(),
                ["status"] = order.Estado.ToString()
            },
            cancellationToken);
    }

    private Task<Result<OrderDetailDto>> ExecuteLifecycleChangeAsync(
        Guid orderId,
        Action<Pedido> transition,
        string action,
        Func<Pedido, string> detailFactory,
        Func<Pedido, IReadOnlyDictionary<string, string>> metadataFactory,
        CancellationToken cancellationToken)
    {
        return OrderServiceSupport.ExecuteAsync(async () =>
        {
            Pedido? order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
            if (order is null)
            {
                return Result.Failure<OrderDetailDto>(
                    Error.NotFound("Orders.NotFound", $"No se encontró un pedido con identificador '{orderId}'."));
            }

            transition(order);

            await _orderRepository.UpdateAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await OrderServiceSupport.AuditOrderEventAsync(
                _auditTrailService,
                order,
                action,
                detailFactory(order),
                metadataFactory(order),
                cancellationToken);

            return Result.Success(order.ToOrderDetailDto());
        }, "Orders.Domain");
    }
}
