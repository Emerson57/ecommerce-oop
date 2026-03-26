namespace PlataformaECommerce.Application.Interfaces.Repositories.Audit;

/// <summary>
/// Representa un evento de auditoría transversal asociado a cualquier agregado del sistema.
/// </summary>
/// <remarks>
/// Este modelo actúa como frontera semántica entre la capa Application y la infraestructura
/// de auditoría, permitiendo registrar trazabilidad rica y reutilizable sin exponer detalles
/// de MongoDB, BSON ni convenciones específicas de persistencia documental.
/// </remarks>
public sealed record AuditEntry
{
    /// <summary>
    /// Obtiene o establece el identificador del agregado auditado.
    /// </summary>
    public Guid AggregateId { get; init; }

    /// <summary>
    /// Obtiene o establece el tipo de agregado auditado.
    /// </summary>
    public string AggregateType { get; init; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el módulo funcional al que pertenece el evento.
    /// </summary>
    public string Module { get; init; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la acción funcional registrada.
    /// </summary>
    public string Action { get; init; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la descripción detallada del evento auditado.
    /// </summary>
    public string Detail { get; init; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el actor visible responsable de la operación.
    /// </summary>
    public string PerformedBy { get; init; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el identificador del usuario responsable cuando se encuentra disponible.
    /// </summary>
    public string? PerformedByUserId { get; init; }

    /// <summary>
    /// Obtiene o establece la fecha y hora UTC en la que ocurrió el evento auditado.
    /// </summary>
    public DateTime OccurredAtUtc { get; init; }

    /// <summary>
    /// Obtiene o establece el identificador de correlación asociado al flujo de ejecución.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Obtiene o establece el origen técnico o funcional desde el cual se generó el evento.
    /// </summary>
    public string? Source { get; init; }

    /// <summary>
    /// Obtiene o establece los metadatos complementarios del evento auditado.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }
}
