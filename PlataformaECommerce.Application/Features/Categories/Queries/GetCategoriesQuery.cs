namespace PlataformaECommerce.Application.Features.Categories.Queries;

/// <summary>
/// Representa la consulta para listar categorías del catálogo.
/// </summary>
public sealed class GetCategoriesQuery
{
    /// <summary>
    /// Indica si solo deben devolverse categorías activas.
    /// </summary>
    public bool OnlyActive { get; init; }

    /// <summary>
    /// Indica si solo deben devolverse categorías raíz.
    /// </summary>
    public bool RootOnly { get; init; }

    /// <summary>
    /// Identificador opcional del padre para listar únicamente sus hijas.
    /// </summary>
    public Guid? ParentCategoryId { get; init; }
}
