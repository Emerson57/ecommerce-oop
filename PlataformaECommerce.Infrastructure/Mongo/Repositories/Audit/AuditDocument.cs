using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PlataformaECommerce.Infrastructure.Mongo.Repositories.Audit;

/// <summary>
/// Representa el documento documental persistido en MongoDB para la auditoría transversal del sistema.
/// </summary>
/// <remarks>
/// Este documento conserva la traza operativa completa de eventos relevantes de productos,
/// usuarios, carritos y pedidos, incluyendo el agregado afectado, el actor responsable,
/// la correlación del flujo y los metadatos complementarios necesarios para análisis e investigación.
/// </remarks>
public sealed class AuditDocument
{
    /// <summary>
    /// Obtiene o establece el identificador único del documento en MongoDB.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el identificador del agregado auditado.
    /// </summary>
    public Guid AggregateId { get; set; }

    /// <summary>
    /// Obtiene o establece el tipo del agregado auditado.
    /// </summary>
    public string AggregateType { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el módulo funcional al que pertenece el evento.
    /// </summary>
    public string Module { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la acción funcional registrada.
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la descripción detallada del evento auditado.
    /// </summary>
    public string Detail { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el actor visible responsable del evento.
    /// </summary>
    public string PerformedBy { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el identificador del usuario responsable cuando está disponible.
    /// </summary>
    public string? PerformedByUserId { get; set; }

    /// <summary>
    /// Obtiene o establece la fecha y hora UTC en la que ocurrió el evento.
    /// </summary>
    public DateTime OccurredAtUtc { get; set; }

    /// <summary>
    /// Obtiene o establece el identificador de correlación asociado al flujo de ejecución.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Obtiene o establece el origen técnico o funcional que generó la auditoría.
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Obtiene o establece los metadatos adicionales asociados al evento.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
