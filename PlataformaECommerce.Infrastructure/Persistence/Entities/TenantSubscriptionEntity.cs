namespace PlataformaECommerce.Infrastructure.Persistence.Entities;

/// <summary>
/// Representa la suscripción contractual efectiva de un tenant.
/// </summary>
public sealed class TenantSubscriptionEntity
{
    /// <summary>
    /// Identificador técnico del tenant propietario de la suscripción.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Identificador del plan asociado actualmente a la suscripción.
    /// </summary>
    public string? PlanId { get; set; }

    /// <summary>
    /// Estado contractual actual de la suscripción.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de inicio UTC de la suscripción.
    /// </summary>
    public DateTime? StartedAtUtc { get; set; }

    /// <summary>
    /// Fecha UTC de fin de trial cuando aplica.
    /// </summary>
    public DateTime? TrialEndsAtUtc { get; set; }

    /// <summary>
    /// Fecha UTC de próxima renovación cuando aplica.
    /// </summary>
    public DateTime? RenewalAtUtc { get; set; }

    /// <summary>
    /// Indica si la suscripción se renueva automáticamente.
    /// </summary>
    public bool AutoRenew { get; set; }

    /// <summary>
    /// Cantidad de asientos administrativos contratados.
    /// </summary>
    public int SeatsPurchased { get; set; }

    /// <summary>
    /// Navegación hacia el tenant propietario.
    /// </summary>
    public TenantEntity Tenant { get; set; } = null!;

    /// <summary>
    /// Navegación hacia el plan actualmente asociado.
    /// </summary>
    public TenantPlanEntity? Plan { get; set; }
}
