namespace PlataformaECommerce.Infrastructure.Persistence.Entities;

/// <summary>
/// Representa la proyección persistente de un ítem del carrito dentro de la infraestructura.
/// </summary>
/// <remarks>
/// Esta entidad almacena la instantánea comercial mínima del producto al momento en que
/// fue incorporado o sincronizado dentro del carrito, preservando consistencia histórica
/// para nombre, SKU, tipo, imagen, precio y cantidad.
/// </remarks>
public sealed class CartItemEntity
{
    /// <summary>
    /// Obtiene o establece el identificador único del ítem del carrito.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Obtiene o establece el identificador del carrito propietario del ítem.
    /// </summary>
    public Guid CartId { get; set; }

    /// <summary>
    /// Obtiene o establece el identificador del producto asociado al ítem.
    /// </summary>
    public Guid ProductoId { get; set; }

    /// <summary>
    /// Obtiene o establece el nombre comercial del producto capturado en el carrito.
    /// </summary>
    public string NombreProducto { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el SKU del producto capturado en el carrito.
    /// </summary>
    public string SkuProducto { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el tipo lógico del producto capturado en el carrito.
    /// </summary>
    public string TipoProducto { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la URL de la imagen principal del producto capturado en el carrito.
    /// </summary>
    public string? ImagenPrincipalUrl { get; set; }

    /// <summary>
    /// Obtiene o establece el precio unitario capturado para el ítem.
    /// </summary>
    public decimal PrecioUnitario { get; set; }

    /// <summary>
    /// Obtiene o establece la moneda del precio unitario capturado.
    /// </summary>
    public string Moneda { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la cantidad seleccionada del producto.
    /// </summary>
    public int Cantidad { get; set; }

    /// <summary>
    /// Obtiene o establece la fecha de creación del ítem en UTC.
    /// </summary>
    public DateTime FechaCreacionUtc { get; set; }

    /// <summary>
    /// Obtiene o establece la fecha de última actualización del ítem en UTC.
    /// </summary>
    public DateTime? FechaActualizacionUtc { get; set; }

    /// <summary>
    /// Obtiene o establece la navegación hacia el carrito propietario del ítem.
    /// </summary>
    public CartEntity Cart { get; set; } = null!;
}
