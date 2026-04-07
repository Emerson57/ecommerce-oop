namespace PlataformaECommerce.Application.Common.SaaS;

/// <summary>
/// Representa el estado contractual de suscripción del tenant dentro de la plataforma SaaS.
/// </summary>
public sealed record TenantSubscriptionDefinition
{
    /// <summary>
    /// Identificador del plan asociado actualmente a la suscripción.
    /// </summary>
    public string PlanId { get; init; } = string.Empty;

    /// <summary>
    /// Estado contractual actual de la suscripción.
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Fecha de inicio UTC de la suscripción.
    /// </summary>
    public DateTime? StartedAtUtc { get; init; }

    /// <summary>
    /// Fecha UTC de fin de trial cuando aplica.
    /// </summary>
    public DateTime? TrialEndsAtUtc { get; init; }

    /// <summary>
    /// Fecha UTC de próxima renovación cuando aplica.
    /// </summary>
    public DateTime? RenewalAtUtc { get; init; }

    /// <summary>
    /// Indica si la suscripción se renueva automáticamente.
    /// </summary>
    public bool AutoRenew { get; init; }

    /// <summary>
    /// Cantidad de asientos administrativos contratados.
    /// </summary>
    public int SeatsPurchased { get; init; }
}
