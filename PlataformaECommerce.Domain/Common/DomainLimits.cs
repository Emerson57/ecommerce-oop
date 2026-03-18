namespace PlataformaECommerce.Domain.Common;

/// <summary>
/// Define límites operativos compartidos por los agregados del dominio.
/// </summary>
/// <remarks>
/// Estos valores representan restricciones funcionales estables del negocio y permiten
/// mantener coherencia entre reglas, entidades y validaciones.
/// </remarks>
internal static class DomainLimits
{
    /// <summary>
    /// Cantidad máxima de líneas distintas permitidas en un carrito de compra.
    /// </summary>
    internal const int MaximoItemsPorCarrito = 100;

    /// <summary>
    /// Cantidad máxima de líneas distintas permitidas en un pedido.
    /// </summary>
    internal const int MaximoDetallesPorPedido = 100;

    /// <summary>
    /// Cantidad máxima permitida por una línea comercial.
    /// </summary>
    internal const int MaximoCantidadPorLinea = 999;

    /// <summary>
    /// Cantidad máxima permitida de etiquetas asociadas a un producto.
    /// </summary>
    internal const int MaximoEtiquetasPorProducto = 20;
}