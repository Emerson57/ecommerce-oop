namespace PlataformaECommerce.Application.Interfaces.Services.Audit;

/// <summary>
/// Define el contrato del servicio de aplicación responsable de construir y registrar
/// trazas homogéneas de auditoría para los módulos funcionales del sistema.
/// </summary>
/// <remarks>
/// Este contrato actúa como colaboración transversal interna de <c>Application</c>.
/// Su responsabilidad es encapsular la composición de información contextual como actor,
/// correlación, origen y tiempo de ocurrencia, permitiendo que otros servicios de
/// aplicación expresen únicamente la semántica del evento auditado sin exponer una
/// segunda frontera pública del módulo <c>Audit</c>.
/// </remarks>
public interface IAuditTrailService
{
    /// <summary>
    /// Registra una traza de auditoría para un agregado específico del sistema.
    /// </summary>
    /// <param name="aggregateId">Identificador del agregado auditado.</param>
    /// <param name="aggregateType">Tipo de agregado auditado.</param>
    /// <param name="module">Módulo funcional al que pertenece el evento.</param>
    /// <param name="action">Acción funcional registrada.</param>
    /// <param name="detail">Detalle legible del evento auditado.</param>
    /// <param name="metadata">Metadatos complementarios del evento.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Una tarea que representa la operación asíncrona.</returns>
    Task RegisterAsync(
        Guid aggregateId,
        string aggregateType,
        string module,
        string action,
        string detail,
        IReadOnlyDictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default);
}
