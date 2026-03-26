namespace PlataformaECommerce.Application.Features.Categories.Commands;

/// <summary>
/// Representa el comando para actualizar una categoría existente.
/// </summary>
public sealed class UpdateCategoryCommand
{
    /// <summary>
    /// Identificador de la categoría a actualizar.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Nombre visible de la categoría.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Slug único de la categoría.
    /// </summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>
    /// Descripción opcional de la categoría.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Identificador de la categoría padre cuando se trata de una subcategoría.
    /// </summary>
    public Guid? ParentCategoryId { get; init; }

    /// <summary>
    /// Indica si la categoría debe quedar activa tras la actualización.
    /// </summary>
    public bool IsActive { get; init; }
}
