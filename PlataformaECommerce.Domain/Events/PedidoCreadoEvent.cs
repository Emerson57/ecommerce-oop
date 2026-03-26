using PlataformaECommerce.Domain.Entities.Orders;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.Exceptions;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Domain.Events;

/// <summary>
/// Representa el evento de dominio que indica que un pedido fue creado.
/// </summary>
/// <remarks>
/// Este evento debe generarse cuando un pedido queda formalmente construido
/// dentro del dominio, ya sea a partir de un carrito de compras o mediante
/// un flujo controlado de creación de pedido.
/// 
/// Su propósito es permitir que otras partes del sistema reaccionen a este hecho
/// sin acoplarse directamente a la entidad <see cref="Pedido"/>, por ejemplo:
/// - auditoría,
/// - envío de notificaciones,
/// - generación de tareas operativas,
/// - registro en historial,
/// - integraciones posteriores.
/// </remarks>
public sealed class PedidoCreadoEvent : DomainEvent
{
    /// <summary>
    /// Inicializa una nueva instancia del evento <see cref="PedidoCreadoEvent"/>.
    /// </summary>
    /// <param name="pedido">Pedido que originó el evento.</param>
    public PedidoCreadoEvent(Pedido pedido)
    {
        ArgumentNullException.ThrowIfNull(pedido);

        PedidoId = pedido.Id;
        ClienteId = pedido.ClienteId;
        Estado = pedido.Estado;
        CantidadDetalles = pedido.CantidadDetalles;
        CantidadTotalUnidades = pedido.CantidadTotalUnidades;
        Total = pedido.Total;
        FechaCreacionPedidoUtc = pedido.FechaCreacionUtc;
    }

    /// <summary>
    /// Identificador del pedido creado.
    /// </summary>
    public Guid PedidoId { get; }

    /// <summary>
    /// Identificador del cliente propietario del pedido.
    /// </summary>
    public Guid ClienteId { get; }

    /// <summary>
    /// Estado inicial del pedido al momento del evento.
    /// </summary>
    public EstadoPedido Estado { get; }

    /// <summary>
    /// Cantidad total de líneas registradas en el pedido.
    /// </summary>
    public int CantidadDetalles { get; }

    /// <summary>
    /// Cantidad total de unidades compradas en el pedido.
    /// </summary>
    public int CantidadTotalUnidades { get; }

    /// <summary>
    /// Total monetario del pedido al momento de su creación.
    /// </summary>
    public Money Total { get; }

    /// <summary>
    /// Fecha y hora UTC de creación del pedido.
    /// </summary>
    public DateTime FechaCreacionPedidoUtc { get; }

    /// <summary>
    /// Devuelve una representación resumida del evento.
    /// </summary>
    /// <returns>Cadena representativa del evento de creación de pedido.</returns>
    public override string ToString()
    {
        return $"{base.ToString()} | PedidoId: {PedidoId} | ClienteId: {ClienteId} | Estado: {Estado} | Total: {Total}";
    }
}