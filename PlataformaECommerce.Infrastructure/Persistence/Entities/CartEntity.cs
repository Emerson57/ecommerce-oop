namespace PlataformaECommerce.Infrastructure.Persistence.Entities;

/// <summary>
/// Representa la raíz persistente del agregado de carrito dentro de la infraestructura.
/// </summary>
/// <remarks>
/// Esta entidad conserva el estado operativo del carrito y la relación con sus líneas
/// persistentes, permitiendo reconstruir de forma consistente el agregado
/// <c>CarritoCompra</c> desde la base de datos transaccional.
/// </remarks>
public sealed class CartEntity
{
    /// <summary>
    /// Obtiene o establece el identificador único del carrito.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Obtiene o establece el identificador del cliente propietario del carrito.
    /// </summary>
    public Guid ClienteId { get; set; }

    /// <summary>
    /// Obtiene o establece un valor que indica si el carrito se encuentra activo.
    /// </summary>
    public bool Activo { get; set; }

    /// <summary>
    /// Obtiene o establece la fecha de creación del carrito en UTC.
    /// </summary>
    public DateTime FechaCreacionUtc { get; set; }

    /// <summary>
    /// Obtiene o establece la fecha de última actualización relevante del carrito en UTC.
    /// </summary>
    public DateTime? FechaActualizacionUtc { get; set; }

    /// <summary>
    /// Obtiene o establece la colección persistente de ítems asociados al carrito.
    /// </summary>
    public ICollection<CartItemEntity> Items { get; set; } = new List<CartItemEntity>();
}
