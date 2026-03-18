using PlataformaECommerce.Application.Abstractions;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Orders.DTOs;

namespace PlataformaECommerce.Application.Features.Orders.Commands;

/// <summary>
/// Representa el comando de aplicación para cancelar un pedido existente
/// dentro del sistema.
/// </summary>
/// <remarks>
/// Este comando modela una intención explícita de escritura sobre el sistema,
/// correspondiente al caso de uso de cancelación de un pedido.
///
/// Su propósito es encapsular la información funcional, operativa y de trazabilidad
/// necesaria para ejecutar la cancelación de forma controlada, auditable y coherente
/// con las reglas del dominio.
///
/// La validación de obligatoriedad del motivo y la consistencia de entrada debe
/// resolverse en Application, mientras que la validación definitiva respecto del
/// estado actual del pedido debe reforzarse en el dominio.
///
/// El resultado esperado de la operación es un <see cref="Result{TValue}"/>
/// que contiene un <see cref="OrderDetailDto"/> cuando la ejecución es exitosa.
/// </remarks>
public sealed class CancelOrderCommand : ICommand<Result<OrderDetailDto>>
{
    #region Identificación

    /// <summary>
    /// Identificador único del pedido que se desea cancelar.
    /// </summary>
    public Guid OrderId { get; init; }

    #endregion

    #region Información de la cancelación

    /// <summary>
    /// Motivo funcional, operativo o comercial por el cual se solicita la cancelación.
    /// </summary>
    public string Reason { get; init; } = string.Empty;

    /// <summary>
    /// Observación adicional asociada a la cancelación del pedido.
    /// </summary>
    /// <remarks>
    /// Este campo puede complementar el motivo principal y aportar contexto útil
    /// para auditoría, atención al cliente o seguimiento interno.
    /// </remarks>
    public string? Notes { get; init; }

    /// <summary>
    /// Indica si la cancelación fue originada por el cliente.
    /// </summary>
    public bool RequestedByCustomer { get; init; }

    /// <summary>
    /// Referencia externa opcional asociada a la cancelación.
    /// </summary>
    /// <remarks>
    /// Puede representar un ticket, caso de soporte, número de incidente,
    /// identificador de integración o correlación con otros sistemas.
    /// </remarks>
    public string? ExternalReference { get; init; }

    #endregion

    #region Contexto y trazabilidad

    /// <summary>
    /// Identificador del usuario que solicita o ejecuta la cancelación, cuando esté disponible.
    /// </summary>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>
    /// Dirección IP desde la cual se originó la solicitud, cuando esté disponible.
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// Canal de origen desde el cual se genera la solicitud.
    /// </summary>
    /// <remarks>
    /// Ejemplos comunes:
    /// - Web
    /// - Mobile
    /// - AdminPortal
    /// - CustomerService
    /// - ApiClient
    /// </remarks>
    public string? Source { get; init; }

    /// <summary>
    /// Fecha y hora UTC en la que la capa superior registró la solicitud.
    /// </summary>
    public DateTime? RequestedAtUtc { get; init; }

    #endregion

    #region Propiedades calculadas

    /// <summary>
    /// Indica si el motivo de cancelación contiene un valor estructuralmente válido.
    /// </summary>
    public bool HasReason => !string.IsNullOrWhiteSpace(Reason);

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del comando de cancelación.
    /// </summary>
    /// <returns>Cadena representativa del comando.</returns>
    public override string ToString()
    {
        return $"CancelOrderCommand | OrderId: {OrderId} | RequestedByUserId: {RequestedByUserId} | RequestedByCustomer: {RequestedByCustomer} | Source: {Source}";
    }

    #endregion
}