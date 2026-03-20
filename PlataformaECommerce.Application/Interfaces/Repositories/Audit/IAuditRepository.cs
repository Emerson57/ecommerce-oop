namespace PlataformaECommerce.Application.Interfaces.Repositories.Audit;

/// <summary>
/// Define el contrato del repositorio responsable de persistir y consultar
/// eventos de auditoría transversales dentro del sistema.
/// </summary>
/// <remarks>
/// Esta abstracción pertenece a la capa Application y describe una frontera estable
/// para la trazabilidad funcional de agregados como productos, usuarios, carritos
/// y pedidos, evitando dependencia directa respecto de MongoDB u otros mecanismos
/// concretos de almacenamiento documental.
/// </remarks>
public interface IAuditRepository
{
    /// <summary>
    /// Registra un evento de auditoría transversal.
    /// </summary>
    /// <param name="entry">Evento semántico de auditoría a persistir.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Una tarea que representa la operación asíncrona.</returns>
    Task RegisterEventAsync(
        AuditEntry entry,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el historial de auditoría registrado para un agregado específico.
    /// </summary>
    /// <param name="aggregateId">Identificador del agregado auditado.</param>
    /// <param name="aggregateType">Tipo de agregado auditado.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Una colección de eventos de auditoría asociados al agregado indicado.</returns>
    Task<IReadOnlyCollection<AuditEntry>> GetHistoryAsync(
        Guid aggregateId,
        string aggregateType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca eventos de auditoría aplicando filtros funcionales sobre el rastro transversal.
    /// </summary>
    /// <param name="filter">Criterios de búsqueda a aplicar.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Una colección de eventos de auditoría que cumplen los filtros indicados.</returns>
    Task<AuditSearchResult> SearchAsync(
        AuditSearchFilter filter,
        CancellationToken cancellationToken = default);
}
