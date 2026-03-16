using PlataformaECommerce.Domain.Entities.Orders;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Domain.Events;

/// <summary>
/// Representa el evento de dominio que indica que un pedido fue registrado como pagado.
/// </summary>
/// <remarks>
/// Este evento expresa que el flujo de pago del pedido fue completado con éxito
/// desde la perspectiva del dominio. Puede utilizarse para activar procesos posteriores como:
/// - alistamiento,
/// - facturación,
/// - preparación logística,
/// - notificación al cliente,
/// - sincronización con sistemas externos.
/// </remarks>
public sealed class PedidoPagadoEvent : DomainEvent
{
    /// <summary>
    /// Inicializa una nueva instancia del evento <see cref="PedidoPagadoEvent"/>.
    /// </summary>
    /// <param name="pedido">Pedido que originó el evento.</param>
    public PedidoPagadoEvent(Pedido pedido)
    {
        ArgumentNullException.ThrowIfNull(pedido);

        PedidoId = pedido.Id;
        ClienteId = pedido.ClienteId;
        Estado = pedido.Estado;
        Total = pedido.Total;
        FechaPagoUtc = pedido.FechaPagoUtc;
    }

    /// <summary>
    /// Identificador del pedido pagado.
    /// </summary>
    public Guid PedidoId { get; }

    /// <summary>
    /// Identificador del cliente propietario del pedido.
    /// </summary>
    public Guid ClienteId { get; }

    /// <summary>
    /// Estado del pedido al momento del evento.
    /// </summary>
    public EstadoPedido Estado { get; }

    /// <summary>
    /// Total monetario del pedido.
    /// </summary>
    public Money Total { get; }

    /// <summary>
    /// Fecha y hora UTC en que el pago fue registrado.
    /// </summary>
    public DateTime? FechaPagoUtc { get; }

    /// <summary>
    /// Devuelve una representación resumida del evento.
    /// </summary>
    /// <returns>Cadena representativa del evento de pago del pedido.</returns>
    public override string ToString()
    {
        return $"{base.ToString()} | PedidoId: {PedidoId} | ClienteId: {ClienteId} | Estado: {Estado} | Total: {Total}";
    }
}