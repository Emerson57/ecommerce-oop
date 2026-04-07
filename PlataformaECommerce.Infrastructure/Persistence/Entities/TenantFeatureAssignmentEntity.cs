namespace PlataformaECommerce.Infrastructure.Persistence.Entities;

/// <summary>
/// Representa la habilitación explícita de un feature para un tenant específico.
/// </summary>
public sealed class TenantFeatureAssignmentEntity
{
    /// <summary>
    /// Identificador técnico del tenant propietario de la asignación.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Identificador técnico del feature habilitado.
    /// </summary>
    public string FeatureId { get; set; } = string.Empty;

    /// <summary>
    /// Navegación hacia el tenant propietario.
    /// </summary>
    public TenantEntity Tenant { get; set; } = null!;

    /// <summary>
    /// Navegación hacia el feature habilitado.
    /// </summary>
    public TenantFeatureEntity Feature { get; set; } = null!;
}
