using PlataformaECommerce.Domain.Entities.Products;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Domain.Events;

/// <summary>
/// Representa el evento de dominio que indica que un producto quedó sin stock.
/// </summary>
/// <remarks>
/// Este evento expresa que el inventario disponible del producto llegó a cero,
/// lo cual constituye un hecho relevante del negocio.
/// 
/// Puede utilizarse para desencadenar procesos posteriores como:
/// - notificación al equipo de operaciones,
/// - reposición automática,
/// - despublicación temporal del catálogo,
/// - alertas de inventario,
/// - actualización de proyecciones o dashboards.
/// </remarks>
public sealed class ProductoSinStockEvent : DomainEvent
{
    /// <summary>
    /// Inicializa una nueva instancia del evento <see cref="ProductoSinStockEvent"/>.
    /// </summary>
    /// <param name="producto">Producto que originó el evento.</param>
    public ProductoSinStockEvent(Producto producto)
    {
        ArgumentNullException.ThrowIfNull(producto);

        ProductoId = producto.Id;
        NombreProducto = producto.Nombre;
        SkuProducto = producto.Sku;
        TipoProducto = producto.TipoProducto;
        Precio = producto.Precio;
        Activo = producto.Activo;
        StockActual = producto.Stock;
    }

    /// <summary>
    /// Identificador del producto afectado.
    /// </summary>
    public Guid ProductoId { get; }

    /// <summary>
    /// Nombre comercial del producto.
    /// </summary>
    public string NombreProducto { get; }

    /// <summary>
    /// SKU del producto.
    /// </summary>
    public Sku SkuProducto { get; }

    /// <summary>
    /// Tipo de producto afectado.
    /// </summary>
    public TipoProducto TipoProducto { get; }

    /// <summary>
    /// Precio actual del producto.
    /// </summary>
    public Money Precio { get; }

    /// <summary>
    /// Indica si el producto permanece activo al momento del evento.
    /// </summary>
    public bool Activo { get; }

    /// <summary>
    /// Stock actual del producto al momento del evento.
    /// </summary>
    public int StockActual { get; }

    /// <summary>
    /// Devuelve una representación resumida del evento.
    /// </summary>
    /// <returns>Cadena representativa del evento de producto sin stock.</returns>
    public override string ToString()
    {
        return $"{base.ToString()} | ProductoId: {ProductoId} | Nombre: {NombreProducto} | SKU: {SkuProducto} | StockActual: {StockActual}";
    }
}