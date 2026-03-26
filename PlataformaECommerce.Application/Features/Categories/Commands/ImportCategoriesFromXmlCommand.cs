namespace PlataformaECommerce.Application.Features.Categories.Commands;

/// <summary>
/// Representa el comando para importar categorías y subcategorías desde un documento XML.
/// </summary>
public sealed class ImportCategoriesFromXmlCommand
{
    /// <summary>
    /// Contenido XML a procesar para registrar la jerarquía de categorías.
    /// </summary>
    public string XmlContent { get; init; } = string.Empty;
}
