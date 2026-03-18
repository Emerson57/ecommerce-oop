namespace PlataformaECommerce.Application.Features.Orders.DTOs;

/// <summary>
/// Representa la solicitud de cancelación de un pedido dentro de la capa de aplicación.
/// </summary>
/// <remarks>
/// Este DTO se utiliza para transportar la información necesaria para ejecutar
/// el caso de uso de cancelación de un pedido, desacoplando la entrada externa
/// respecto del agregado de dominio <c>Pedido</c>.
///
/// Su propósito es servir como contrato de entrada para:
/// - endpoints HTTP,
/// - handlers de comandos,
/// - servicios de aplicación,
/// - procesos administrativos,
/// - módulos de atención al cliente,
/// - integraciones externas.
///
/// La estructura contiene únicamente datos de transporte y no debe incluir
/// lógica de negocio ni validaciones complejas. Dichas validaciones deben
/// resolverse en la capa Application mediante validadores especializados
/// y, posteriormente, reforzarse en el dominio.
/// </remarks>
public sealed class CancelOrderRequestDto
{
    #region Identificación del pedido

    /// <summary>
    /// Identificador único del pedido que se desea cancelar.
    /// </summary>
    public Guid OrderId { get; init; }

    #endregion

    #region Información de la cancelación

    /// <summary>
    /// Motivo funcional, operativo o comercial por el cual se solicita
    /// la cancelación del pedido.
    /// </summary>
    /// <remarks>
    /// Este campo permite conservar trazabilidad del contexto de negocio
    /// que originó la cancelación y puede utilizarse posteriormente para:
    /// - auditoría,
    /// - atención al cliente,
    /// - analítica operativa,
    /// - clasificación de causas,
    /// - y seguimiento interno.
    /// </remarks>
    public string Reason { get; init; } = string.Empty;

    #endregion

    #region Contexto y trazabilidad

    /// <summary>
    /// Identificador del usuario que solicita la cancelación, cuando esté disponible.
    /// </summary>
    /// <remarks>
    /// Este valor puede utilizarse para trazabilidad, auditoría
    /// o control de seguridad cuando la capa superior desee enviarlo explícitamente.
    /// </remarks>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>
    /// Dirección IP desde la cual se originó la solicitud, cuando esté disponible.
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// Canal de origen de la solicitud de cancelación, cuando la capa superior desee informarlo.
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
    /// Referencia externa opcional asociada a la solicitud de cancelación.
    /// </summary>
    /// <remarks>
    /// Puede representar un identificador de correlación, ticket,
    /// caso de soporte, incidente o cualquier referencia funcional
    /// útil para observabilidad y seguimiento transversal.
    /// </remarks>
    public string? ExternalReference { get; init; }

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida de la solicitud de cancelación del pedido.
    /// </summary>
    /// <returns>Cadena representativa de la solicitud.</returns>
    public override string ToString()
    {
        return $"CancelOrderRequestDto | OrderId: {OrderId} | Reason: {Reason} | RequestedByUserId: {RequestedByUserId} | Source: {Source}";
    }

    #endregion
}