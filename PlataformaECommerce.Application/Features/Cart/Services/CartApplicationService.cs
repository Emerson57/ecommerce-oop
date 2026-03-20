using FluentValidation;
using PlataformaECommerce.Application.Common.Execution;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Cart.Commands;
using PlataformaECommerce.Application.Features.Cart.DTOs;
using PlataformaECommerce.Application.Features.Cart.Queries;
using PlataformaECommerce.Application.Features.Cart.Validators;
using PlataformaECommerce.Application.Interfaces.Services.Audit;
using PlataformaECommerce.Application.Interfaces.Persistence;
using PlataformaECommerce.Application.Interfaces.Repositories.Cart;
using PlataformaECommerce.Application.Interfaces.Repositories.Products;
using PlataformaECommerce.Application.Interfaces.Repositories.Users;
using PlataformaECommerce.Application.Interfaces.Services.Cart;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Application.Features.Cart.Mappings;
using PlataformaECommerce.Domain.Entities.Cart;
using PlataformaECommerce.Domain.Entities.Products;
using PlataformaECommerce.Domain.Entities.Users;

namespace PlataformaECommerce.Application.Features.Cart.Services;

/// <summary>
/// Proporciona los casos de uso de aplicación relacionados con la gestión
/// del carrito de compras dentro del sistema.
/// </summary>
/// <remarks>
/// Esta clase coordina la ejecución de operaciones de lectura y escritura
/// sobre el agregado de carrito, actuando como servicio de aplicación.
///
/// Su responsabilidad incluye:
/// - validación estructural de comandos y consultas,
/// - coordinación con repositorios,
/// - control de persistencia mediante unidad de trabajo,
/// - transformación de datos hacia DTOs,
/// - y orquestación de acciones de negocio sin invadir el dominio.
///
/// Este servicio constituye la implementación pública de los casos de uso del
/// módulo de carrito, utilizando comandos y consultas como modelos de entrada
/// para orquestar operaciones coherentes dentro de <c>Application</c>.
/// </remarks>
public sealed class CartApplicationService : ICartApplicationService
{
    #region Campos privados

    /// <summary>
    /// Repositorio de carritos.
    /// </summary>
    private readonly ICartRepository _cartRepository;

    /// <summary>
    /// Repositorio de productos.
    /// </summary>
    private readonly IProductRepository _productRepository;

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

    private readonly IValidator<AddProductToCartCommand> _addProductToCartCommandValidator;
    private readonly IValidator<UpdateCartItemQuantityCommand> _updateCartItemQuantityCommandValidator;

    #endregion

    #region Constructor

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="CartApplicationService"/>.
    /// </summary>
    /// <param name="cartRepository">Repositorio de carritos.</param>
    /// <param name="productRepository">Repositorio de productos.</param>
    /// <param name="userRepository">Repositorio de usuarios.</param>
    /// <param name="unitOfWork">Unidad de trabajo.</param>
    /// <param name="auditTrailService">Servicio transversal de auditoría.</param>
    public CartApplicationService(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IAuditTrailService auditTrailService,
        IValidator<AddProductToCartCommand> addProductToCartCommandValidator,
        IValidator<UpdateCartItemQuantityCommand> updateCartItemQuantityCommandValidator)
    {
        _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _auditTrailService = auditTrailService ?? throw new ArgumentNullException(nameof(auditTrailService));
        _addProductToCartCommandValidator = addProductToCartCommandValidator ?? throw new ArgumentNullException(nameof(addProductToCartCommandValidator));
        _updateCartItemQuantityCommandValidator = updateCartItemQuantityCommandValidator ?? throw new ArgumentNullException(nameof(updateCartItemQuantityCommandValidator));
    }

    #endregion

    #region Casos de uso de escritura

    /// <summary>
    /// Crea un nuevo carrito para un cliente específico.
    /// </summary>
    /// <param name="command">Comando de creación de carrito.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con la representación del carrito creado cuando la operación es exitosa.
    /// </returns>
    public async Task<Result<CartDto>> CreateCartAsync(
        CreateCartCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.CustomerId == Guid.Empty)
        {
            return Result.Failure<CartDto>(
                Error.Validation("Cart.InvalidCustomerId", "El identificador del cliente es obligatorio."));
        }

        return await ExecuteAsync(async () =>
        {
            Cliente? customer = await _userRepository.GetCustomerByIdAsync(command.CustomerId, cancellationToken);
            if (customer is null)
            {
                return Result.Failure<CartDto>(
                    Error.NotFound("Cart.CustomerNotFound", $"No se encontró un cliente con identificador '{command.CustomerId}'."));
            }

            CarritoCompra? existingCart = await _cartRepository.GetByCustomerIdAsync(command.CustomerId, cancellationToken);
            if (existingCart is not null)
            {
                if (command.IsActive && !existingCart.Activo)
                {
                    existingCart.Activar();
                    await _cartRepository.UpdateAsync(existingCart, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await AuditCartEventAsync(
                        existingCart,
                        "cart.reactivated",
                        $"Se reactivó el carrito del cliente '{existingCart.ClienteId}'.",
                        new Dictionary<string, string>
                        {
                            ["customerId"] = existingCart.ClienteId.ToString(),
                            ["isActive"] = existingCart.Activo.ToString()
                        },
                        cancellationToken);
                }

                return Result.Success(existingCart.ToCartDto());
            }

            CarritoCompra cart = new(command.CustomerId);

            if (!command.IsActive)
            {
                cart.Desactivar();
            }

            await _cartRepository.AddAsync(cart, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await AuditCartEventAsync(
                cart,
                "cart.created",
                $"Se creó un nuevo carrito para el cliente '{cart.ClienteId}'.",
                new Dictionary<string, string>
                {
                    ["customerId"] = cart.ClienteId.ToString(),
                    ["isActive"] = cart.Activo.ToString()
                },
                cancellationToken);

            return Result.Success(cart.ToCartDto());
        }, "Cart.Domain");
    }

    /// <summary>
    /// Agrega un producto a un carrito existente.
    /// </summary>
    /// <param name="command">Comando de agregado de producto al carrito.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con la representación actualizada del carrito cuando la operación es exitosa.
    /// </returns>
    public async Task<Result<CartDto>> AddProductToCartAsync(
        AddProductToCartCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = await ValidateAsync(command, _addProductToCartCommandValidator, cancellationToken);
        if (validationError is not null)
        {
            return Result.Failure<CartDto>(validationError);
        }

        return await ExecuteAsync(async () =>
        {
            CarritoCompra? cart = await _cartRepository.GetByIdAsync(command.CartId, cancellationToken);
            if (cart is null)
            {
                return Result.Failure<CartDto>(
                    Error.NotFound("Cart.NotFound", $"No se encontró un carrito con identificador '{command.CartId}'."));
            }

            Producto? product = await FindProductByIdAsync(command.ProductId, cancellationToken);
            if (product is null)
            {
                return Result.Failure<CartDto>(
                    Error.NotFound("Cart.ProductNotFound", $"No se encontró un producto con identificador '{command.ProductId}'."));
            }

            cart.AgregarProducto(product, command.Quantity);

            await _cartRepository.UpdateAsync(cart, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await AuditCartEventAsync(
                cart,
                "cart.product.added",
                $"Se agregó el producto '{product.Id}' al carrito '{cart.Id}'.",
                new Dictionary<string, string>
                {
                    ["productId"] = product.Id.ToString(),
                    ["quantity"] = command.Quantity.ToString(),
                    ["itemsCount"] = cart.CantidadItems.ToString(),
                    ["totalUnits"] = cart.CantidadTotalUnidades.ToString()
                },
                cancellationToken);

            return Result.Success(cart.ToCartDto());
        }, "Cart.Domain");
    }

    /// <summary>
    /// Actualiza la cantidad de un ítem existente dentro del carrito.
    /// </summary>
    /// <param name="command">Comando de actualización de cantidad.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con la representación actualizada del carrito cuando la operación es exitosa.
    /// </returns>
    public async Task<Result<CartDto>> UpdateCartItemQuantityAsync(
        UpdateCartItemQuantityCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = await ValidateAsync(command, _updateCartItemQuantityCommandValidator, cancellationToken);
        if (validationError is not null)
        {
            return Result.Failure<CartDto>(validationError);
        }

        return await ExecuteAsync(async () =>
        {
            CarritoCompra? cart = await _cartRepository.GetByIdAsync(command.CartId, cancellationToken);
            if (cart is null)
            {
                return Result.Failure<CartDto>(
                    Error.NotFound("Cart.NotFound", $"No se encontró un carrito con identificador '{command.CartId}'."));
            }

            Producto? product = await FindProductByIdAsync(command.ProductId, cancellationToken);
            if (product is null)
            {
                return Result.Failure<CartDto>(
                    Error.NotFound("Cart.ProductNotFound", $"No se encontró un producto con identificador '{command.ProductId}'."));
            }

            ItemCarrito? item = cart.ObtenerItemPorProductoId(command.ProductId);
            if (item is null)
            {
                return Result.Failure<CartDto>(
                    Error.NotFound("Cart.ItemNotFound", $"No se encontró un ítem asociado al producto '{command.ProductId}' dentro del carrito."));
            }

            if (command.CartItemId != item.Id)
            {
                return Result.Failure<CartDto>(
                    Error.Validation("Cart.ItemMismatch", "El identificador del ítem del carrito no corresponde al producto informado."));
            }

            if (command.IsRemovalRequest)
            {
                bool removed = cart.RemoverProducto(command.ProductId);

                if (!removed)
                {
                    return Result.Failure<CartDto>(
                        Error.NotFound("Cart.ItemNotFound", $"No fue posible remover el producto '{command.ProductId}' del carrito."));
                }
            }
            else
            {
                cart.ActualizarCantidadProducto(product, command.NewQuantity);
            }

            await _cartRepository.UpdateAsync(cart, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await AuditCartEventAsync(
                cart,
                command.IsRemovalRequest ? "cart.product.removed" : "cart.item.quantity.updated",
                command.IsRemovalRequest
                    ? $"Se removió el producto '{command.ProductId}' del carrito '{cart.Id}'."
                    : $"Se actualizó la cantidad del producto '{command.ProductId}' en el carrito '{cart.Id}'.",
                new Dictionary<string, string>
                {
                    ["productId"] = command.ProductId.ToString(),
                    ["cartItemId"] = command.CartItemId.ToString(),
                    ["newQuantity"] = command.NewQuantity.ToString(),
                    ["itemsCount"] = cart.CantidadItems.ToString(),
                    ["totalUnits"] = cart.CantidadTotalUnidades.ToString()
                },
                cancellationToken);

            return Result.Success(cart.ToCartDto());
        }, "Cart.Domain");
    }

    /// <summary>
    /// Remueve un producto del carrito.
    /// </summary>
    /// <param name="command">Comando de remoción de producto del carrito.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con la representación actualizada del carrito cuando la operación es exitosa.
    /// </returns>
    public async Task<Result<CartDto>> RemoveProductFromCartAsync(
        RemoveProductFromCartCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.CartId == Guid.Empty)
        {
            return Result.Failure<CartDto>(
                Error.Validation("Cart.InvalidId", "El identificador del carrito es obligatorio."));
        }

        if (command.ProductId == Guid.Empty)
        {
            return Result.Failure<CartDto>(
                Error.Validation("Cart.InvalidProductId", "El identificador del producto es obligatorio."));
        }

        return await ExecuteAsync(async () =>
        {
            CarritoCompra? cart = await _cartRepository.GetByIdAsync(command.CartId, cancellationToken);
            if (cart is null)
            {
                return Result.Failure<CartDto>(
                    Error.NotFound("Cart.NotFound", $"No se encontró un carrito con identificador '{command.CartId}'."));
            }

            if (command.HasCartItemId)
            {
                ItemCarrito? item = cart.ObtenerItemPorProductoId(command.ProductId);
                if (item is null || item.Id != command.CartItemId!.Value)
                {
                    return Result.Failure<CartDto>(
                        Error.NotFound("Cart.ItemNotFound", "No se encontró el ítem del carrito indicado para el producto especificado."));
                }
            }

            bool removed = cart.RemoverProducto(command.ProductId);
            if (!removed)
            {
                return Result.Failure<CartDto>(
                    Error.NotFound("Cart.ItemNotFound", $"No se encontró el producto '{command.ProductId}' dentro del carrito."));
            }

            await _cartRepository.UpdateAsync(cart, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await AuditCartEventAsync(
                cart,
                "cart.product.removed",
                $"Se removió el producto '{command.ProductId}' del carrito '{cart.Id}'.",
                new Dictionary<string, string>
                {
                    ["productId"] = command.ProductId.ToString(),
                    ["cartItemId"] = command.CartItemId?.ToString() ?? string.Empty,
                    ["itemsCount"] = cart.CantidadItems.ToString(),
                    ["totalUnits"] = cart.CantidadTotalUnidades.ToString()
                },
                cancellationToken);

            return Result.Success(cart.ToCartDto());
        }, "Cart.Domain");
    }

    /// <summary>
    /// Vacía completamente un carrito existente.
    /// </summary>
    /// <param name="command">Comando de vaciado del carrito.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con la representación actualizada del carrito cuando la operación es exitosa.
    /// </returns>
    public async Task<Result<CartDto>> ClearCartAsync(
        ClearCartCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.CartId == Guid.Empty)
        {
            return Result.Failure<CartDto>(
                Error.Validation("Cart.InvalidId", "El identificador del carrito es obligatorio."));
        }

        return await ExecuteAsync(async () =>
        {
            CarritoCompra? cart = await _cartRepository.GetByIdAsync(command.CartId, cancellationToken);
            if (cart is null)
            {
                return Result.Failure<CartDto>(
                    Error.NotFound("Cart.NotFound", $"No se encontró un carrito con identificador '{command.CartId}'."));
            }

            int previousItemsCount = cart.CantidadItems;
            int previousTotalUnits = cart.CantidadTotalUnidades;
            cart.VaciarCarrito();

            await _cartRepository.UpdateAsync(cart, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await AuditCartEventAsync(
                cart,
                "cart.cleared",
                $"Se vació completamente el carrito '{cart.Id}'.",
                new Dictionary<string, string>
                {
                    ["previousItemsCount"] = previousItemsCount.ToString(),
                    ["previousTotalUnits"] = previousTotalUnits.ToString()
                },
                cancellationToken);

            return Result.Success(cart.ToCartDto());
        }, "Cart.Domain");
    }

    #endregion

    #region Casos de uso de lectura

    /// <summary>
    /// Obtiene el carrito asociado a un cliente.
    /// </summary>
    /// <param name="query">Consulta de carrito por cliente.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con la representación del carrito cuando la operación es exitosa.
    /// </returns>
    public async Task<Result<CartDto>> GetCartByCustomerIdAsync(
        GetCartByCustomerIdQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.CustomerId == Guid.Empty)
        {
            return Result.Failure<CartDto>(
                Error.Validation("Cart.InvalidCustomerId", "El identificador del cliente es obligatorio."));
        }

        Cliente? customer = await _userRepository.GetCustomerByIdAsync(query.CustomerId, cancellationToken);
        if (customer is null)
        {
            return Result.Failure<CartDto>(
                Error.NotFound("Cart.CustomerNotFound", $"No se encontró un cliente con identificador '{query.CustomerId}'."));
        }

        CarritoCompra? cart = await _cartRepository.GetByCustomerIdAsync(query.CustomerId, cancellationToken);
        if (cart is null)
        {
            return Result.Failure<CartDto>(
                Error.NotFound("Cart.NotFoundByCustomer", $"No se encontró un carrito asociado al cliente '{query.CustomerId}'."));
        }

        if (query.OnlyActiveCart && !cart.Activo)
        {
            return Result.Failure<CartDto>(
                Error.NotFound("Cart.ActiveCartNotFound", $"No se encontró un carrito activo asociado al cliente '{query.CustomerId}'."));
        }

        return Result.Success(cart.ToCartDto());
    }

    #endregion

    #region Métodos privados auxiliares

    /// <summary>
    /// Busca un producto por su identificador dentro del repositorio actual.
    /// </summary>
    /// <param name="productId">Identificador del producto.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Producto encontrado o <see langword="null"/> si no existe.</returns>
    private async Task<Producto?> FindProductByIdAsync(Guid productId, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Producto> products = await _productRepository.GetAllAsync(cancellationToken);
        return products.FirstOrDefault(product => product.Id == productId);
    }

    private static Task<Error?> ValidateAsync<TCommand>(
        TCommand command,
        IValidator<TCommand> validator,
        CancellationToken cancellationToken)
    {
        return ApplicationExecution.ValidateAsync(
            command,
            validator,
            "Cart.Validation",
            "La solicitud del carrito contiene errores de validación.",
            cancellationToken);
    }

    private static Task<Result<TResponse>> ExecuteAsync<TResponse>(
        Func<Task<Result<TResponse>>> operation,
        string errorCode)
    {
        return ApplicationExecution.ExecuteAsync(operation, errorCode);
    }

    /// <summary>
    /// Registra un evento de auditoría asociado a una operación exitosa sobre carritos.
    /// </summary>
    /// <param name="cart">Carrito afectado por la operación.</param>
    /// <param name="action">Acción semántica auditada.</param>
    /// <param name="detail">Detalle legible del evento.</param>
    /// <param name="metadata">Metadatos complementarios del evento.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    private Task AuditCartEventAsync(
        CarritoCompra cart,
        string action,
        string detail,
        IReadOnlyDictionary<string, string>? metadata,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cart);

        return _auditTrailService.RegisterAsync(
            cart.Id,
            nameof(CarritoCompra),
            "Cart",
            action,
            detail,
            metadata,
            cancellationToken);
    }

    #endregion
}