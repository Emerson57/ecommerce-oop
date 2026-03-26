namespace PlataformaECommerce.Web.Services.Categories;

/// <summary>
/// Centraliza las columnas y filas base utilizadas por las plantillas de importación de categorías.
/// </summary>
internal static class CategoryImportTemplateData
{
    /// <summary>
    /// Encabezados oficiales del contrato tabular de importación.
    /// </summary>
    internal static IReadOnlyList<string> Headers { get; } =
    [
        "Name",
        "Slug",
        "Description",
        "IsActive",
        "ParentCategoryName"
    ];

    /// <summary>
    /// Filas de ejemplo incluidas en las plantillas descargables.
    /// </summary>
    internal static IReadOnlyList<CategoryImportTemplateRow> SampleRows { get; } =
    [
        new("Tecnologia", "tecnologia", "Categoria principal para dispositivos y accesorios.", true, null),
        new("Laptops", "laptops", "Computadores portatiles y estaciones moviles.", true, "Tecnologia"),
        new("Monitores", "monitores", "Pantallas para productividad y entretenimiento.", true, "Tecnologia"),
        new("Hogar", "hogar", "Categoria principal para productos del hogar.", true, null),
        new("Iluminacion", "iluminacion", "Soluciones decorativas y funcionales para espacios interiores.", true, "Hogar")
    ];
}
