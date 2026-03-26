using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Cart.Commands;
using PlataformaECommerce.Application.Features.Cart.DTOs;
using PlataformaECommerce.Application.Features.Cart.Queries;

namespace PlataformaECommerce.Application.Interfaces.Services.Cart;

/// <summary>
/// Define el contrato del servicio de aplicación encargado de coordinar
/// los casos de uso del módulo de carrito de compras.
/// </summary>
/// <remarks>
/// Este contrato constituye la frontera pública del módulo de carrito dentro de
/// <c>Application</c>. Los comandos y consultas recibidos por sus métodos actúan
/// como modelos de entrada del caso de uso y no como una arquitectura pública alternativa.
/// </remarks>
public interface ICartApplicationService
{
    /// <summary>
    /// Crea un nuevo carrito para un cliente específico.
    /// </summary>
    Task<Result<CartDto>> CreateCartAsync(
        CreateCartCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Agrega un producto a un carrito existente.
    /// </summary>
    Task<Result<CartDto>> AddProductToCartAsync(
        AddProductToCartCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza la cantidad de un ítem existente dentro del carrito.
    /// </summary>
    Task<Result<CartDto>> UpdateCartItemQuantityAsync(
        UpdateCartItemQuantityCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Remueve un producto del carrito.
    /// </summary>
    Task<Result<CartDto>> RemoveProductFromCartAsync(
        RemoveProductFromCartCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Vacía completamente un carrito existente.
    /// </summary>
    Task<Result<CartDto>> ClearCartAsync(
        ClearCartCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el carrito asociado a un cliente.
    /// </summary>
    Task<Result<CartDto>> GetCartByCustomerIdAsync(
        GetCartByCustomerIdQuery query,
        CancellationToken cancellationToken = default);
}
