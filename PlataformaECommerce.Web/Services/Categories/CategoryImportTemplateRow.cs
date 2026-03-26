namespace PlataformaECommerce.Web.Services.Categories;

/// <summary>
/// Representa una fila tabular reutilizable para las plantillas e importaciones de categorías.
/// </summary>
internal sealed record CategoryImportTemplateRow(
    string Name,
    string Slug,
    string? Description,
    bool IsActive,
    string? ParentCategoryName);
