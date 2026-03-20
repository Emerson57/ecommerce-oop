namespace PlataformaECommerce.Infrastructure.Persistence.Entities;

/// <summary>
/// Representa la proyección persistente de un detalle de pedido dentro de la infraestructura.
/// </summary>
/// <remarks>
/// Esta entidad preserva la instantánea comercial del producto adquirido al momento
/// de la compra, permitiendo reconstruir de forma consistente el historial de venta
/// sin depender de cambios posteriores en el catálogo.
/// </remarks>
public sealed class OrderItemEntity
{
    /// <summary>
    /// Obtiene o establece el identificador único del detalle del pedido.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Obtiene o establece el identificador del pedido propietario del detalle.
    /// </summary>
    public Guid PedidoId { get; set; }

    /// <summary>
    /// Obtiene o establece el identificador del producto asociado a la línea del pedido.
    /// </summary>
    public Guid ProductoId { get; set; }

    /// <summary>
    /// Obtiene o establece el nombre comercial del producto capturado en la compra.
    /// </summary>
    public string NombreProducto { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el SKU del producto capturado en la compra.
    /// </summary>
    public string SkuProducto { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el tipo lógico del producto capturado en la compra.
    /// </summary>
    public string TipoProducto { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la URL de la imagen principal del producto capturado en la compra.
    /// </summary>
    public string? ImagenPrincipalUrl { get; set; }

    /// <summary>
    /// Obtiene o establece el precio unitario aplicado a la línea del pedido.
    /// </summary>
    public decimal PrecioUnitario { get; set; }

    /// <summary>
    /// Obtiene o establece la moneda del precio unitario aplicado a la línea del pedido.
    /// </summary>
    public string Moneda { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la cantidad adquirida del producto.
    /// </summary>
    public int Cantidad { get; set; }

    /// <summary>
    /// Obtiene o establece la fecha de creación del detalle del pedido en UTC.
    /// </summary>
    public DateTime FechaCreacionUtc { get; set; }

    /// <summary>
    /// Obtiene o establece la navegación hacia el pedido propietario del detalle.
    /// </summary>
    public OrderEntity Pedido { get; set; } = null!;
}
