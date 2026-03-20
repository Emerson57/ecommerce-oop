using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Orders.DTOs;

namespace PlataformaECommerce.Application.Features.Orders.Commands;

/// <summary>
/// Representa el comando de aplicación para confirmar un pedido existente
/// dentro del flujo comercial del sistema.
/// </summary>
/// <remarks>
/// Este comando modela una intención explícita de escritura sobre el sistema,
/// correspondiente al caso de uso de confirmación formal de un pedido.
///
/// Su propósito es desacoplar la acción de confirmación respecto de otros
/// cambios del ciclo de vida del pedido, permitiendo tratar esta operación
/// como una transición funcional específica y claramente expresada dentro
/// de la capa Application.
///
/// La validación de estructura, consistencia y permisos debe resolverse en
/// validadores y servicios de aplicación, mientras que la validación final
/// del estado permitido debe reforzarse en el dominio.
///
/// El resultado esperado de la operación es un <see cref="Result{TValue}"/>
/// que contiene un <see cref="OrderDetailDto"/> cuando la ejecución es exitosa.
/// </remarks>
public sealed class ConfirmOrderCommand
{
    #region Identificación

    /// <summary>
    /// Identificador único del pedido que será confirmado.
    /// </summary>
    public Guid OrderId { get; init; }

    #endregion

    #region Información funcional de la operación

    /// <summary>
    /// Observación funcional o comentario asociado a la confirmación del pedido.
    /// </summary>
    /// <remarks>
    /// Este campo resulta útil para auditoría, soporte, trazabilidad operativa
    /// o correlación con procesos administrativos internos.
    /// </remarks>
    public string? Notes { get; init; }

    /// <summary>
    /// Referencia externa opcional asociada a la operación de confirmación.
    /// </summary>
    /// <remarks>
    /// Puede representar un identificador de correlación, ticket,
    /// caso de soporte, sesión o referencia de integración.
    /// </remarks>
    public string? ExternalReference { get; init; }

    #endregion

    #region Contexto y trazabilidad

    /// <summary>
    /// Identificador del usuario que solicita o ejecuta la confirmación del pedido.
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
    /// - ApiClient
    /// - CustomerService
    /// </remarks>
    public string? Source { get; init; }

    /// <summary>
    /// Fecha y hora UTC en la que la capa superior registró la solicitud.
    /// </summary>
    public DateTime? RequestedAtUtc { get; init; }

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del comando de confirmación.
    /// </summary>
    /// <returns>Cadena representativa del comando.</returns>
    public override string ToString()
    {
        return $"ConfirmOrderCommand | OrderId: {OrderId} | RequestedByUserId: {RequestedByUserId} | Source: {Source}";
    }

    #endregion
}