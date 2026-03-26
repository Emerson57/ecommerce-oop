namespace PlataformaECommerce.Application.Features.Categories.DTOs;

/// <summary>
/// Representa la proyección de lectura de una categoría del catálogo.
/// </summary>
public sealed class CategoryDto
{
    /// <summary>
    /// Identificador único de la categoría.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Nombre visible de la categoría.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Slug normalizado de la categoría.
    /// </summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>
    /// Descripción opcional de la categoría.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Identificador de la categoría padre cuando aplica.
    /// </summary>
    public Guid? ParentCategoryId { get; init; }

    /// <summary>
    /// Indica si la categoría se encuentra activa.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Indica si la categoría corresponde a un nodo raíz.
    /// </summary>
    public bool IsRootCategory { get; init; }

    /// <summary>
    /// Indica si la categoría corresponde a una subcategoría.
    /// </summary>
    public bool IsSubcategory => ParentCategoryId.HasValue;
}
