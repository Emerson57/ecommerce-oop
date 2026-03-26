using System.Text;

namespace PlataformaECommerce.Web.Services.Categories;

/// <summary>
/// Centraliza la plantilla CSV descargable utilizada para importación masiva de categorías.
/// </summary>
internal static class CategoryCsvTemplateProvider
{
    /// <summary>
    /// Nombre sugerido para el archivo de plantilla CSV.
    /// </summary>
    internal const string FileName = "plantilla-categorias.csv";

    /// <summary>
    /// Tipo de contenido HTTP para la descarga de la plantilla CSV.
    /// </summary>
    internal const string ContentType = "text/csv";

    /// <summary>
    /// Construye el contenido textual de la plantilla CSV.
    /// </summary>
    internal static string BuildTemplate()
    {
        StringBuilder builder = new();
        builder.AppendLine(string.Join(',', CategoryImportTemplateData.Headers));

        foreach (CategoryImportTemplateRow row in CategoryImportTemplateData.SampleRows)
        {
            builder.AppendLine(string.Join(',',
                Escape(row.Name),
                Escape(row.Slug),
                Escape(row.Description),
                Escape(row.IsActive ? "true" : "false"),
                Escape(row.ParentCategoryName)));
        }

        return builder.ToString();
    }

    /// <summary>
    /// Convierte la plantilla CSV en bytes UTF-8 listos para descarga.
    /// </summary>
    internal static byte[] BuildTemplateBytes()
        => Encoding.UTF8.GetBytes(BuildTemplate());

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        string normalizedValue = value.Replace("\"", "\"\"");
        return normalizedValue.IndexOfAny([',', '"', '\r', '\n']) >= 0
            ? $"\"{normalizedValue}\""
            : normalizedValue;
    }
}
