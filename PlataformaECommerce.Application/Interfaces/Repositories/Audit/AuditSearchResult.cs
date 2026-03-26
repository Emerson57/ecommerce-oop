namespace PlataformaECommerce.Application.Interfaces.Repositories.Audit;

/// <summary>
/// Representa el resultado paginado de una búsqueda sobre el rastro transversal de auditoría.
/// </summary>
/// <remarks>
/// Este modelo permite transportar simultáneamente los eventos recuperados y las métricas
/// de paginación calculadas por la infraestructura documental, evitando que capas superiores
/// infieran el total a partir de resultados truncados.
/// </remarks>
public sealed record AuditSearchResult
{
    /// <summary>
    /// Obtiene o establece la colección de eventos de auditoría recuperados.
    /// </summary>
    public IReadOnlyCollection<AuditEntry> Items { get; init; } = Array.Empty<AuditEntry>();

    /// <summary>
    /// Obtiene o establece la cantidad total de coincidencias encontradas para el filtro aplicado.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Obtiene o establece el número de página solicitado.
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Obtiene o establece el tamaño de página aplicado a la consulta.
    /// </summary>
    public int PageSize { get; init; } = 25;
}
