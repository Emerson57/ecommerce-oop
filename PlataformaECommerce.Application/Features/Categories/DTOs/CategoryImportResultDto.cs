namespace PlataformaECommerce.Application.Features.Categories.DTOs;

/// <summary>
/// Representa el resultado resumido de una importación XML de categorías.
/// </summary>
public sealed class CategoryImportResultDto
{
    /// <summary>
    /// Cantidad de categorías raíz creadas.
    /// </summary>
    public int RootCategoriesCreated { get; init; }

    /// <summary>
    /// Cantidad de subcategorías creadas.
    /// </summary>
    public int SubcategoriesCreated { get; init; }

    /// <summary>
    /// Cantidad total de nodos creados durante la importación.
    /// </summary>
    public int TotalCreated => RootCategoriesCreated + SubcategoriesCreated;
}
