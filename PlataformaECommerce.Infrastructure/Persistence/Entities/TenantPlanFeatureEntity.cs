namespace PlataformaECommerce.Infrastructure.Persistence.Entities;

/// <summary>
/// Representa la asociación entre un plan SaaS y un feature incluido.
/// </summary>
public sealed class TenantPlanFeatureEntity
{
    /// <summary>
    /// Identificador técnico del plan propietario.
    /// </summary>
    public string PlanId { get; set; } = string.Empty;

    /// <summary>
    /// Identificador técnico del feature incluido.
    /// </summary>
    public string FeatureId { get; set; } = string.Empty;

    /// <summary>
    /// Navegación hacia el plan propietario.
    /// </summary>
    public TenantPlanEntity Plan { get; set; } = null!;

    /// <summary>
    /// Navegación hacia el feature incluido.
    /// </summary>
    public TenantFeatureEntity Feature { get; set; } = null!;
}
