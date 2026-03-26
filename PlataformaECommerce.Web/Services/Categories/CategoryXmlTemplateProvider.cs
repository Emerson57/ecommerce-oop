using System.Text;
using System.Xml.Linq;

namespace PlataformaECommerce.Web.Services.Categories;

/// <summary>
/// Centraliza la plantilla XML descargable utilizada para importación masiva de categorías.
/// </summary>
internal static class CategoryXmlTemplateProvider
{
    /// <summary>
    /// Nombre sugerido para el archivo de plantilla.
    /// </summary>
    internal const string FileName = "plantilla-categorias.xml";

    /// <summary>
    /// Tipo de contenido HTTP para la descarga de la plantilla XML.
    /// </summary>
    internal const string ContentType = "application/xml";

    /// <summary>
    /// Construye el contenido textual de la plantilla XML.
    /// </summary>
    internal static string BuildTemplate()
    {
        XDocument document = BuildTemplateDocument(CategoryImportTemplateData.SampleRows);
        return document.Declaration is null
            ? document.ToString(SaveOptions.DisableFormatting)
            : document.Declaration + document.ToString(SaveOptions.DisableFormatting);
    }

    /// <summary>
    /// Convierte la plantilla XML en bytes UTF-8 listos para descarga.
    /// </summary>
    internal static byte[] BuildTemplateBytes()
        => Encoding.UTF8.GetBytes(BuildTemplate());

    internal static XDocument BuildTemplateDocument(IEnumerable<CategoryImportTemplateRow> rows)
    {
        return new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("Categories",
                rows.Select(row =>
                    new XElement("Category",
                        new XElement("Name", row.Name),
                        new XElement("Slug", row.Slug),
                        new XElement("Description", row.Description ?? string.Empty),
                        new XElement("IsActive", row.IsActive ? "true" : "false"),
                        new XElement("ParentCategoryName", row.ParentCategoryName ?? string.Empty)))));
    }
}
