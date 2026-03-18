using PlataformaECommerce.Domain.Common;
using PlataformaECommerce.Domain.Entities.Cart;
using PlataformaECommerce.Domain.Entities.Products;

namespace PlataformaECommerce.Domain.Rules;

/// <summary>
/// Representa la regla de negocio que determina si un carrito
/// puede recibir un producto y una cantidad específicos.
/// </summary>
/// <remarks>
/// Esta regla centraliza la validación previa al agregado de productos al carrito,
/// consolidando condiciones relacionadas con:
/// - existencia del carrito,
/// - estado activo del carrito,
/// - existencia del producto,
/// - disponibilidad comercial del producto,
/// - cantidad válida,
/// - capacidad máxima del carrito,
/// - y stock suficiente.
///
/// Su objetivo es mantener una decisión de negocio coherente y reutilizable
/// desde el dominio en los procesos de compra.
/// </remarks>
public static class CarritoPuedeAgregarProductoRule
{
    /// <summary>
    /// Evalúa si el carrito puede recibir un producto con una cantidad determinada.
    /// </summary>
    /// <param name="carrito">Carrito a evaluar.</param>
    /// <param name="producto">Producto que se desea agregar.</param>
    /// <param name="cantidad">Cantidad solicitada.</param>
    /// <returns>
    /// <see langword="true"/> si la operación puede realizarse;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    public static bool IsSatisfiedBy(CarritoCompra? carrito, Producto? producto, int cantidad)
    {
        if (carrito is null || producto is null)
        {
            return false;
        }

        if (!carrito.Activo)
        {
            return false;
        }

        if (cantidad <= 0)
        {
            return false;
        }

        if (!producto.EstaDisponible())
        {
            return false;
        }

        if (!producto.TieneStockDisponible(cantidad))
        {
            return false;
        }

        if (carrito.TieneItems() && !MonedaConsistenteRule.IsSatisfiedBy(carrito.Total.Currency, producto.Precio))
        {
            return false;
        }

        bool yaExiste = carrito.ContieneProducto(producto.Id);

        if (!yaExiste && carrito.CantidadItems >= DomainLimits.MaximoItemsPorCarrito)
        {
            return false;
        }

        if (yaExiste)
        {
            int cantidadActual = carrito.ObtenerCantidadDeProducto(producto.Id);
            int cantidadResultante = cantidadActual + cantidad;

            if (!producto.TieneStockDisponible(cantidadResultante))
            {
                return false;
            }
        }

        return true;
    }
}