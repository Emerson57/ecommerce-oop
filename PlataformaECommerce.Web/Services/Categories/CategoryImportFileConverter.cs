using System.Globalization;
using System.Text;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualBasic.FileIO;
using PlataformaECommerce.Application.Common.Results;

namespace PlataformaECommerce.Web.Services.Categories;

/// <summary>
/// Convierte archivos de importación de categorías en el contrato XML canónico consumido por Application.
/// </summary>
internal static class CategoryImportFileConverter
{
    private static readonly IReadOnlyDictionary<string, int> HeaderIndexes = CategoryImportTemplateData.Headers
        .Select((header, index) => new { header, index })
        .ToDictionary(item => item.header, item => item.index, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Convierte un archivo `XML`, `CSV` o `XLSX` en el contrato XML alineado con categorías.
    /// </summary>
    internal static async Task<Result<string>> ConvertToXmlAsync(IFormFile importFile, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(importFile);

        string extension = Path.GetExtension(importFile.FileName);
        return extension.ToLowerInvariant() switch
        {
            ".xml" => await ReadXmlAsync(importFile, cancellationToken),
            ".csv" => await ConvertCsvToXmlAsync(importFile, cancellationToken),
            ".xlsx" => await ConvertExcelToXmlAsync(importFile, cancellationToken),
            _ => Result.Failure<string>(Error.Validation("Categories.ImportFileTypeNotSupported", "El archivo de categorías debe ser XML, CSV o Excel (.xlsx)."))
        };
    }

    private static async Task<Result<string>> ReadXmlAsync(IFormFile importFile, CancellationToken cancellationToken)
    {
        using Stream stream = importFile.OpenReadStream();
        using StreamReader reader = new(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: false);
        string xmlContent = await reader.ReadToEndAsync(cancellationToken);
        return Result.Success(xmlContent);
    }

    private static async Task<Result<string>> ConvertCsvToXmlAsync(IFormFile importFile, CancellationToken cancellationToken)
    {
        using Stream stream = importFile.OpenReadStream();
        using TextFieldParser parser = new(stream, Encoding.UTF8, detectEncoding: true)
        {
            TextFieldType = FieldType.Delimited,
            HasFieldsEnclosedInQuotes = true,
            TrimWhiteSpace = false
        };
        parser.SetDelimiters(",");

        string[]? headers = parser.ReadFields();
        Result headersValidationResult = ValidateHeaders(headers);
        if (headersValidationResult.IsFailure)
        {
            return Result.Failure<string>(headersValidationResult.Error);
        }

        List<CategoryImportTemplateRow> rows = [];
        while (!parser.EndOfData)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string[]? fields = parser.ReadFields();
            if (fields is null || fields.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            Result<CategoryImportTemplateRow> rowResult = BuildRow(fields);
            if (rowResult.IsFailure)
            {
                return Result.Failure<string>(rowResult.Error);
            }

            rows.Add(rowResult.Value);
        }

        return BuildXmlResult(rows);
    }

    private static async Task<Result<string>> ConvertExcelToXmlAsync(IFormFile importFile, CancellationToken cancellationToken)
    {
        using Stream stream = importFile.OpenReadStream();
        using SpreadsheetDocument document = SpreadsheetDocument.Open(stream, false);

        WorkbookPart? workbookPart = document.WorkbookPart;
        WorksheetPart? worksheetPart = workbookPart?.WorksheetParts.FirstOrDefault();
        SheetData? sheetData = worksheetPart?.Worksheet.GetFirstChild<SheetData>();
        if (sheetData is null)
        {
            return Result.Failure<string>(Error.Validation("Categories.ImportExcelEmpty", "La plantilla Excel de categorías no contiene filas válidas."));
        }

        Row[] rows = sheetData.Elements<Row>().ToArray();
        if (rows.Length == 0)
        {
            return Result.Failure<string>(Error.Validation("Categories.ImportExcelEmpty", "La plantilla Excel de categorías no contiene filas válidas."));
        }

        string[] headers = ExtractRowValues(workbookPart!, rows[0], CategoryImportTemplateData.Headers.Count);

        Result headersValidationResult = ValidateHeaders(headers);
        if (headersValidationResult.IsFailure)
        {
            return Result.Failure<string>(headersValidationResult.Error);
        }

        List<CategoryImportTemplateRow> importedRows = [];
        foreach (Row row in rows.Skip(1))
        {
            cancellationToken.ThrowIfCancellationRequested();

            string[] fields = ExtractRowValues(workbookPart!, row, CategoryImportTemplateData.Headers.Count);

            if (fields.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            Result<CategoryImportTemplateRow> rowResult = BuildRow(fields);
            if (rowResult.IsFailure)
            {
                return Result.Failure<string>(rowResult.Error);
            }

            importedRows.Add(rowResult.Value);
        }

        return BuildXmlResult(importedRows);
    }

    private static Result BuildHeadersValidationError(string message)
        => Result.Failure(Error.Validation("Categories.ImportHeadersInvalid", message));

    private static Result ValidateHeaders(string[]? headers)
    {
        if (headers is null || headers.Length < CategoryImportTemplateData.Headers.Count)
        {
            return BuildHeadersValidationError("La plantilla de categorías debe incluir las columnas Name, Slug, Description, IsActive y ParentCategoryName.");
        }

        for (int index = 0; index < CategoryImportTemplateData.Headers.Count; index++)
        {
            if (!string.Equals(headers[index]?.Trim(), CategoryImportTemplateData.Headers[index], StringComparison.OrdinalIgnoreCase))
            {
                return BuildHeadersValidationError("La plantilla cargada no coincide con el orden oficial de columnas: Name, Slug, Description, IsActive y ParentCategoryName.");
            }
        }

        return Result.Success();
    }

    private static Result<CategoryImportTemplateRow> BuildRow(IReadOnlyList<string> fields)
    {
        string name = GetValue(fields, "Name");
        string slug = GetValue(fields, "Slug");
        string? description = Normalize(GetValue(fields, "Description"));
        string isActiveRaw = GetValue(fields, "IsActive");
        string? parentCategoryName = Normalize(GetValue(fields, "ParentCategoryName"));

        if (!TryParseBoolean(isActiveRaw, out bool isActive))
        {
            return Result.Failure<CategoryImportTemplateRow>(
                Error.Validation("Categories.ImportBooleanInvalid", "La columna 'IsActive' debe usar valores como true, false, VERDADERO o FALSO."));
        }

        return Result.Success(new CategoryImportTemplateRow(name, slug, description, isActive, parentCategoryName));
    }

    private static Result<string> BuildXmlResult(IEnumerable<CategoryImportTemplateRow> rows)
    {
        List<CategoryImportTemplateRow> normalizedRows = rows.ToList();
        if (normalizedRows.Count == 0)
        {
            return Result.Failure<string>(Error.Validation("Categories.ImportWithoutItems", "La plantilla cargada no contiene categorías para importar."));
        }

        XDocument document = CategoryXmlTemplateProvider.BuildTemplateDocument(normalizedRows);

        return Result.Success(document.Declaration is null
            ? document.ToString(SaveOptions.DisableFormatting)
            : document.Declaration + document.ToString(SaveOptions.DisableFormatting));
    }

    private static string GetValue(IReadOnlyList<string> fields, string header)
    {
        int index = HeaderIndexes[header];
        return index < fields.Count
            ? fields[index].Trim()
            : string.Empty;
    }

    private static string GetCellValue(WorkbookPart workbookPart, Cell cell)
    {
        string rawValue = cell.CellValue?.Text ?? cell.InnerText ?? string.Empty;
        if (cell.DataType?.Value == CellValues.SharedString)
        {
            SharedStringTablePart? sharedStringPart = workbookPart.SharedStringTablePart;
            if (sharedStringPart is not null && int.TryParse(rawValue, out int sharedStringIndex))
            {
                return sharedStringPart.SharedStringTable.Elements<SharedStringItem>().ElementAt(sharedStringIndex).InnerText;
            }
        }

        return cell.InlineString?.InnerText ?? rawValue;
    }

    private static string[] ExtractRowValues(WorkbookPart workbookPart, Row row, int expectedColumnCount)
    {
        string[] values = Enumerable.Repeat(string.Empty, expectedColumnCount).ToArray();
        Cell[] cells = row.Elements<Cell>().ToArray();
        bool hasCellReferences = cells.Any(cell => !string.IsNullOrWhiteSpace(cell.CellReference?.Value));

        if (!hasCellReferences)
        {
            for (int index = 0; index < Math.Min(cells.Length, values.Length); index++)
            {
                values[index] = GetCellValue(workbookPart, cells[index]);
            }

            return values;
        }

        foreach (Cell cell in cells)
        {
            int columnIndex = GetColumnIndex(cell.CellReference?.Value);
            if (columnIndex < 0 || columnIndex >= values.Length)
            {
                continue;
            }

            values[columnIndex] = GetCellValue(workbookPart, cell);
        }

        return values;
    }

    private static int GetColumnIndex(string? cellReference)
    {
        if (string.IsNullOrWhiteSpace(cellReference))
        {
            return -1;
        }

        int index = 0;
        foreach (char character in cellReference)
        {
            if (!char.IsLetter(character))
            {
                break;
            }

            index = (index * 26) + (char.ToUpperInvariant(character) - 'A' + 1);
        }

        return index - 1;
    }

    private static bool TryParseBoolean(string? value, out bool result)
    {
        string? normalizedValue = Normalize(value)?.ToLowerInvariant();
        switch (normalizedValue)
        {
            case "true":
            case "verdadero":
            case "1":
            case "si":
            case "sí":
            case "yes":
                result = true;
                return true;
            case "false":
            case "falso":
            case "0":
            case "no":
                result = false;
                return true;
            default:
                result = default;
                return false;
        }
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
