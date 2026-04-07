namespace PlataformaECommerce.Infrastructure.Persistence.Entities;

/// <summary>
/// Representa un plan comercial disponible en el catálogo SaaS.
/// </summary>
public sealed class TenantPlanEntity
{
    /// <summary>
    /// Identificador técnico único del plan.
    /// </summary>
    public string PlanId { get; set; } = string.Empty;

    /// <summary>
    /// Nombre visible del plan.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Descripción comercial del plan.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Precio mensual de referencia del plan.
    /// </summary>
    public decimal MonthlyPrice { get; set; }

    /// <summary>
    /// Moneda del precio de referencia del plan.
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad de administradores incluidos por el plan.
    /// </summary>
    public int IncludedAdministrators { get; set; }

    /// <summary>
    /// Cantidad de productos incluidos por el plan.
    /// </summary>
    public int IncludedProducts { get; set; }

    /// <summary>
    /// Indica si el plan se encuentra habilitado para asignación.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Features incluidos por el plan.
    /// </summary>
    public ICollection<TenantPlanFeatureEntity> PlanFeatures { get; set; } = new List<TenantPlanFeatureEntity>();
}
