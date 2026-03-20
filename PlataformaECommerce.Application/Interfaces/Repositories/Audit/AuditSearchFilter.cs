namespace PlataformaECommerce.Application.Interfaces.Repositories.Audit;

/// <summary>
/// Representa el conjunto de criterios de búsqueda soportados por el repositorio
/// transversal de auditoría.
/// </summary>
/// <remarks>
/// Este modelo permite expresar filtros funcionales sobre el rastro de auditoría
/// sin acoplar la capa Application a detalles específicos del motor documental
/// subyacente.
/// </remarks>
public sealed record AuditSearchFilter
{
    /// <summary>
    /// Obtiene o establece el identificador del agregado auditado.
    /// </summary>
    public Guid? AggregateId { get; init; }

    /// <summary>
    /// Obtiene o establece el tipo del agregado auditado.
    /// </summary>
    public string? AggregateType { get; init; }

    /// <summary>
    /// Obtiene o establece el módulo funcional asociado a la traza.
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
    /// Obtiene o establece el identificador de correlación asociado al evento.
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
    /// Obtiene o establece el tamaño de página aplicado a la búsqueda.
    /// </summary>
    public int PageSize { get; init; } = 25;

    /// <summary>
    /// Obtiene o establece un valor que indica si el orden debe ser descendente por fecha.
    /// </summary>
    public bool SortDescending { get; init; } = true;
}
