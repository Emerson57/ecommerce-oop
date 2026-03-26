namespace PlataformaECommerce.Application.Features.Categories.Commands;

/// <summary>
/// Representa el comando para registrar una nueva categoría del catálogo.
/// </summary>
public sealed class CreateCategoryCommand
{
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
    /// Identificador de la categoría padre cuando se registra una subcategoría.
    /// </summary>
    public Guid? ParentCategoryId { get; init; }

    /// <summary>
    /// Indica si la categoría debe quedar activa al finalizar el alta.
    /// </summary>
    public bool IsActive { get; init; } = true;
}
