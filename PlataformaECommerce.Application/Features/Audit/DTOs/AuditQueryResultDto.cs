namespace PlataformaECommerce.Application.Features.Audit.DTOs;

/// <summary>
/// Representa el resultado consolidado de una consulta de auditoría transversal.
/// </summary>
/// <remarks>
/// Este DTO agrupa la colección proyectada de eventos auditados junto con métricas
/// básicas del resultado, permitiendo una evolución futura hacia escenarios con
/// paginación o recuentos enriquecidos.
/// </remarks>
public sealed record AuditQueryResultDto
{
    /// <summary>
    /// Obtiene o establece la colección de eventos de auditoría recuperados.
    /// </summary>
    public IReadOnlyCollection<AuditEntryDto> Items { get; init; } = Array.Empty<AuditEntryDto>();

    /// <summary>
    /// Obtiene o establece la cantidad total de eventos devueltos por la consulta.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Obtiene o establece la cantidad de eventos incluidos efectivamente en la respuesta.
    /// </summary>
    public int ReturnedCount { get; init; }

    /// <summary>
    /// Obtiene o establece el número de página actual del resultado.
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Obtiene o establece el tamaño de página aplicado a la consulta.
    /// </summary>
    public int PageSize { get; init; } = 25;

    /// <summary>
    /// Obtiene o establece la cantidad total de páginas calculadas para la consulta.
    /// </summary>
    public int TotalPages { get; init; }

    /// <summary>
    /// Obtiene o establece un valor que indica si existe una página anterior disponible.
    /// </summary>
    public bool HasPreviousPage { get; init; }

    /// <summary>
    /// Obtiene o establece un valor que indica si existe una página siguiente disponible.
    /// </summary>
    public bool HasNextPage { get; init; }
}
