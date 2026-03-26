namespace PlataformaECommerce.Web.Services.Products;

/// <summary>
/// Centraliza la estructura oficial de la plantilla Excel de importación de productos.
/// </summary>
internal static class ProductImportTemplateData
{
    /// <summary>
    /// Cantidad de filas de captura preparadas en la plantilla descargable.
    /// </summary>
    internal const int TemplateRowCount = 200;

    /// <summary>
    /// Encabezados oficiales de la plantilla de importación de productos.
    /// </summary>
    internal static IReadOnlyList<string> Headers { get; } =
    [
        "Nombre",
        "Descripcion",
        "SKU",
        "Precio",
        "Moneda",
        "Stock",
        "Activo",
        "TipoProducto",
        "Slug",
        "Categoria",
        "Subcategoria",
        "EtiquetasSerializadas",
        "FormatoArchivo",
        "TamanoMB",
        "RequiereLicencia",
        "PesoKg",
        "AltoCm",
        "AnchoCm",
        "LargoCm",
        "RequiereEnvio"
    ];

    /// <summary>
    /// Valores válidos de tipo de producto dentro de la plantilla.
    /// </summary>
    internal static IReadOnlyList<string> ProductTypes { get; } =
    [
        "Digital",
        "Fisico"
    ];

    /// <summary>
    /// Valores booleanos aceptados en las listas desplegables de la plantilla.
    /// </summary>
    internal static IReadOnlyList<string> BooleanValues { get; } =
    [
        "true",
        "false"
    ];
}
