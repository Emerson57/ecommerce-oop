using PlataformaECommerce.Application.Abstractions;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Orders.DTOs;

namespace PlataformaECommerce.Application.Features.Orders.Commands;

/// <summary>
/// Representa el comando de aplicación para despachar o enviar un pedido
/// dentro del sistema.
/// </summary>
/// <remarks>
/// Este comando modela una intención explícita de escritura sobre el sistema,
/// correspondiente al caso de uso de registro del envío o despacho de un pedido.
///
/// Su propósito es encapsular la información operativa y logística necesaria
/// para soportar la transición del pedido hacia un estado de salida,
/// despacho o entrega en tránsito.
///
/// La validación estructural de los datos de envío debe resolverse en Application,
/// mientras que la validación final de la transición de estado permitida
/// debe ser reforzada en el dominio.
///
/// El resultado esperado de la operación es un <see cref="Result{TValue}"/>
/// que contiene un <see cref="OrderDetailDto"/> cuando la ejecución es exitosa.
/// </remarks>
public sealed class ShipOrderCommand : ICommand<Result<OrderDetailDto>>
{
    #region Identificación

    /// <summary>
    /// Identificador único del pedido que será enviado o despachado.
    /// </summary>
    public Guid OrderId { get; init; }

    #endregion

    #region Información logística

    /// <summary>
    /// Nombre del transportador, operador logístico o proveedor de envío.
    /// </summary>
    public string? CarrierName { get; init; }

    /// <summary>
    /// Número de guía, tracking o referencia logística del envío.
    /// </summary>
    public string? TrackingNumber { get; init; }

    /// <summary>
    /// URL opcional para seguimiento del envío.
    /// </summary>
    public string? TrackingUrl { get; init; }

    /// <summary>
    /// Fecha y hora UTC en la que el envío fue reportado o despachado.
    /// </summary>
    public DateTime? ShippedAtUtc { get; init; }

    /// <summary>
    /// Observación funcional o comentario asociado al despacho del pedido.
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Referencia externa opcional asociada al despacho.
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
    /// Devuelve una representación resumida del comando de envío.
    /// </summary>
    /// <returns>Cadena representativa del comando.</returns>
    public override string ToString()
    {
        return $"ShipOrderCommand | OrderId: {OrderId} | CarrierName: {CarrierName} | TrackingNumber: {TrackingNumber} | RequestedByUserId: {RequestedByUserId}";
    }

    #endregion
}