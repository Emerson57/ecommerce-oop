using PlataformaECommerce.Domain.Entities.Products;

namespace PlataformaECommerce.Domain.Rules;

/// <summary>
/// Representa la regla de negocio que determina si un producto
/// se encuentra disponible para operación comercial.
/// </summary>
/// <remarks>
/// Esta regla centraliza la validación de disponibilidad de un producto
/// en escenarios como:
/// - agregado al carrito,
/// - compra directa,
/// - activación comercial,
/// - promociones,
/// - verificación previa al checkout.
///
/// Un producto se considera disponible cuando:
/// - existe,
/// - está activo,
/// - y tiene al menos una unidad en inventario.
/// </remarks>
public static class ProductoDisponibleRule
{
    /// <summary>
    /// Evalúa si un producto cumple las condiciones mínimas para considerarse disponible.
    /// </summary>
    /// <param name="producto">Producto a evaluar.</param>
    /// <returns>
    /// <see langword="true"/> si el producto está disponible;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    public static bool IsSatisfiedBy(Producto? producto)
    {
        if (producto is null)
        {
            return false;
        }

        return producto.Activo && producto.Stock > 0;
    }
}