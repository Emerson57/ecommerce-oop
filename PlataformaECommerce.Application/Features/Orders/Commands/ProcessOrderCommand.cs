using PlataformaECommerce.Application.Abstractions;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Orders.DTOs;

namespace PlataformaECommerce.Application.Features.Orders.Commands;

/// <summary>
/// Representa el comando de aplicación para pasar un pedido
/// al estado de procesamiento operativo dentro del sistema.
/// </summary>
/// <remarks>
/// Este comando modela una intención explícita de escritura sobre el sistema,
/// correspondiente al caso de uso de inicio o registro de alistamiento,
/// preparación o ejecución operativa del pedido.
///
/// Su propósito es desacoplar esta transición respecto de otras acciones
/// del ciclo de vida del pedido, permitiendo una trazabilidad clara
/// del momento en que la orden entra a la fase operativa.
///
/// La validación de permisos, consistencia de entrada y contexto debe resolverse
/// en Application, mientras que la validación final de estados permitidos
/// debe ser reforzada por el dominio.
///
/// El resultado esperado de la operación es un <see cref="Result{TValue}"/>
/// que contiene un <see cref="OrderDetailDto"/> cuando la ejecución es exitosa.
/// </remarks>
public sealed class ProcessOrderCommand : ICommand<Result<OrderDetailDto>>
{
    #region Identificación

    /// <summary>
    /// Identificador único del pedido que será pasado a procesamiento.
    /// </summary>
    public Guid OrderId { get; init; }

    #endregion

    #region Información funcional de la operación

    /// <summary>
    /// Motivo funcional o comentario asociado al inicio del procesamiento del pedido.
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// Observación operativa o comentario adicional asociado a la transición.
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Referencia externa opcional asociada al procesamiento del pedido.
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
    /// Devuelve una representación resumida del comando de procesamiento.
    /// </summary>
    /// <returns>Cadena representativa del comando.</returns>
    public override string ToString()
    {
        return $"ProcessOrderCommand | OrderId: {OrderId} | RequestedByUserId: {RequestedByUserId} | Source: {Source}";
    }

    #endregion
}