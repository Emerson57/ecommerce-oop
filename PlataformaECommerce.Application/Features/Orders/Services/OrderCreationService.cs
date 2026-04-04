using System.Globalization;
using FluentValidation;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Common.Notifications;
using PlataformaECommerce.Application.Features.Orders.Commands;
using PlataformaECommerce.Application.Features.Orders.DTOs;
using PlataformaECommerce.Application.Features.Orders.Mappings;
using PlataformaECommerce.Application.Interfaces.Persistence;
using PlataformaECommerce.Application.Interfaces.Repositories.Cart;
using PlataformaECommerce.Application.Interfaces.Repositories.Orders;
using PlataformaECommerce.Application.Interfaces.Repositories.Users;
using PlataformaECommerce.Application.Interfaces.Services.Audit;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Application.Interfaces.Services.Orders;
using PlataformaECommerce.Domain.Entities.Cart;
using PlataformaECommerce.Domain.Entities.Orders;
using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Application.Features.Orders.Services;

/// <summary>
/// Orquesta la creación de pedidos a partir del carrito del cliente.
/// </summary>
public sealed class OrderCreationService : IOrderCreationService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditTrailService _auditTrailService;
    private readonly IEmailNotificationService _emailNotificationService;
    private readonly IValidator<CreateOrderFromCartCommand> _createOrderFromCartCommandValidator;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="OrderCreationService"/>.
    /// </summary>
    public OrderCreationService(
        IOrderRepository orderRepository,
        ICartRepository cartRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IAuditTrailService auditTrailService,
        IEmailNotificationService emailNotificationService,
        IValidator<CreateOrderFromCartCommand> createOrderFromCartCommandValidator)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _auditTrailService = auditTrailService ?? throw new ArgumentNullException(nameof(auditTrailService));
        _emailNotificationService = emailNotificationService ?? throw new ArgumentNullException(nameof(emailNotificationService));
        _createOrderFromCartCommandValidator = createOrderFromCartCommandValidator ?? throw new ArgumentNullException(nameof(createOrderFromCartCommandValidator));
    }

    /// <inheritdoc />
    public async Task<Result<OrderDetailDto>> CreateOrderFromCartAsync(
        CreateOrderFromCartCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = await OrderServiceSupport.ValidateAsync(command, _createOrderFromCartCommandValidator, cancellationToken);
        if (validationError is not null)
        {
            return Result.Failure<OrderDetailDto>(validationError);
        }

        return await OrderServiceSupport.ExecuteAsync(async () =>
        {
            Cliente? customer = await _userRepository.GetCustomerByIdAsync(command.CustomerId, cancellationToken);
            if (customer is null)
            {
                return Result.Failure<OrderDetailDto>(
                    Error.NotFound("Orders.CustomerNotFound", $"No se encontró un cliente con identificador '{command.CustomerId}'."));
            }

            CarritoCompra? cart = await _cartRepository.GetByIdAsync(command.CartId, cancellationToken);
            if (cart is null)
            {
                return Result.Failure<OrderDetailDto>(
                    Error.NotFound("Orders.CartNotFound", $"No se encontró un carrito con identificador '{command.CartId}'."));
            }

            if (cart.ClienteId != command.CustomerId)
            {
                return Result.Failure<OrderDetailDto>(
                    Error.Validation("Orders.CartCustomerMismatch", "El carrito indicado no pertenece al cliente informado."));
            }

            if (!cart.Activo)
            {
                return Result.Failure<OrderDetailDto>(
                    Error.Conflict("Orders.CartInactive", "No es posible crear un pedido a partir de un carrito inactivo."));
            }

            if (!cart.TieneItems())
            {
                return Result.Failure<OrderDetailDto>(
                    Error.Validation("Orders.EmptyCart", "No es posible crear un pedido a partir de un carrito vacío."));
            }

            Pedido order = new(cart);

            if (order.ContieneProductosFisicos())
            {
                if (!command.HasShippingAddress)
                {
                    return Result.Failure<OrderDetailDto>(
                        Error.Validation("Orders.ShippingAddressRequired", "Debes informar una dirección de envío para pedidos con productos físicos."));
                }

                order.AsignarDireccionEnvio(new DireccionEnvio(
                    command.ShippingStreet!,
                    command.ShippingCity!,
                    command.ShippingRegion!,
                    command.ShippingCountry!,
                    command.ShippingPostalCode!));
            }

            order.SeleccionarMetodoPago(command.PaymentMethod!.Value);
            order.Confirmar();

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                await _orderRepository.AddAsync(order, cancellationToken);
                cart.VaciarCarrito();
                await _cartRepository.UpdateAsync(cart, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }

            await OrderServiceSupport.AuditOrderEventAsync(
                _auditTrailService,
                order,
                "order.created",
                $"Se creó un pedido a partir del carrito '{cart.Id}'.",
                new Dictionary<string, string>
                {
                    ["customerId"] = order.ClienteId.ToString(),
                    ["cartId"] = cart.Id.ToString(),
                    ["itemsCount"] = order.CantidadDetalles.ToString(),
                    ["totalAmount"] = order.Total.Amount.ToString(CultureInfo.InvariantCulture),
                    ["currency"] = order.Total.Currency,
                    ["paymentMethod"] = order.MetodoPagoSeleccionado?.ToString() ?? string.Empty,
                    ["hasShippingAddress"] = order.TieneDireccionEnvio().ToString()
                },
                cancellationToken);

            Result emailResult = await _emailNotificationService.SendOrderConfirmationEmailAsync(
                new OrderConfirmationEmailNotification
                {
                    ToEmail = customer.CorreoElectronico.Value,
                    RecipientName = customer.Nombre,
                    OrderId = order.Id,
                    TotalAmount = order.Total.Amount,
                    Currency = order.Total.Currency,
                    PaymentMethod = order.MetodoPagoSeleccionado,
                    ShippingAddressSummary = order.DireccionEnvio is null
                        ? null
                        : $"{order.DireccionEnvio.Calle}, {order.DireccionEnvio.Ciudad}, {order.DireccionEnvio.Departamento}, {order.DireccionEnvio.Pais}, {order.DireccionEnvio.CodigoPostal}",
                    Items = order.Detalles
                        .Select(detail => new OrderConfirmationEmailItem
                        {
                            ProductName = detail.NombreProducto,
                            ProductSku = detail.SkuProducto.Value,
                            Quantity = detail.Cantidad,
                            Subtotal = detail.Subtotal.Amount,
                            Currency = detail.Subtotal.Currency
                        })
                        .ToArray()
                },
                cancellationToken);

            if (emailResult.IsFailure)
            {
                await OrderServiceSupport.AuditOrderEventAsync(
                    _auditTrailService,
                    order,
                    "order.confirmation-email.failed",
                    $"No fue posible entregar el correo de confirmación del pedido '{order.Id}'.",
                    new Dictionary<string, string>
                    {
                        ["email"] = customer.CorreoElectronico.Value,
                        ["errorCode"] = emailResult.Error.Code
                    },
                    cancellationToken);
            }
            else
            {
                await OrderServiceSupport.AuditOrderEventAsync(
                    _auditTrailService,
                    order,
                    "order.confirmation-email.sent",
                    $"Se envió el correo de confirmación del pedido '{order.Id}'.",
                    new Dictionary<string, string>
                    {
                        ["email"] = customer.CorreoElectronico.Value,
                        ["paymentMethod"] = order.MetodoPagoSeleccionado?.ToString() ?? string.Empty
                    },
                    cancellationToken);
            }

            return Result.Success(order.ToOrderDetailDto());
        }, "Orders.Domain");
    }
}
