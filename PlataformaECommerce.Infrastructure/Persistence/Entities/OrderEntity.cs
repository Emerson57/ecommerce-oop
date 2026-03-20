namespace PlataformaECommerce.Infrastructure.Persistence.Entities;

/// <summary>
/// Representa la raíz persistente del agregado de pedidos dentro de la infraestructura.
/// </summary>
/// <remarks>
/// Esta entidad conserva el estado transaccional, temporal y logístico del pedido,
/// incluyendo su ciclo de vida, su dirección de envío desnormalizada y la colección
/// de detalles persistentes requerida para reconstruir el agregado <c>Pedido</c>.
/// </remarks>
public sealed class OrderEntity
{
    /// <summary>
    /// Obtiene o establece el identificador único del pedido.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Obtiene o establece el identificador del cliente propietario del pedido.
    /// </summary>
    public Guid ClienteId { get; set; }

    /// <summary>
    /// Obtiene o establece el estado funcional actual del pedido.
    /// </summary>
    public string Estado { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la fecha de creación del pedido en UTC.
    /// </summary>
    public DateTime FechaCreacionUtc { get; set; }

    /// <summary>
    /// Obtiene o establece la fecha de última actualización relevante del pedido en UTC.
    /// </summary>
    public DateTime? FechaActualizacionUtc { get; set; }

    /// <summary>
    /// Obtiene o establece la fecha de confirmación del pedido en UTC.
    /// </summary>
    public DateTime? FechaConfirmacionUtc { get; set; }

    /// <summary>
    /// Obtiene o establece la fecha de registro del pago del pedido en UTC.
    /// </summary>
    public DateTime? FechaPagoUtc { get; set; }

    /// <summary>
    /// Obtiene o establece la fecha de envío del pedido en UTC.
    /// </summary>
    public DateTime? FechaEnvioUtc { get; set; }

    /// <summary>
    /// Obtiene o establece la fecha de entrega del pedido en UTC.
    /// </summary>
    public DateTime? FechaEntregaUtc { get; set; }

    /// <summary>
    /// Obtiene o establece la fecha de cancelación del pedido en UTC.
    /// </summary>
    public DateTime? FechaCancelacionUtc { get; set; }

    /// <summary>
    /// Obtiene o establece la observación o motivo de cancelación del pedido.
    /// </summary>
    public string? ObservacionCancelacion { get; set; }

    /// <summary>
    /// Obtiene o establece la calle de la dirección de envío asociada al pedido.
    /// </summary>
    public string? DireccionCalle { get; set; }

    /// <summary>
    /// Obtiene o establece la ciudad de la dirección de envío asociada al pedido.
    /// </summary>
    public string? DireccionCiudad { get; set; }

    /// <summary>
    /// Obtiene o establece el departamento de la dirección de envío asociada al pedido.
    /// </summary>
    public string? DireccionDepartamento { get; set; }

    /// <summary>
    /// Obtiene o establece el país de la dirección de envío asociada al pedido.
    /// </summary>
    public string? DireccionPais { get; set; }

    /// <summary>
    /// Obtiene o establece el código postal de la dirección de envío asociada al pedido.
    /// </summary>
    public string? DireccionCodigoPostal { get; set; }

    /// <summary>
    /// Obtiene o establece la colección persistente de detalles asociados al pedido.
    /// </summary>
    public ICollection<OrderItemEntity> Detalles { get; set; } = new List<OrderItemEntity>();
}
