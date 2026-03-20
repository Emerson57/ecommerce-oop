using PlataformaECommerce.Application.Features.Cart.DTOs;
using PlataformaECommerce.Domain.Entities.Cart;

namespace PlataformaECommerce.Application.Features.Cart.Mappings;

/// <summary>
/// Proporciona métodos de extensión para mapear entidades del dominio del carrito
/// hacia objetos de transferencia de datos de la capa Application.
/// </summary>
/// <remarks>
/// Esta clase centraliza la lógica de proyección de las entidades
/// <see cref="CarritoCompra"/> e <see cref="ItemCarrito"/> hacia DTOs
/// de lectura y respuesta, evitando duplicación de código en:
/// 
/// - servicios de aplicación,
/// - páginas y controladores consumidores,
/// - y otros componentes de orquestación.
///
/// Su propósito es:
/// - mantener consistencia en las proyecciones,
/// - desacoplar la capa Application del detalle de serialización,
/// - facilitar mantenimiento,
/// - y mejorar la legibilidad de los casos de uso.
///
/// La clase asume que el dominio ya integra value objects como:
/// - <c>Money</c> para totales y precios,
/// - <c>Sku</c> para referencias comerciales de producto.
/// </remarks>
public static class CartMappings
{
    #region Mapeos individuales

    /// <summary>
    /// Proyecta una entidad <see cref="CarritoCompra"/> hacia un <see cref="CartDto"/>.
    /// </summary>
    /// <param name="cart">Carrito a proyectar.</param>
    /// <returns>DTO del carrito.</returns>
    /// <exception cref="ArgumentNullException">
    /// Se produce cuando el carrito es nulo.
    /// </exception>
    public static CartDto ToCartDto(this CarritoCompra cart)
    {
        ArgumentNullException.ThrowIfNull(cart);

        return new CartDto
        {
            Id = cart.Id,
            CustomerId = cart.ClienteId,
            IsActive = cart.Activo,
            Items = cart.Items.ToCartItemDtos(),
            ItemsCount = cart.CantidadItems,
            TotalUnits = cart.CantidadTotalUnidades,
            TotalAmount = cart.Total.Amount,
            Currency = cart.Total.Currency,
            CreatedAtUtc = cart.FechaCreacionUtc,
            UpdatedAtUtc = cart.FechaActualizacionUtc
        };
    }

    /// <summary>
    /// Proyecta una entidad <see cref="ItemCarrito"/> hacia un <see cref="CartItemDto"/>.
    /// </summary>
    /// <param name="item">Ítem del carrito a proyectar.</param>
    /// <returns>DTO del ítem del carrito.</returns>
    /// <exception cref="ArgumentNullException">
    /// Se produce cuando el ítem es nulo.
    /// </exception>
    public static CartItemDto ToCartItemDto(this ItemCarrito item)
    {
        ArgumentNullException.ThrowIfNull(item);

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

    #region Mapeos de colecciones

    /// <summary>
    /// Proyecta una colección de entidades <see cref="CarritoCompra"/>
    /// hacia una colección de <see cref="CartDto"/>.
    /// </summary>
    /// <param name="carts">Colección de carritos a proyectar.</param>
    /// <returns>Colección de DTOs de carrito.</returns>
    /// <exception cref="ArgumentNullException">
    /// Se produce cuando la colección es nula.
    /// </exception>
    public static IReadOnlyCollection<CartDto> ToCartDtos(this IEnumerable<CarritoCompra> carts)
    {
        ArgumentNullException.ThrowIfNull(carts);

        return carts
            .Select(cart => cart.ToCartDto())
            .ToArray();
    }

    /// <summary>
    /// Proyecta una colección de entidades <see cref="ItemCarrito"/>
    /// hacia una colección de <see cref="CartItemDto"/>.
    /// </summary>
    /// <param name="items">Colección de ítems a proyectar.</param>
    /// <returns>Colección de DTOs de ítems del carrito.</returns>
    /// <exception cref="ArgumentNullException">
    /// Se produce cuando la colección es nula.
    /// </exception>
    public static IReadOnlyCollection<CartItemDto> ToCartItemDtos(this IEnumerable<ItemCarrito> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        return items
            .Select(item => item.ToCartItemDto())
            .ToArray();
    }

    #endregion
}
