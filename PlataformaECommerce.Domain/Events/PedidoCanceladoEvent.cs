using PlataformaECommerce.Domain.Entities.Orders;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Domain.Events;

/// <summary>
/// Representa el evento de dominio que indica que un pedido fue cancelado.
/// </summary>
/// <remarks>
/// Este evento expresa que el ciclo de vida del pedido fue interrumpido
/// mediante una cancelación válida desde la perspectiva del dominio.
/// 
/// Puede utilizarse para desencadenar procesos posteriores como:
/// - reversión o compensación de inventario,
/// - actualización de métricas,
/// - notificación al cliente,
/// - auditoría del motivo de cancelación,
/// - integración con módulos financieros o logísticos.
/// </remarks>
public sealed class PedidoCanceladoEvent : DomainEvent
{
    /// <summary>
    /// Inicializa una nueva instancia del evento <see cref="PedidoCanceladoEvent"/>.
    /// </summary>
    /// <param name="pedido">Pedido que originó el evento.</param>
    public PedidoCanceladoEvent(Pedido pedido)
    {
        ArgumentNullException.ThrowIfNull(pedido);

        PedidoId = pedido.Id;
        ClienteId = pedido.ClienteId;
        Estado = pedido.Estado;
        Total = pedido.Total;
        FechaCancelacionUtc = pedido.FechaCancelacionUtc;
        MotivoCancelacion = pedido.ObservacionCancelacion;
    }

    /// <summary>
    /// Identificador del pedido cancelado.
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
    /// Total monetario del pedido cancelado.
    /// </summary>
    public Money Total { get; }

    /// <summary>
    /// Fecha y hora UTC en que el pedido fue cancelado.
    /// </summary>
    public DateTime? FechaCancelacionUtc { get; }

    /// <summary>
    /// Motivo funcional registrado para la cancelación.
    /// </summary>
    public string? MotivoCancelacion { get; }

    /// <summary>
    /// Devuelve una representación resumida del evento.
    /// </summary>
    /// <returns>Cadena representativa del evento de cancelación del pedido.</returns>
    public override string ToString()
    {
        return $"{base.ToString()} | PedidoId: {PedidoId} | ClienteId: {ClienteId} | Estado: {Estado} | Motivo: {MotivoCancelacion}";
    }
}