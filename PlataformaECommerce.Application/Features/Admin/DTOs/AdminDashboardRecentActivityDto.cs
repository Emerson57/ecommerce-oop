namespace PlataformaECommerce.Application.Features.Admin.DTOs;

/// <summary>
/// Representa una actividad reciente del rastro de auditoría para el dashboard administrativo.
/// </summary>
/// <remarks>
/// Este DTO resume los datos mínimos necesarios para presentar actividad operativa
/// reciente en el backoffice sin exponer directamente el contrato completo del
/// repositorio de auditoría.
/// </remarks>
public sealed record AdminDashboardRecentActivityDto
{
    /// <summary>
    /// Obtiene o establece la fecha y hora UTC en la que ocurrió la actividad.
    /// </summary>
    public DateTime OccurredAtUtc { get; init; }

    /// <summary>
    /// Obtiene o establece el módulo funcional asociado a la actividad.
    /// </summary>
    public string Module { get; init; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la acción funcional registrada.
    /// </summary>
    public string Action { get; init; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el detalle legible de la actividad.
    /// </summary>
    public string Detail { get; init; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el actor visible responsable del evento.
    /// </summary>
    public string PerformedBy { get; init; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el identificador de correlación asociado al evento cuando esté disponible.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Obtiene o establece el origen funcional o técnico del evento cuando esté disponible.
    /// </summary>
    public string? Source { get; init; }
}
