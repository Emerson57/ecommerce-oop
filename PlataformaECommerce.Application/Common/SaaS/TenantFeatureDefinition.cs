namespace PlataformaECommerce.Application.Common.SaaS;

/// <summary>
/// Representa una capability funcional comercializable dentro del catálogo SaaS.
/// </summary>
public sealed record TenantFeatureDefinition
{
    /// <summary>
    /// Identificador técnico único del feature.
    /// </summary>
    public string FeatureId { get; init; } = string.Empty;

    /// <summary>
    /// Nombre visible del feature.
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Descripción funcional breve del feature.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Categoría comercial u operativa del feature.
    /// </summary>
    public string Category { get; init; } = string.Empty;
}
