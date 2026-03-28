using System.Globalization;
using FluentValidation;
using PlataformaECommerce.Application.Common.Execution;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Orders.Commands;
using PlataformaECommerce.Application.Features.Orders.DTOs;
using PlataformaECommerce.Application.Features.Orders.Queries;
using PlataformaECommerce.Application.Features.Orders.Validators;
using PlataformaECommerce.Application.Interfaces.Services.Audit;
using PlataformaECommerce.Application.Interfaces.Persistence;
using PlataformaECommerce.Application.Interfaces.Repositories.Cart;
using PlataformaECommerce.Application.Interfaces.Repositories.Orders;
using PlataformaECommerce.Application.Interfaces.Repositories.Users;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Application.Interfaces.Services.Orders;
using PlataformaECommerce.Application.Features.Orders.Mappings;
using PlataformaECommerce.Domain.Entities.Cart;
using PlataformaECommerce.Domain.Entities.Orders;
using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Application.Features.Orders.Services;

/// <summary>
/// Proporciona los casos de uso de aplicación relacionados con la gestión
/// del ciclo de vida de los pedidos dentro del sistema.
/// </summary>
/// <remarks>
/// Esta clase actúa como servicio de aplicación para coordinar operaciones
/// de lectura y escritura sobre el agregado <see cref="Pedido"/>, manteniendo
/// una separación clara entre:
/// 
/// - validaciones estructurales de la capa Application,
/// - reglas de negocio propias del dominio,
/// - persistencia a través de repositorios,
/// - control transaccional mediante unidad de trabajo,
/// - y proyección de entidades hacia DTOs.
///
/// Su propósito es centralizar, de forma profesional y consistente, los
/// principales casos de uso del módulo de pedidos, incluyendo:
/// 
/// - creación de pedidos desde carrito,
/// - confirmación del pedido,
/// - registro de pago,
/// - procesamiento operativo,
/// - despacho,
/// - entrega,
/// - cancelación,
/// - consulta por identificador,
/// - y consulta de historial por cliente.
///
/// Este servicio constituye la implementación pública de los casos de uso del
/// módulo de pedidos, utilizando comandos y consultas como modelos de entrada
/// y manteniendo una frontera estable para las capas consumidoras.
/// </remarks>
public sealed class OrderApplicationService : IOrderApplicationService
{
    #region Campos privados

    /// <summary>
    /// Repositorio de pedidos.
    /// </summary>
    private readonly IOrderRepository _orderRepository;

    /// <summary>
    /// Repositorio de carritos.
    /// </summary>
    private readonly ICartRepository _cartRepository;

    /// <summary>
    /// Repositorio de usuarios.
    /// </summary>
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Unidad de trabajo asociada a la persistencia.
    /// </summary>
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Servicio transversal de auditoría.
    /// </summary>
    private readonly IAuditTrailService _auditTrailService;

    private readonly IValidator<CreateOrderFromCartCommand> _createOrderFromCartCommandValidator;
    private readonly IValidator<CancelOrderCommand> _cancelOrderCommandValidator;

    #endregion

    #region Constructor

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="OrderApplicationService"/>.
    /// </summary>
    /// <param name="orderRepository">Repositorio de pedidos.</param>
    /// <param name="cartRepository">Repositorio de carritos.</param>
    /// <param name="userRepository">Repositorio de usuarios.</param>
    /// <param name="unitOfWork">Unidad de trabajo.</param>
    /// <param name="auditTrailService">Servicio transversal de auditoría.</param>
    public OrderApplicationService(
        IOrderRepository orderRepository,
        ICartRepository cartRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IAuditTrailService auditTrailService,
        IValidator<CreateOrderFromCartCommand> createOrderFromCartCommandValidator,
        IValidator<CancelOrderCommand> cancelOrderCommandValidator)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _auditTrailService = auditTrailService ?? throw new ArgumentNullException(nameof(auditTrailService));
        _createOrderFromCartCommandValidator = createOrderFromCartCommandValidator ?? throw new ArgumentNullException(nameof(createOrderFromCartCommandValidator));
        _cancelOrderCommandValidator = cancelOrderCommandValidator ?? throw new ArgumentNullException(nameof(cancelOrderCommandValidator));
    }

    #endregion

    #region Casos de uso de escritura

    /// <summary>
    /// Crea un nuevo pedido a partir de un carrito existente.
    /// </summary>
    /// <param name="command">Comando de creación del pedido desde carrito.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con el detalle del pedido creado cuando la operación es exitosa.
    /// </returns>
    public async Task<Result<OrderDetailDto>> CreateOrderFromCartAsync(
        CreateOrderFromCartCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = await ValidateAsync(command, _createOrderFromCartCommandValidator, cancellationToken);
        if (validationError is not null)
        {
            return Result.Failure<OrderDetailDto>(validationError);
        }

        return await ExecuteAsync(async () =>
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

            bool requiresShippingAddress = order.ContieneProductosFisicos();
            if (requiresShippingAddress && !command.HasShippingAddress)
            {
                return Result.Failure<OrderDetailDto>(
                    Error.Validation("Orders.ShippingAddressRequired", "La dirección de envío es obligatoria cuando el pedido contiene productos físicos."));
            }

            if (command.HasShippingAddress)
            {
                order.AsignarDireccionEnvio(CreateShippingAddress(command));
            }

            await _orderRepository.AddAsync(order, cancellationToken);

            cart.VaciarCarrito();
            await _cartRepository.UpdateAsync(cart, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await AuditOrderEventAsync(
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
                    ["hasShippingAddress"] = order.TieneDireccionEnvio().ToString()
                },
                cancellationToken);

            return Result.Success(order.ToOrderDetailDto());
        }, "Orders.Domain");
    }

    /// <summary>
    /// Confirma un pedido existente.
    /// </summary>
    /// <param name="command">Comando de confirmación del pedido.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con el detalle actualizado del pedido cuando la operación es exitosa.
    /// </returns>
    public async Task<Result<OrderDetailDto>> ConfirmOrderAsync(
        ConfirmOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.OrderId == Guid.Empty)
        {
            return Result.Failure<OrderDetailDto>(
                Error.Validation("Orders.InvalidId", "El identificador del pedido es obligatorio."));
        }

        return await ExecuteAsync(async () =>
        {
            Pedido? order = await _orderRepository.GetByIdAsync(command.OrderId, cancellationToken);
            if (order is null)
            {
                return Result.Failure<OrderDetailDto>(
                    Error.NotFound("Orders.NotFound", $"No se encontró un pedido con identificador '{command.OrderId}'."));
            }

            order.Confirmar();

            await _orderRepository.UpdateAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await AuditOrderEventAsync(
                order,
                "order.confirmed",
                $"Se confirmó el pedido '{order.Id}'.",
                new Dictionary<string, string>
                {
                    ["status"] = order.Estado.ToString(),
                    ["totalAmount"] = order.Total.Amount.ToString(CultureInfo.InvariantCulture),
                    ["currency"] = order.Total.Currency
                },
                cancellationToken);

            return Result.Success(order.ToOrderDetailDto());
        }, "Orders.Domain");
    }

    /// <summary>
    /// Registra el pago exitoso de un pedido.
    /// </summary>
    /// <param name="command">Comando de registro de pago.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con el detalle actualizado del pedido cuando la operación es exitosa.
    /// </returns>
    public async Task<Result<OrderDetailDto>> RegisterOrderPaymentAsync(
        RegisterOrderPaymentCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = ValidateRegisterPaymentCommand(command);
        if (validationError is not null)
        {
            return Result.Failure<OrderDetailDto>(validationError);
        }

        return await ExecuteAsync(async () =>
        {
            Pedido? order = await _orderRepository.GetByIdAsync(command.OrderId, cancellationToken);
            if (order is null)
            {
                return Result.Failure<OrderDetailDto>(
                    Error.NotFound("Orders.NotFound", $"No se encontró un pedido con identificador '{command.OrderId}'."));
            }

            if (!string.Equals(order.Total.Currency, command.Currency?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return Result.Failure<OrderDetailDto>(
                    Error.Validation(
                        "Orders.PaymentCurrencyMismatch",
                        $"La moneda del pago '{command.Currency}' no coincide con la moneda del pedido '{order.Total.Currency}'."));
            }

            if (command.Amount != order.Total.Amount)
            {
                return Result.Failure<OrderDetailDto>(
                    Error.Validation(
                        "Orders.PaymentAmountMismatch",
                        $"El valor pagado '{command.Amount:N2}' no coincide con el total del pedido '{order.Total.Amount:N2}'."));
            }

            order.RegistrarPago();

            await _orderRepository.UpdateAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await AuditOrderEventAsync(
                order,
                "order.payment.registered",
                $"Se registró el pago del pedido '{order.Id}'.",
                new Dictionary<string, string>
                {
                    ["paymentReference"] = command.PaymentReference.Trim(),
                    ["paymentMethod"] = command.PaymentMethod.Trim(),
                    ["amount"] = command.Amount.ToString(CultureInfo.InvariantCulture),
                    ["currency"] = command.Currency.Trim().ToUpperInvariant(),
                    ["status"] = order.Estado.ToString()
                },
                cancellationToken);

            return Result.Success(order.ToOrderDetailDto());
        }, "Orders.Domain");
    }

    /// <summary>
    /// Marca un pedido como en proceso operativo.
    /// </summary>
    /// <param name="command">Comando de paso a procesamiento.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con el detalle actualizado del pedido cuando la operación es exitosa.
    /// </returns>
    public async Task<Result<OrderDetailDto>> ProcessOrderAsync(
        ProcessOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.OrderId == Guid.Empty)
        {
            return Result.Failure<OrderDetailDto>(
                Error.Validation("Orders.InvalidId", "El identificador del pedido es obligatorio."));
        }

        return await ExecuteAsync(async () =>
        {
            Pedido? order = await _orderRepository.GetByIdAsync(command.OrderId, cancellationToken);
            if (order is null)
            {
                return Result.Failure<OrderDetailDto>(
                    Error.NotFound("Orders.NotFound", $"No se encontró un pedido con identificador '{command.OrderId}'."));
            }

            order.MarcarEnProceso();

            await _orderRepository.UpdateAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await AuditOrderEventAsync(
                order,
                "order.processing.started",
                $"El pedido '{order.Id}' pasó a estado en proceso.",
                new Dictionary<string, string>
                {
                    ["status"] = order.Estado.ToString()
                },
                cancellationToken);

            return Result.Success(order.ToOrderDetailDto());
        }, "Orders.Domain");
    }

    /// <summary>
    /// Marca un pedido como enviado o despachado.
    /// </summary>
    /// <param name="command">Comando de envío del pedido.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con el detalle actualizado del pedido cuando la operación es exitosa.
    /// </returns>
    public async Task<Result<OrderDetailDto>> ShipOrderAsync(
        ShipOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.OrderId == Guid.Empty)
        {
            return Result.Failure<OrderDetailDto>(
                Error.Validation("Orders.InvalidId", "El identificador del pedido es obligatorio."));
        }

        if (string.IsNullOrWhiteSpace(command.CarrierName))
        {
            return Result.Failure<OrderDetailDto>(
                Error.Validation("Orders.InvalidCarrierName", "El nombre del transportador es obligatorio para despachar el pedido."));
        }

        if (string.IsNullOrWhiteSpace(command.TrackingNumber))
        {
            return Result.Failure<OrderDetailDto>(
                Error.Validation("Orders.InvalidTrackingNumber", "El número de guía o seguimiento es obligatorio para despachar el pedido."));
        }

        return await ExecuteAsync(async () =>
        {
            Pedido? order = await _orderRepository.GetByIdAsync(command.OrderId, cancellationToken);
            if (order is null)
            {
                return Result.Failure<OrderDetailDto>(
                    Error.NotFound("Orders.NotFound", $"No se encontró un pedido con identificador '{command.OrderId}'."));
            }

            order.MarcarEnviado();

            await _orderRepository.UpdateAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await AuditOrderEventAsync(
                order,
                "order.shipped",
                $"Se despachó el pedido '{order.Id}'.",
                new Dictionary<string, string>
                {
                    ["carrierName"] = command.CarrierName.Trim(),
                    ["trackingNumber"] = command.TrackingNumber.Trim(),
                    ["status"] = order.Estado.ToString(),
                    ["hasShippingAddress"] = order.TieneDireccionEnvio().ToString()
                },
                cancellationToken);

            return Result.Success(order.ToOrderDetailDto());
        }, "Orders.Domain");
    }

    /// <summary>
    /// Marca un pedido como entregado.
    /// </summary>
    /// <param name="command">Comando de entrega del pedido.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con el detalle actualizado del pedido cuando la operación es exitosa.
    /// </returns>
    public async Task<Result<OrderDetailDto>> DeliverOrderAsync(
        DeliverOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.OrderId == Guid.Empty)
        {
            return Result.Failure<OrderDetailDto>(
                Error.Validation("Orders.InvalidId", "El identificador del pedido es obligatorio."));
        }

        return await ExecuteAsync(async () =>
        {
            Pedido? order = await _orderRepository.GetByIdAsync(command.OrderId, cancellationToken);
            if (order is null)
            {
                return Result.Failure<OrderDetailDto>(
                    Error.NotFound("Orders.NotFound", $"No se encontró un pedido con identificador '{command.OrderId}'."));
            }

            order.MarcarEntregado();

            await _orderRepository.UpdateAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await AuditOrderEventAsync(
                order,
                "order.delivered",
                $"Se entregó el pedido '{order.Id}'.",
                new Dictionary<string, string>
                {
                    ["status"] = order.Estado.ToString()
                },
                cancellationToken);

            return Result.Success(order.ToOrderDetailDto());
        }, "Orders.Domain");
    }

    /// <summary>
    /// Cancela un pedido existente.
    /// </summary>
    /// <param name="command">Comando de cancelación del pedido.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con el detalle actualizado del pedido cuando la operación es exitosa.
    /// </returns>
    public async Task<Result<OrderDetailDto>> CancelOrderAsync(
        CancelOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = await ValidateAsync(command, _cancelOrderCommandValidator, cancellationToken);
        if (validationError is not null)
        {
            return Result.Failure<OrderDetailDto>(validationError);
        }

        return await ExecuteAsync(async () =>
        {
            Pedido? order = await _orderRepository.GetByIdAsync(command.OrderId, cancellationToken);
            if (order is null)
            {
                return Result.Failure<OrderDetailDto>(
                    Error.NotFound("Orders.NotFound", $"No se encontró un pedido con identificador '{command.OrderId}'."));
            }

            order.Cancelar(command.Reason);

            await _orderRepository.UpdateAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await AuditOrderEventAsync(
                order,
                "order.cancelled",
                $"Se canceló el pedido '{order.Id}'.",
                new Dictionary<string, string>
                {
                    ["reason"] = command.Reason.Trim(),
                    ["status"] = order.Estado.ToString()
                },
                cancellationToken);

            return Result.Success(order.ToOrderDetailDto());
        }, "Orders.Domain");
    }

    #endregion

    #region Casos de uso de lectura

    /// <summary>
    /// Obtiene el detalle de un pedido a partir de su identificador.
    /// </summary>
    /// <param name="query">Consulta de pedido por identificador.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con el detalle del pedido cuando la operación es exitosa.
    /// </returns>
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

    /// <summary>
    /// Obtiene los pedidos asociados a un cliente específico,
    /// aplicando filtros, ordenamiento y paginación en memoria.
    /// </summary>
    /// <param name="query">Consulta de pedidos por cliente.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con la colección de pedidos cuando la operación es exitosa.
    /// </returns>
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

        IEnumerable<Pedido> filteredOrders = ApplyOrdersFilter(orders, query);
        IEnumerable<Pedido> orderedOrders = ApplyOrdersSorting(filteredOrders, query);

        IReadOnlyCollection<OrderDto> result = orderedOrders
            .Skip(query.Offset)
            .Take(query.NormalizedPageSize)
            .ToOrderDtos(query.IncludeItems)
            .ToArray();

        return Result.Success(result);
    }

    #endregion

    #region Métodos privados auxiliares

    private static Task<Error?> ValidateAsync<TCommand>(
        TCommand command,
        IValidator<TCommand> validator,
        CancellationToken cancellationToken)
    {
        return ApplicationExecution.ValidateAsync(
            command,
            validator,
            "Orders.Validation",
            "La solicitud del pedido contiene errores de validación.",
            cancellationToken);
    }

    private static Task<Result<TResponse>> ExecuteAsync<TResponse>(
        Func<Task<Result<TResponse>>> operation,
        string errorCode)
    {
        return ApplicationExecution.ExecuteAsync(operation, errorCode);
    }

    /// <summary>
    /// Valida estructuralmente el comando de registro de pago.
    /// </summary>
    /// <param name="command">Comando a validar.</param>
    /// <returns>
    /// Un error de validación cuando la estructura es inválida;
    /// en caso contrario, <see langword="null"/>.
    /// </returns>
    private static Error? ValidateRegisterPaymentCommand(RegisterOrderPaymentCommand command)
    {
        if (command.OrderId == Guid.Empty)
        {
            return Error.Validation("Orders.InvalidId", "El identificador del pedido es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(command.PaymentReference))
        {
            return Error.Validation("Orders.InvalidPaymentReference", "La referencia del pago es obligatoria.");
        }

        if (string.IsNullOrWhiteSpace(command.PaymentMethod))
        {
            return Error.Validation("Orders.InvalidPaymentMethod", "El método de pago es obligatorio.");
        }

        if (command.Amount <= 0)
        {
            return Error.Validation("Orders.InvalidPaymentAmount", "El monto del pago debe ser mayor que cero.");
        }

        if (string.IsNullOrWhiteSpace(command.Currency))
        {
            return Error.Validation("Orders.InvalidPaymentCurrency", "La moneda del pago es obligatoria.");
        }

        return null;
    }

    /// <summary>
    /// Construye la dirección de envío asociada al checkout cuando la solicitud la contiene.
    /// </summary>
    private static DireccionEnvio CreateShippingAddress(CreateOrderFromCartCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        return new DireccionEnvio(
            command.ShippingStreet!.Trim(),
            command.ShippingCity!.Trim(),
            command.ShippingDepartment!.Trim(),
            command.ShippingCountry!.Trim(),
            command.ShippingPostalCode!.Trim());
    }

    /// <summary>
    /// Aplica los filtros funcionales definidos en la consulta de pedidos por cliente.
    /// </summary>
    /// <param name="orders">Colección base de pedidos.</param>
    /// <param name="query">Consulta con criterios de filtrado.</param>
    /// <returns>Colección filtrada de pedidos.</returns>
    private static IEnumerable<Pedido> ApplyOrdersFilter(
        IEnumerable<Pedido> orders,
        GetOrdersByCustomerIdQuery query)
    {
        IEnumerable<Pedido> filteredOrders = orders;

        if (query.CreatedFromUtc.HasValue)
        {
            filteredOrders = filteredOrders.Where(order => order.FechaCreacionUtc >= query.CreatedFromUtc.Value);
        }

        if (query.CreatedToUtc.HasValue)
        {
            filteredOrders = filteredOrders.Where(order => order.FechaCreacionUtc <= query.CreatedToUtc.Value);
        }

        if (query.MinTotalAmount.HasValue)
        {
            filteredOrders = filteredOrders.Where(order => order.Total.Amount >= query.MinTotalAmount.Value);
        }

        if (query.MaxTotalAmount.HasValue)
        {
            filteredOrders = filteredOrders.Where(order => order.Total.Amount <= query.MaxTotalAmount.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Currency))
        {
            filteredOrders = filteredOrders.Where(order =>
                string.Equals(order.Total.Currency, query.Currency.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (query.OnlyFinalized == true)
        {
            filteredOrders = filteredOrders.Where(order => order.EstaFinalizado());
        }

        if (query.OnlyActive == true)
        {
            filteredOrders = filteredOrders.Where(order => !order.EstaFinalizado());
        }

        return filteredOrders;
    }

    /// <summary>
    /// Aplica el criterio de ordenamiento solicitado a la colección de pedidos.
    /// </summary>
    /// <param name="orders">Colección de pedidos a ordenar.</param>
    /// <param name="query">Consulta que contiene el criterio de ordenamiento.</param>
    /// <returns>Colección ordenada de pedidos.</returns>
    private static IEnumerable<Pedido> ApplyOrdersSorting(
        IEnumerable<Pedido> orders,
        GetOrdersByCustomerIdQuery query)
    {
        string sortBy = query.SortBy?.Trim().ToLowerInvariant() ?? "createdat";

        return sortBy switch
        {
            "totalamount" or "total" => query.SortDescending
                ? orders.OrderByDescending(order => order.Total.Amount).ThenByDescending(order => order.FechaCreacionUtc)
                : orders.OrderBy(order => order.Total.Amount).ThenBy(order => order.FechaCreacionUtc),

            "status" => query.SortDescending
                ? orders.OrderByDescending(order => order.Estado).ThenByDescending(order => order.FechaCreacionUtc)
                : orders.OrderBy(order => order.Estado).ThenBy(order => order.FechaCreacionUtc),

            "updatedat" => query.SortDescending
                ? orders.OrderByDescending(order => order.FechaActualizacionUtc ?? order.FechaCreacionUtc)
                : orders.OrderBy(order => order.FechaActualizacionUtc ?? order.FechaCreacionUtc),

            _ => query.SortDescending
                ? orders.OrderByDescending(order => order.FechaCreacionUtc)
                : orders.OrderBy(order => order.FechaCreacionUtc)
        };
    }

    /// <summary>
    /// Registra un evento de auditoría asociado a una operación exitosa sobre pedidos.
    /// </summary>
    /// <param name="order">Pedido afectado por la operación.</param>
    /// <param name="action">Acción semántica auditada.</param>
    /// <param name="detail">Detalle legible del evento.</param>
    /// <param name="metadata">Metadatos complementarios del evento.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    private Task AuditOrderEventAsync(
        Pedido order,
        string action,
        string detail,
        IReadOnlyDictionary<string, string>? metadata,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(order);

        return _auditTrailService.RegisterAsync(
            order.Id,
            nameof(Pedido),
            "Orders",
            action,
            detail,
            metadata,
            cancellationToken);
    }

    #endregion
}