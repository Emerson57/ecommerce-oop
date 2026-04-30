namespace PlataformaECommerce.Infrastructure.Persistence.Entities;

/// <summary>
/// Representa la entidad persistente de auditoría transversal sobre SQL Server.
/// </summary>
public sealed class AuditEntryEntity : ITenantOwnedEntity
{
    /// <summary>
    /// Identificador único del evento de auditoría.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador lógico del tenant propietario del registro.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Identificador del agregado auditado.
    /// </summary>
    public Guid AggregateId { get; set; }

    /// <summary>
    /// Tipo del agregado auditado.
    /// </summary>
    public string AggregateType { get; set; } = string.Empty;

    /// <summary>
    /// Módulo funcional asociado al evento.
    /// </summary>
    public string Module { get; set; } = string.Empty;

    /// <summary>
    /// Acción funcional registrada.
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Descripción legible del evento.
    /// </summary>
    public string Detail { get; set; } = string.Empty;

    /// <summary>
    /// Actor visible responsable del evento.
    /// </summary>
    public string PerformedBy { get; set; } = string.Empty;

    /// <summary>
    /// Identificador del usuario responsable, cuando existe.
    /// </summary>
    public string? PerformedByUserId { get; set; }

    /// <summary>
    /// Fecha UTC en la que ocurrió el evento.
    /// </summary>
    public DateTime OccurredAtUtc { get; set; }

    /// <summary>
    /// Identificador de correlación asociado al flujo.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Origen técnico/funcional del evento.
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Metadatos complementarios serializados en JSON.
    /// </summary>
    public string MetadataJson { get; set; } = "{}";
}
