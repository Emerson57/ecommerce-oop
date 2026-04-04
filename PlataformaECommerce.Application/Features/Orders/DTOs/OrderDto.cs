using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Application.Features.Orders.DTOs;

/// <summary>
/// Representa el objeto de transferencia de datos de un pedido
/// dentro de la capa de aplicación.
/// </summary>
/// <remarks>
/// Este DTO se utiliza para transportar la información consolidada de un pedido
/// desde la capa Application hacia capas superiores como:
/// - Web API,
/// - paneles administrativos,
/// - procesos de seguimiento,
/// - módulos de atención al cliente,
/// - integraciones externas,
/// - y consultas internas.
///
/// Su propósito es desacoplar la representación expuesta del pedido
/// respecto de la entidad de dominio <c>Pedido</c>, evitando filtrar
/// directamente detalles internos del modelo.
///
/// Este DTO contiene:
/// - información básica del pedido,
/// - identificación del cliente,
/// - estado actual,
/// - resumen económico,
/// - detalle de líneas,
/// - trazabilidad temporal,
/// - y datos de cancelación cuando corresponda.
/// </remarks>
public sealed class OrderDto
{
    #region Identificación básica

    /// <summary>
    /// Identificador único del pedido.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Identificador único del cliente propietario del pedido.
    /// </summary>
    public Guid CustomerId { get; init; }

    #endregion

    #region Estado operativo

    /// <summary>
    /// Estado actual del ciclo de vida del pedido.
    /// </summary>
    public EstadoPedido Status { get; init; }

    #endregion

    #region Información de contenido

    /// <summary>
    /// Colección de líneas o ítems del pedido.
    /// </summary>
    public IReadOnlyCollection<OrderItemDto> Items { get; init; } = Array.Empty<OrderItemDto>();

    /// <summary>
    /// Cantidad total de líneas registradas en el pedido.
    /// </summary>
    public int ItemsCount { get; init; }

    /// <summary>
    /// Cantidad total de unidades acumuladas entre todas las líneas del pedido.
    /// </summary>
    public int TotalUnits { get; init; }

    /// <summary>
    /// Total monetario consolidado del pedido.
    /// </summary>
    public decimal TotalAmount { get; init; }

    /// <summary>
    /// Código de moneda asociado al total del pedido.
    /// </summary>
    public string Currency { get; init; } = string.Empty;

    #endregion

    #region Información temporal

    /// <summary>
    /// Fecha y hora UTC en que fue creado el pedido.
    /// </summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>
    /// Fecha y hora UTC de la última actualización relevante del pedido.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; init; }

    /// <summary>
    /// Fecha y hora UTC en que el pedido fue confirmado.
    /// </summary>
    public DateTime? ConfirmedAtUtc { get; init; }

    /// <summary>
    /// Fecha y hora UTC en que el pago del pedido fue registrado.
    /// </summary>
    public DateTime? PaidAtUtc { get; init; }

    /// <summary>
    /// Fecha y hora UTC en que el pedido fue enviado.
    /// </summary>
    public DateTime? ShippedAtUtc { get; init; }

    /// <summary>
    /// Fecha y hora UTC en que el pedido fue entregado.
    /// </summary>
    public DateTime? DeliveredAtUtc { get; init; }

    /// <summary>
    /// Fecha y hora UTC en que el pedido fue cancelado.
    /// </summary>
    public DateTime? CancelledAtUtc { get; init; }

    #endregion

    #region Información de cancelación

    /// <summary>
    /// Observación o motivo de cancelación del pedido.
    /// </summary>
    public string? CancellationReason { get; init; }

    /// <summary>
    /// Método de pago seleccionado durante el checkout.
    /// </summary>
    public MetodoPagoPedido? PaymentMethod { get; init; }

    #endregion

    #region Propiedades calculadas

    /// <summary>
    /// Indica si el pedido contiene al menos una línea.
    /// </summary>
    public bool HasItems => ItemsCount > 0;

    /// <summary>
    /// Indica si el pedido se encuentra en un estado final.
    /// </summary>
    public bool IsCompleted => Status == EstadoPedido.Entregado;

    /// <summary>
    /// Indica si el pedido fue cancelado.
    /// </summary>
    public bool IsCancelled => Status == EstadoPedido.Cancelado;

    /// <summary>
    /// Indica si el pedido se encuentra finalizado.
    /// </summary>
    public bool IsFinalized => Status is EstadoPedido.Entregado or EstadoPedido.Cancelado;

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del DTO del pedido.
    /// </summary>
    /// <returns>Cadena representativa del pedido.</returns>
    public override string ToString()
    {
        return $"OrderDto | Id: {Id} | CustomerId: {CustomerId} | Status: {Status} | ItemsCount: {ItemsCount} | TotalUnits: {TotalUnits} | TotalAmount: {Currency} {TotalAmount:N2}";
    }

    #endregion
}
