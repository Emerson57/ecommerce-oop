namespace PlataformaECommerce.Infrastructure.Persistence.Entities;

/// <summary>
/// Representa la definición operativa y el estado mínimo del aprovisionamiento inicial de un tenant.
/// </summary>
public sealed class TenantProvisioningEntity
{
    /// <summary>
    /// Identificador técnico del tenant propietario de la definición de provisión.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Correo sugerido para el bootstrap del primer super usuario.
    /// </summary>
    public string BootstrapSuperUserEmail { get; set; } = string.Empty;

    /// <summary>
    /// Indica si el aprovisionamiento contempla sembrar categorías base.
    /// </summary>
    public bool SeedBaseCategories { get; set; }

    /// <summary>
    /// Indica si el aprovisionamiento contempla sembrar catálogo demo.
    /// </summary>
    public bool SeedDemoCatalog { get; set; }

    /// <summary>
    /// Indica si el storefront público debe quedar habilitado al aprovisionar el tenant.
    /// </summary>
    public bool EnablePublicStorefront { get; set; }

    /// <summary>
    /// Notas operativas adicionales del aprovisionamiento.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Fecha UTC en la que se confirmó el bootstrap del super usuario inicial.
    /// </summary>
    public DateTime? SuperUserProvisionedAtUtc { get; set; }

    /// <summary>
    /// Fecha UTC en la que se confirmó la provisión de categorías base.
    /// </summary>
    public DateTime? BaseCategoriesProvisionedAtUtc { get; set; }

    /// <summary>
    /// Fecha UTC en la que se confirmó la provisión del catálogo demo.
    /// </summary>
    public DateTime? DemoCatalogProvisionedAtUtc { get; set; }

    /// <summary>
    /// Fecha UTC de la última sincronización aplicada desde configuración hacia persistencia.
    /// </summary>
    public DateTime? LastSynchronizedAtUtc { get; set; }

    /// <summary>
    /// Navegación hacia el tenant propietario.
    /// </summary>
    public TenantEntity Tenant { get; set; } = null!;
}
