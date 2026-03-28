using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Application.Features.Orders.DTOs;

/// <summary>
/// Representa el objeto de transferencia de datos detallado de un pedido
/// dentro de la capa de aplicación.
/// </summary>
/// <remarks>
/// Este DTO se utiliza para transportar la información detallada de un pedido
/// desde la capa Application hacia capas superiores como:
/// - Web API,
/// - paneles administrativos,
/// - módulos de atención al cliente,
/// - procesos de seguimiento,
/// - integraciones externas,
/// - y consultas internas.
///
/// Su propósito es desacoplar la representación expuesta del pedido
/// respecto de la entidad de dominio <c>Pedido</c>, evitando exponer
/// directamente detalles internos del modelo.
///
/// A diferencia de <see cref="OrderDto"/>, esta clase está orientada
/// a escenarios donde se requiere una vista más completa del pedido,
/// incluyendo trazabilidad temporal ampliada, detalle de líneas
/// y datos de cancelación.
/// </remarks>
public sealed class OrderDetailDto
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
    /// Fecha y hora UTC en que el pago del pedido fue registrado como exitoso.
    /// </summary>
    public DateTime? PaidAtUtc { get; init; }

    /// <summary>
    /// Fecha y hora UTC en que el pedido fue enviado o despachado.
    /// </summary>
    public DateTime? ShippedAtUtc { get; init; }

    /// <summary>
    /// Fecha y hora UTC en que el pedido fue entregado o completado.
    /// </summary>
    public DateTime? DeliveredAtUtc { get; init; }

    /// <summary>
    /// Fecha y hora UTC en que el pedido fue cancelado.
    /// </summary>
    public DateTime? CancelledAtUtc { get; init; }

    #endregion

    #region Información de cancelación

    /// <summary>
    /// Motivo u observación registrada para la cancelación del pedido.
    /// </summary>
    public string? CancellationReason { get; init; }

    /// <summary>
    /// Calle o línea principal de la dirección de envío registrada para el pedido.
    /// </summary>
    public string? ShippingStreet { get; init; }

    /// <summary>
    /// Ciudad de la dirección de envío registrada para el pedido.
    /// </summary>
    public string? ShippingCity { get; init; }

    /// <summary>
    /// Departamento, provincia o estado de la dirección de envío registrada para el pedido.
    /// </summary>
    public string? ShippingDepartment { get; init; }

    /// <summary>
    /// País de la dirección de envío registrada para el pedido.
    /// </summary>
    public string? ShippingCountry { get; init; }

    /// <summary>
    /// Código postal de la dirección de envío registrada para el pedido.
    /// </summary>
    public string? ShippingPostalCode { get; init; }

    #endregion

    #region Metadatos adicionales

    /// <summary>
    /// Indica si el pedido contiene al menos una línea de producto físico.
    /// </summary>
    public bool ContainsPhysicalProducts { get; init; }

    /// <summary>
    /// Indica si el pedido contiene al menos una línea de producto digital.
    /// </summary>
    public bool ContainsDigitalProducts { get; init; }

    #endregion

    #region Propiedades calculadas

    /// <summary>
    /// Indica si el pedido contiene al menos una línea registrada.
    /// </summary>
    public bool HasItems => ItemsCount > 0;

    /// <summary>
    /// Indica si el pedido se encuentra pagado.
    /// </summary>
    public bool IsPaid => PaidAtUtc.HasValue || Status is EstadoPedido.Pagado or EstadoPedido.EnProceso or EstadoPedido.Enviado or EstadoPedido.Entregado;

    /// <summary>
    /// Indica si el pedido se encuentra enviado.
    /// </summary>
    public bool IsShipped => ShippedAtUtc.HasValue || Status is EstadoPedido.Enviado or EstadoPedido.Entregado;

    /// <summary>
    /// Indica si el pedido fue entregado satisfactoriamente.
    /// </summary>
    public bool IsDelivered => Status == EstadoPedido.Entregado;

    /// <summary>
    /// Indica si el pedido fue cancelado.
    /// </summary>
    public bool IsCancelled => Status == EstadoPedido.Cancelado;

    /// <summary>
    /// Indica si el pedido se encuentra en un estado final.
    /// </summary>
    public bool IsFinalized => Status is EstadoPedido.Entregado or EstadoPedido.Cancelado;

    /// <summary>
    /// Indica si el pedido contiene una dirección de envío completa.
    /// </summary>
    public bool HasShippingAddress =>
        !string.IsNullOrWhiteSpace(ShippingStreet) &&
        !string.IsNullOrWhiteSpace(ShippingCity) &&
        !string.IsNullOrWhiteSpace(ShippingDepartment) &&
        !string.IsNullOrWhiteSpace(ShippingCountry) &&
        !string.IsNullOrWhiteSpace(ShippingPostalCode);

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del DTO detallado del pedido.
    /// </summary>
    /// <returns>Cadena representativa del pedido.</returns>
    public override string ToString()
    {
        return $"OrderDetailDto | Id: {Id} | CustomerId: {CustomerId} | Status: {Status} | ItemsCount: {ItemsCount} | TotalUnits: {TotalUnits} | TotalAmount: {Currency} {TotalAmount:N2} | IsFinalized: {IsFinalized}";
    }

    #endregion
}