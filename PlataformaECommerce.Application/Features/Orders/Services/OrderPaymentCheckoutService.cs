using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Orders.Commands;
using PlataformaECommerce.Application.Features.Orders.DTOs;
using PlataformaECommerce.Application.Interfaces.Repositories.Orders;
using PlataformaECommerce.Application.Interfaces.Services.Orders;
using PlataformaECommerce.Domain.Entities.Orders;
using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Application.Features.Orders.Services;

/// <summary>
/// Orquesta la creación de sesiones de pago externas y la confirmación de sus retornos.
/// </summary>
public sealed class OrderPaymentCheckoutService : IOrderPaymentCheckoutService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IPaymentService _paymentService;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="OrderPaymentCheckoutService"/>.
    /// </summary>
    public OrderPaymentCheckoutService(
        IOrderRepository orderRepository,
        IPaymentGateway paymentGateway,
        IPaymentService paymentService)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _paymentGateway = paymentGateway ?? throw new ArgumentNullException(nameof(paymentGateway));
        _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
    }

    /// <inheritdoc />
    public async Task<Result<PaymentCheckoutSessionDto>> CreateCheckoutSessionAsync(
        CreateOrderPaymentSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.OrderId == Guid.Empty)
        {
            return Result.Failure<PaymentCheckoutSessionDto>(
                Error.Validation("Orders.InvalidId", "El identificador del pedido es obligatorio."));
        }

        if (string.IsNullOrWhiteSpace(command.ReturnUrl))
        {
            return Result.Failure<PaymentCheckoutSessionDto>(
                Error.Validation("Orders.PaymentReturnUrlRequired", "La URL de retorno del pago es obligatoria."));
        }

        Pedido? order = await _orderRepository.GetByIdAsync(command.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure<PaymentCheckoutSessionDto>(
                Error.NotFound("Orders.NotFound", $"No se encontró un pedido con identificador '{command.OrderId}'."));
        }

        if (command.ExpectedCustomerId.HasValue && order.ClienteId != command.ExpectedCustomerId.Value)
        {
            return Result.Failure<PaymentCheckoutSessionDto>(
                Error.Unauthorized("Orders.CustomerOwnershipMismatch", "El pedido no pertenece al cliente autenticado."));
        }

        if (order.EstaFinalizado())
        {
            return Result.Failure<PaymentCheckoutSessionDto>(
                Error.Conflict("Orders.Finalized", "No es posible iniciar un pago para un pedido finalizado."));
        }

        if (order.Estado is EstadoPedido.Pagado or EstadoPedido.EnProceso or EstadoPedido.Enviado or EstadoPedido.Entregado)
        {
            return Result.Failure<PaymentCheckoutSessionDto>(
                Error.Conflict("Orders.AlreadyPaid", "El pedido ya tiene un pago registrado o avanzó más allá del pago."));
        }

        if (order.Estado != EstadoPedido.Confirmado)
        {
            return Result.Failure<PaymentCheckoutSessionDto>(
                Error.Conflict("Orders.InvalidPaymentState", "Solo los pedidos confirmados pueden iniciar el pago externo."));
        }

        if (!order.MetodoPagoSeleccionado.HasValue)
        {
            return Result.Failure<PaymentCheckoutSessionDto>(
                Error.Validation("Orders.PaymentMethodRequired", "El pedido no tiene un método de pago seleccionado."));
        }

        if (order.MetodoPagoSeleccionado == MetodoPagoPedido.ContraEntrega)
        {
            return Result.Failure<PaymentCheckoutSessionDto>(
                Error.Conflict("Orders.CashOnDelivery", "Los pedidos contra entrega no requieren una pasarela de pago externa."));
        }

        return await _paymentGateway.CreateCheckoutSessionAsync(new PaymentGatewayCheckoutRequestDto
        {
            OrderId = order.Id,
            CustomerId = order.ClienteId,
            PaymentMethod = order.MetodoPagoSeleccionado.Value,
            Amount = order.Total.Amount,
            Currency = order.Total.Currency,
            PaymentReference = BuildPaymentReference(order.Id),
            ReturnUrl = command.ReturnUrl.Trim()
        }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<PaymentReturnResultDto>> ConfirmPaymentReturnAsync(
        ConfirmOrderPaymentReturnCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.OrderId == Guid.Empty)
        {
            return Result.Failure<PaymentReturnResultDto>(
                Error.Validation("Orders.InvalidId", "El identificador del pedido es obligatorio."));
        }

        if (string.IsNullOrWhiteSpace(command.GatewayTransactionId))
        {
            return Result.Failure<PaymentReturnResultDto>(
                Error.Validation("Orders.PaymentTransactionRequired", "La transacción externa del pago es obligatoria."));
        }

        Pedido? order = await _orderRepository.GetByIdAsync(command.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure<PaymentReturnResultDto>(
                Error.NotFound("Orders.NotFound", $"No se encontró un pedido con identificador '{command.OrderId}'."));
        }

        PaymentGatewayTransactionDto gatewayTransaction;
        var gatewayResult = await _paymentGateway.VerifyTransactionAsync(command.GatewayTransactionId.Trim(), cancellationToken);
        if (gatewayResult.IsFailure)
        {
            return Result.Failure<PaymentReturnResultDto>(gatewayResult.Error);
        }

        gatewayTransaction = gatewayResult.Value;
        string expectedReference = BuildPaymentReference(order.Id);
        if (!string.Equals(expectedReference, gatewayTransaction.PaymentReference, StringComparison.Ordinal))
        {
            return Result.Failure<PaymentReturnResultDto>(
                Error.Validation("Orders.PaymentReferenceMismatch", "La referencia de pago validada no corresponde al pedido consultado."));
        }

        if (!string.Equals(order.Total.Currency, gatewayTransaction.Currency, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<PaymentReturnResultDto>(
                Error.Validation("Orders.PaymentCurrencyMismatch", "La moneda validada por la pasarela no coincide con la del pedido."));
        }

        if (order.Total.Amount != gatewayTransaction.Amount)
        {
            return Result.Failure<PaymentReturnResultDto>(
                Error.Validation("Orders.PaymentAmountMismatch", "El valor validado por la pasarela no coincide con el total del pedido."));
        }

        if (order.Estado is EstadoPedido.Pagado or EstadoPedido.EnProceso or EstadoPedido.Enviado or EstadoPedido.Entregado)
        {
            return Result.Success(new PaymentReturnResultDto
            {
                OrderId = order.Id,
                Provider = gatewayTransaction.Provider,
                GatewayTransactionId = gatewayTransaction.GatewayTransactionId,
                IsApproved = true,
                WasAlreadyRegistered = true,
                UserMessage = "El pago de este pedido ya estaba confirmado previamente."
            });
        }

        if (gatewayTransaction.Status != PaymentGatewayTransactionStatus.Approved)
        {
            return Result.Success(new PaymentReturnResultDto
            {
                OrderId = order.Id,
                Provider = gatewayTransaction.Provider,
                GatewayTransactionId = gatewayTransaction.GatewayTransactionId,
                IsApproved = false,
                WasAlreadyRegistered = false,
                UserMessage = gatewayTransaction.Status switch
                {
                    PaymentGatewayTransactionStatus.Pending => "El pago aún se encuentra pendiente de confirmación por la pasarela.",
                    PaymentGatewayTransactionStatus.Declined => "La pasarela reportó que el pago fue rechazado. Puedes intentarlo nuevamente desde el detalle del pedido.",
                    _ => "La pasarela reportó un error durante la confirmación del pago."
                }
            });
        }

        var registerResult = await _paymentService.RegisterOrderPaymentAsync(new RegisterOrderPaymentCommand
        {
            OrderId = order.Id,
            PaymentReference = gatewayTransaction.PaymentReference,
            PaymentMethod = string.IsNullOrWhiteSpace(gatewayTransaction.PaymentMethod)
                ? order.MetodoPagoSeleccionado?.ToString() ?? string.Empty
                : gatewayTransaction.PaymentMethod,
            Amount = gatewayTransaction.Amount,
            Currency = gatewayTransaction.Currency,
            PaidAtUtc = gatewayTransaction.PaidAtUtc,
            PaymentProvider = gatewayTransaction.Provider,
            RequestedByUserId = command.RequestedByUserId,
            IpAddress = command.IpAddress,
            Source = command.Source,
            RequestedAtUtc = command.RequestedAtUtc,
            ExternalReference = gatewayTransaction.GatewayTransactionId
        }, cancellationToken);

        if (registerResult.IsFailure)
        {
            return Result.Failure<PaymentReturnResultDto>(registerResult.Error);
        }

        return Result.Success(new PaymentReturnResultDto
        {
            OrderId = order.Id,
            Provider = gatewayTransaction.Provider,
            GatewayTransactionId = gatewayTransaction.GatewayTransactionId,
            IsApproved = true,
            WasAlreadyRegistered = false,
            UserMessage = "El pago fue confirmado correctamente y el pedido quedó marcado como pagado."
        });
    }

    private static string BuildPaymentReference(Guid orderId)
    {
        return $"PAY-{orderId:N}";
    }
}
