namespace PlataformaECommerce.Infrastructure.Persistence.Entities;

/// <summary>
/// Representa un feature comercializable del catálogo SaaS.
/// </summary>
public sealed class TenantFeatureEntity
{
    /// <summary>
    /// Identificador técnico único del feature.
    /// </summary>
    public string FeatureId { get; set; } = string.Empty;

    /// <summary>
    /// Nombre visible del feature.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Descripción funcional breve del feature.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Categoría comercial u operativa del feature.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Indica si el feature se encuentra habilitado en el catálogo.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Relación con planes que incluyen el feature.
    /// </summary>
    public ICollection<TenantPlanFeatureEntity> PlanFeatures { get; set; } = new List<TenantPlanFeatureEntity>();

    /// <summary>
    /// Relación con tenants que habilitan explícitamente el feature.
    /// </summary>
    public ICollection<TenantFeatureAssignmentEntity> TenantAssignments { get; set; } = new List<TenantFeatureAssignmentEntity>();
}
