namespace PlataformaECommerce.Application.Common.SaaS;

/// <summary>
/// Representa la definición inicial de aprovisionamiento recomendada para un tenant.
/// </summary>
public sealed record TenantProvisioningDefinition
{
    /// <summary>
    /// Correo sugerido para el bootstrap del primer super usuario del tenant.
    /// </summary>
    public string BootstrapSuperUserEmail { get; init; } = string.Empty;

    /// <summary>
    /// Indica si el aprovisionamiento inicial contempla sembrar categorías base.
    /// </summary>
    public bool SeedBaseCategories { get; init; }

    /// <summary>
    /// Indica si el aprovisionamiento inicial contempla sembrar catálogo demo.
    /// </summary>
    public bool SeedDemoCatalog { get; init; }

    /// <summary>
    /// Indica si el storefront público debe quedar habilitado desde el arranque.
    /// </summary>
    public bool EnablePublicStorefront { get; init; }

    /// <summary>
    /// Notas operativas adicionales para el tenant.
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Fecha UTC en la que se confirmó el bootstrap del super usuario inicial.
    /// </summary>
    public DateTime? SuperUserProvisionedAtUtc { get; init; }

    /// <summary>
    /// Fecha UTC en la que se confirmó la provisión de categorías base.
    /// </summary>
    public DateTime? BaseCategoriesProvisionedAtUtc { get; init; }

    /// <summary>
    /// Fecha UTC en la que se confirmó la provisión del catálogo demo.
    /// </summary>
    public DateTime? DemoCatalogProvisionedAtUtc { get; init; }
}
