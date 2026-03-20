namespace PlataformaECommerce.Application.Features.Audit.Queries;

/// <summary>
/// Representa una consulta de aplicación para recuperar eventos del rastro transversal de auditoría.
/// </summary>
/// <remarks>
/// Esta consulta permite filtrar la trazabilidad del sistema por agregado, módulo,
/// actor o correlación, y está orientada a escenarios administrativos y de soporte
/// sobre la interfaz de lectura del <c>audit_trail</c>.
/// </remarks>
public sealed class GetAuditTrailQuery
{
    /// <summary>
    /// Obtiene o establece el identificador del agregado auditado cuando se desea filtrar por él.
    /// </summary>
    public Guid? AggregateId { get; init; }

    /// <summary>
    /// Obtiene o establece el tipo del agregado auditado.
    /// </summary>
    public string? AggregateType { get; init; }

    /// <summary>
    /// Obtiene o establece el módulo funcional asociado a los eventos buscados.
    /// </summary>
    public string? Module { get; init; }

    /// <summary>
    /// Obtiene o establece la acción funcional registrada.
    /// </summary>
    public string? Action { get; init; }

    /// <summary>
    /// Obtiene o establece el actor visible responsable del evento.
    /// </summary>
    public string? PerformedBy { get; init; }

    /// <summary>
    /// Obtiene o establece el identificador de correlación asociado al flujo de ejecución.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Obtiene o establece la fecha UTC mínima incluida en la búsqueda.
    /// </summary>
    public DateTime? FromUtc { get; init; }

    /// <summary>
    /// Obtiene o establece la fecha UTC máxima incluida en la búsqueda.
    /// </summary>
    public DateTime? ToUtc { get; init; }

    /// <summary>
    /// Obtiene o establece el número de página solicitado.
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Obtiene o establece el tamaño de página aplicado a la consulta.
    /// </summary>
    public int PageSize { get; init; } = 25;

    /// <summary>
    /// Obtiene o establece un valor que indica si el orden debe ser descendente por fecha.
    /// </summary>
    public bool SortDescending { get; init; } = true;
}
