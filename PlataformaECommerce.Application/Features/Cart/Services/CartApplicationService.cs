using FluentValidation.Results;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Cart.Commands;
using PlataformaECommerce.Application.Features.Cart.DTOs;
using PlataformaECommerce.Application.Features.Cart.Queries;
using PlataformaECommerce.Application.Features.Cart.Validators;
using PlataformaECommerce.Application.Interfaces.Persistence;
using PlataformaECommerce.Application.Interfaces.Repositories.Cart;
using PlataformaECommerce.Application.Interfaces.Repositories.Products;
using PlataformaECommerce.Application.Interfaces.Repositories.Users;
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
/// Este servicio no reemplaza a handlers CQRS, pero constituye una capa
/// de orquestación válida y profesional para centralizar los principales
/// casos de uso del módulo de carrito.
/// </remarks>
public sealed class CartApplicationService
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

    #endregion

    #region Constructor

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="CartApplicationService"/>.
    /// </summary>
    /// <param name="cartRepository">Repositorio de carritos.</param>
    /// <param name="productRepository">Repositorio de productos.</param>
    /// <param name="userRepository">Repositorio de usuarios.</param>
    /// <param name="unitOfWork">Unidad de trabajo.</param>
    public CartApplicationService(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
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
            }

            return Result.Success(MapToCartDto(existingCart));
        }

        CarritoCompra cart = new(command.CustomerId);

        if (!command.IsActive)
        {
            cart.Desactivar();
        }

        await _cartRepository.AddAsync(cart, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToCartDto(cart));
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

        ValidationResult validationResult = await new AddProductToCartCommandValidator()
            .ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return Result.Failure<CartDto>(BuildValidationError(validationResult, "Cart.Validation"));
        }

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

        return Result.Success(MapToCartDto(cart));
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

        ValidationResult validationResult = await new UpdateCartItemQuantityCommandValidator()
            .ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return Result.Failure<CartDto>(BuildValidationError(validationResult, "Cart.Validation"));
        }

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

        return Result.Success(MapToCartDto(cart));
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

        return Result.Success(MapToCartDto(cart));
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

        CarritoCompra? cart = await _cartRepository.GetByIdAsync(command.CartId, cancellationToken);
        if (cart is null)
        {
            return Result.Failure<CartDto>(
                Error.NotFound("Cart.NotFound", $"No se encontró un carrito con identificador '{command.CartId}'."));
        }

        cart.VaciarCarrito();

        await _cartRepository.UpdateAsync(cart, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToCartDto(cart));
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

        return Result.Success(MapToCartDto(cart));
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

    /// <summary>
    /// Construye un error de validación de aplicación a partir del resultado de FluentValidation.
    /// </summary>
    /// <param name="validationResult">Resultado de validación.</param>
    /// <param name="errorCode">Código base del error.</param>
    /// <returns>Error de validación estructurado.</returns>
    private static Error BuildValidationError(ValidationResult validationResult, string errorCode)
    {
        string message = string.Join(
            " | ",
            validationResult.Errors
                .Where(error => !string.IsNullOrWhiteSpace(error.ErrorMessage))
                .Select(error => error.ErrorMessage.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase));

        return Error.Validation(
            errorCode,
            string.IsNullOrWhiteSpace(message)
                ? "La solicitud del carrito contiene errores de validación."
                : message);
    }

    /// <summary>
    /// Proyecta una entidad de dominio <see cref="CarritoCompra"/> hacia un <see cref="CartDto"/>.
    /// </summary>
    /// <param name="cart">Carrito a proyectar.</param>
    /// <returns>DTO del carrito.</returns>
    private static CartDto MapToCartDto(CarritoCompra cart)
    {
        return new CartDto
        {
            Id = cart.Id,
            CustomerId = cart.ClienteId,
            IsActive = cart.Activo,
            Items = cart.Items.Select(MapToCartItemDto).ToArray(),
            ItemsCount = cart.CantidadItems,
            TotalUnits = cart.CantidadTotalUnidades,
            TotalAmount = cart.Total.Amount,
            Currency = cart.Total.Currency,
            CreatedAtUtc = cart.FechaCreacionUtc,
            UpdatedAtUtc = cart.FechaActualizacionUtc
        };
    }

    /// <summary>
    /// Proyecta una entidad de dominio <see cref="ItemCarrito"/> hacia un <see cref="CartItemDto"/>.
    /// </summary>
    /// <param name="item">Ítem del carrito a proyectar.</param>
    /// <returns>DTO del ítem del carrito.</returns>
    private static CartItemDto MapToCartItemDto(ItemCarrito item)
    {
        return new CartItemDto
        {
            Id = item.Id,
            ProductId = item.ProductoId,
            ProductName = item.NombreProducto,
            ProductSku = item.SkuProducto.Value,
            ProductType = item.TipoProducto,
            MainImageUrl = item.ImagenPrincipalUrl,
            Quantity = item.Cantidad,
            UnitPrice = item.PrecioUnitario.Amount,
            Currency = item.PrecioUnitario.Currency,
            Subtotal = item.Subtotal.Amount,
            CreatedAtUtc = item.FechaCreacionUtc,
            UpdatedAtUtc = item.FechaActualizacionUtc
        };
    }

    #endregion
}