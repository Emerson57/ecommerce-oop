using System.Globalization;
using FluentValidation;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Orders.Commands;
using PlataformaECommerce.Application.Features.Orders.DTOs;
using PlataformaECommerce.Application.Features.Orders.Mappings;
using PlataformaECommerce.Application.Interfaces.Persistence;
using PlataformaECommerce.Application.Interfaces.Repositories.Cart;
using PlataformaECommerce.Application.Interfaces.Repositories.Orders;
using PlataformaECommerce.Application.Interfaces.Repositories.Users;
using PlataformaECommerce.Application.Interfaces.Services.Audit;
using PlataformaECommerce.Application.Interfaces.Services.Orders;
using PlataformaECommerce.Domain.Entities.Cart;
using PlataformaECommerce.Domain.Entities.Orders;
using PlataformaECommerce.Domain.Entities.Users;

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
        IValidator<CreateOrderFromCartCommand> createOrderFromCartCommandValidator)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _auditTrailService = auditTrailService ?? throw new ArgumentNullException(nameof(auditTrailService));
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

            await _orderRepository.AddAsync(order, cancellationToken);
            cart.VaciarCarrito();
            await _cartRepository.UpdateAsync(cart, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
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
                    ["currency"] = order.Total.Currency
                },
                cancellationToken);

            return Result.Success(order.ToOrderDetailDto());
        }, "Orders.Domain");
    }
}
