using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Orders.DTOs;

namespace PlataformaECommerce.Application.Features.Orders.Commands;

/// <summary>
/// Representa el comando de aplicación para marcar un pedido como entregado
/// o completado satisfactoriamente dentro del sistema.
/// </summary>
/// <remarks>
/// Este comando modela una intención explícita de escritura sobre el sistema,
/// correspondiente al caso de uso de cierre exitoso del ciclo operativo
/// de un pedido.
///
/// Su propósito es desacoplar la entrega respecto de otros cambios del ciclo
/// de vida de la orden, permitiendo registrar esta transición final con
/// trazabilidad operativa, administrativa y funcional.
///
/// La validación de estructura, permisos y contexto debe resolverse
/// en Application, mientras que la validación final del estado permitido
/// debe reforzarse en el dominio.
///
/// El resultado esperado de la operación es un <see cref="Result{TValue}"/>
/// que contiene un <see cref="OrderDetailDto"/> cuando la ejecución es exitosa.
/// </remarks>
public sealed class DeliverOrderCommand
{
    #region Identificación

    /// <summary>
    /// Identificador único del pedido que será marcado como entregado.
    /// </summary>
    public Guid OrderId { get; init; }

    #endregion

    #region Información funcional de la operación

    /// <summary>
    /// Nombre de la persona que recibió el pedido, cuando aplique.
    /// </summary>
    public string? ReceivedBy { get; init; }

    /// <summary>
    /// Evidencia funcional, referencia o comentario de entrega.
    /// </summary>
    /// <remarks>
    /// Puede representar un código de confirmación, observación del operador,
    /// soporte de entrega, acta, evidencia digital o nota administrativa.
    /// </remarks>
    public string? DeliveryEvidence { get; init; }

    /// <summary>
    /// Fecha y hora UTC en la que la entrega fue registrada o confirmada.
    /// </summary>
    public DateTime? DeliveredAtUtc { get; init; }

    /// <summary>
    /// Observación funcional o comentario adicional asociado a la entrega.
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Referencia externa opcional asociada a la operación de entrega.
    /// </summary>
    public string? ExternalReference { get; init; }

    #endregion

    #region Contexto y trazabilidad

    /// <summary>
    /// Identificador del usuario que solicita o ejecuta la operación.
    /// </summary>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>
    /// Dirección IP desde la cual se originó la solicitud, cuando esté disponible.
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// Canal de origen desde el cual se genera la solicitud.
    /// </summary>
    public string? Source { get; init; }

    /// <summary>
    /// Fecha y hora UTC en la que la capa superior registró la solicitud.
    /// </summary>
    public DateTime? RequestedAtUtc { get; init; }

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del comando de entrega.
    /// </summary>
    /// <returns>Cadena representativa del comando.</returns>
    public override string ToString()
    {
        return $"DeliverOrderCommand | OrderId: {OrderId} | ReceivedBy: {ReceivedBy} | RequestedByUserId: {RequestedByUserId} | Source: {Source}";
    }

    #endregion
}