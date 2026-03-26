using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace PlataformaECommerce.Web.Services.Categories;

/// <summary>
/// Centraliza la plantilla Excel descargable utilizada para importación masiva de categorías.
/// </summary>
internal static class CategoryExcelTemplateProvider
{
    /// <summary>
    /// Nombre sugerido para el archivo de plantilla Excel.
    /// </summary>
    internal const string FileName = "plantilla-categorias.xlsx";

    /// <summary>
    /// Tipo de contenido HTTP para la descarga de la plantilla Excel.
    /// </summary>
    internal const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    /// <summary>
    /// Construye la plantilla Excel en memoria.
    /// </summary>
    internal static byte[] BuildTemplateBytes()
    {
        using MemoryStream stream = new();
        using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook, true))
        {
            WorkbookPart workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            SheetData sheetData = new();
            worksheetPart.Worksheet = new Worksheet(sheetData);

            Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
            sheets.Append(new Sheet
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "Categorias"
            });

            AppendRow(sheetData, CategoryImportTemplateData.Headers);

            foreach (CategoryImportTemplateRow row in CategoryImportTemplateData.SampleRows)
            {
                AppendRow(sheetData,
                [
                    row.Name,
                    row.Slug,
                    row.Description ?? string.Empty,
                    row.IsActive ? "true" : "false",
                    row.ParentCategoryName ?? string.Empty
                ]);
            }

            workbookPart.Workbook.Save();
        }

        return stream.ToArray();
    }

    private static void AppendRow(SheetData sheetData, IReadOnlyList<string> values)
    {
        Row row = new();
        foreach (string value in values)
        {
            row.AppendChild(new Cell
            {
                DataType = CellValues.InlineString,
                InlineString = new InlineString(new Text(value ?? string.Empty))
            });
        }

        sheetData.AppendChild(row);
    }
}
