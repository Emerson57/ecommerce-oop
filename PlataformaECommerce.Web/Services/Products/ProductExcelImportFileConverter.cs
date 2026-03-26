using System.Globalization;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Products.Commands;
using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Web.Services.Products;

/// <summary>
/// Convierte un archivo Excel de productos en filas tipadas listas para la capa de aplicación.
/// </summary>
internal static class ProductExcelImportFileConverter
{
    /// <summary>
    /// Convierte un archivo `XLSX` en filas normalizadas de importación de productos.
    /// </summary>
    internal static Task<Result<IReadOnlyCollection<ImportProductRowCommand>>> ConvertAsync(IFormFile importFile, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(importFile);

        if (!string.Equals(Path.GetExtension(importFile.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(Result.Failure<IReadOnlyCollection<ImportProductRowCommand>>(
                Error.Validation("Products.ImportInvalidExtension", "La plantilla de productos solo admite archivos Excel con extensión .xlsx.")));
        }

        using Stream stream = importFile.OpenReadStream();
        using SpreadsheetDocument document = SpreadsheetDocument.Open(stream, false);
        WorkbookPart? workbookPart = document.WorkbookPart;
        Sheet? productsSheet = workbookPart?.Workbook.Sheets?.Elements<Sheet>()
            .FirstOrDefault(sheet => string.Equals(sheet.Name?.Value, "Productos", StringComparison.OrdinalIgnoreCase));
        WorksheetPart? worksheetPart = productsSheet is not null
            ? (WorksheetPart?)workbookPart!.GetPartById(productsSheet.Id!)
            : workbookPart?.WorksheetParts.FirstOrDefault();
        SheetData? sheetData = worksheetPart?.Worksheet.GetFirstChild<SheetData>();
        if (sheetData is null)
        {
            return Task.FromResult(Result.Failure<IReadOnlyCollection<ImportProductRowCommand>>(
                Error.Validation("Products.ImportExcelEmpty", "El archivo Excel de productos no contiene filas válidas.")));
        }

        Row[] rows = sheetData.Elements<Row>().ToArray();
        if (rows.Length <= 1)
        {
            return Task.FromResult(Result.Failure<IReadOnlyCollection<ImportProductRowCommand>>(
                Error.Validation("Products.ImportExcelEmpty", "El archivo Excel de productos no contiene productos para importar.")));
        }

        string[] headers = ExtractRowValues(workbookPart!, rows[0], ProductImportTemplateData.Headers.Count);
        Result headerValidation = ValidateHeaders(headers);
        if (headerValidation.IsFailure)
        {
            return Task.FromResult(Result.Failure<IReadOnlyCollection<ImportProductRowCommand>>(headerValidation.Error));
        }

        List<ImportProductRowCommand> importedRows = [];
        foreach (Row row in rows.Skip(1))
        {
            cancellationToken.ThrowIfCancellationRequested();

            string[] values = ExtractRowValues(workbookPart!, row, ProductImportTemplateData.Headers.Count);
            if (values.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            Result<ImportProductRowCommand> rowResult = BuildRow((int)(row.RowIndex?.Value ?? 0), values);
            if (rowResult.IsFailure)
            {
                return Task.FromResult(Result.Failure<IReadOnlyCollection<ImportProductRowCommand>>(rowResult.Error));
            }

            importedRows.Add(rowResult.Value);
        }

        return Task.FromResult(importedRows.Count == 0
            ? Result.Failure<IReadOnlyCollection<ImportProductRowCommand>>(Error.Validation("Products.ImportWithoutItems", "El archivo Excel no contiene productos para importar."))
            : Result.Success<IReadOnlyCollection<ImportProductRowCommand>>(importedRows));
    }

    private static Result ValidateHeaders(IReadOnlyList<string> headers)
    {
        if (headers.Count < ProductImportTemplateData.Headers.Count)
        {
            return Result.Failure(Error.Validation("Products.ImportHeadersInvalid", "La plantilla Excel de productos no coincide con las columnas oficiales requeridas."));
        }

        for (int index = 0; index < ProductImportTemplateData.Headers.Count; index++)
        {
            if (!string.Equals(headers[index], ProductImportTemplateData.Headers[index], StringComparison.OrdinalIgnoreCase))
            {
                return Result.Failure(Error.Validation("Products.ImportHeadersInvalid", "La plantilla Excel de productos no coincide con el orden oficial de columnas requerido por el sistema."));
            }
        }

        return Result.Success();
    }

    private static Result<ImportProductRowCommand> BuildRow(int rowNumber, IReadOnlyList<string> values)
    {
        if (!TryParseDecimal(GetValue(values, 3), rowNumber, "Precio", out decimal price, out Error? decimalError))
        {
            return Result.Failure<ImportProductRowCommand>(decimalError!);
        }

        if (!TryParseInteger(GetValue(values, 5), rowNumber, "Stock", out int stock, out Error? integerError))
        {
            return Result.Failure<ImportProductRowCommand>(integerError!);
        }

        if (!TryParseBoolean(GetValue(values, 6), out bool isActive))
        {
            return Result.Failure<ImportProductRowCommand>(Error.Validation("Products.ImportBooleanInvalid", $"La fila {rowNumber} contiene un valor inválido en 'Activo'."));
        }

        if (!TryParseProductType(GetValue(values, 7), out TipoProducto productType))
        {
            return Result.Failure<ImportProductRowCommand>(Error.Validation("Products.ImportInvalidType", $"La fila {rowNumber} contiene un valor inválido en 'TipoProducto'. Solo se admite Digital o Fisico."));
        }

        if (!TryParseOptionalBoolean(GetValue(values, 14), rowNumber, "RequiereLicencia", out bool? requiresLicense, out Error? requiresLicenseError))
        {
            return Result.Failure<ImportProductRowCommand>(requiresLicenseError!);
        }

        if (!TryParseOptionalBoolean(GetValue(values, 19), rowNumber, "RequiereEnvio", out bool? requiresShipping, out Error? requiresShippingError))
        {
            return Result.Failure<ImportProductRowCommand>(requiresShippingError!);
        }

        if (!TryParseOptionalDecimal(GetValue(values, 13), rowNumber, "TamanoMB", out decimal? fileSizeMb, out Error? fileSizeError))
        {
            return Result.Failure<ImportProductRowCommand>(fileSizeError!);
        }

        if (!TryParseOptionalDecimal(GetValue(values, 15), rowNumber, "PesoKg", out decimal? weightKg, out Error? weightError))
        {
            return Result.Failure<ImportProductRowCommand>(weightError!);
        }

        if (!TryParseOptionalDecimal(GetValue(values, 16), rowNumber, "AltoCm", out decimal? heightCm, out Error? heightError))
        {
            return Result.Failure<ImportProductRowCommand>(heightError!);
        }

        if (!TryParseOptionalDecimal(GetValue(values, 17), rowNumber, "AnchoCm", out decimal? widthCm, out Error? widthError))
        {
            return Result.Failure<ImportProductRowCommand>(widthError!);
        }

        if (!TryParseOptionalDecimal(GetValue(values, 18), rowNumber, "LargoCm", out decimal? lengthCm, out Error? lengthError))
        {
            return Result.Failure<ImportProductRowCommand>(lengthError!);
        }

        return Result.Success(new ImportProductRowCommand
        {
            RowNumber = rowNumber,
            Name = GetValue(values, 0),
            Description = GetValue(values, 1),
            Sku = GetValue(values, 2),
            Price = price,
            Currency = GetValue(values, 4),
            Stock = stock,
            IsActive = isActive,
            ProductType = productType,
            Slug = GetValue(values, 8),
            CategoryName = GetValue(values, 9),
            SubcategoryName = Normalize(GetValue(values, 10)),
            SerializedTags = Normalize(GetValue(values, 11)),
            FileFormat = Normalize(GetValue(values, 12)),
            FileSizeMb = fileSizeMb,
            RequiresLicense = requiresLicense,
            WeightKg = weightKg,
            HeightCm = heightCm,
            WidthCm = widthCm,
            LengthCm = lengthCm,
            RequiresShipping = requiresShipping
        });
    }

    private static string GetValue(IReadOnlyList<string> values, int index)
        => index < values.Count ? values[index].Trim() : string.Empty;

    private static string[] ExtractRowValues(WorkbookPart workbookPart, Row row, int expectedColumnCount)
    {
        string[] values = Enumerable.Repeat(string.Empty, expectedColumnCount).ToArray();
        Cell[] cells = row.Elements<Cell>().ToArray();
        bool hasCellReferences = cells.Any(cell => !string.IsNullOrWhiteSpace(cell.CellReference?.Value));

        if (!hasCellReferences)
        {
            for (int index = 0; index < Math.Min(cells.Length, expectedColumnCount); index++)
            {
                values[index] = GetCellValue(workbookPart, cells[index]);
            }

            return values;
        }

        foreach (Cell cell in cells)
        {
            int columnIndex = GetColumnIndex(cell.CellReference?.Value);
            if (columnIndex < 0 || columnIndex >= expectedColumnCount)
            {
                continue;
            }

            values[columnIndex] = GetCellValue(workbookPart, cell);
        }

        return values;
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

    private static bool TryParseInteger(string rawValue, int rowNumber, string columnName, out int parsedValue, out Error? error)
    {
        if (int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsedValue))
        {
            error = null;
            return true;
        }

        error = Error.Validation("Products.ImportNumberInvalid", $"La fila {rowNumber} contiene un valor inválido en '{columnName}'.");
        return false;
    }

    private static bool TryParseDecimal(string rawValue, int rowNumber, string columnName, out decimal parsedValue, out Error? error)
    {
        if (decimal.TryParse(rawValue, NumberStyles.Number, CultureInfo.InvariantCulture, out parsedValue)
            || decimal.TryParse(rawValue, NumberStyles.Number, new CultureInfo("es-CO"), out parsedValue))
        {
            error = null;
            return true;
        }

        error = Error.Validation("Products.ImportDecimalInvalid", $"La fila {rowNumber} contiene un valor inválido en '{columnName}'.");
        return false;
    }

    private static bool TryParseOptionalDecimal(string rawValue, int rowNumber, string columnName, out decimal? parsedValue, out Error? error)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            parsedValue = null;
            error = null;
            return true;
        }

        if (TryParseDecimal(rawValue, rowNumber, columnName, out decimal value, out error))
        {
            parsedValue = value;
            return true;
        }

        parsedValue = null;
        return false;
    }

    private static bool TryParseOptionalBoolean(string rawValue, int rowNumber, string columnName, out bool? parsedValue, out Error? error)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            parsedValue = null;
            error = null;
            return true;
        }

        if (TryParseBoolean(rawValue, out bool value))
        {
            parsedValue = value;
            error = null;
            return true;
        }

        parsedValue = null;
        error = Error.Validation("Products.ImportBooleanInvalid", $"La fila {rowNumber} contiene un valor inválido en '{columnName}'.");
        return false;
    }

    private static bool TryParseBoolean(string rawValue, out bool parsedValue)
    {
        string normalized = Normalize(rawValue)?.ToLowerInvariant() ?? string.Empty;
        switch (normalized)
        {
            case "true":
            case "verdadero":
            case "1":
            case "si":
            case "sí":
            case "yes":
                parsedValue = true;
                return true;
            case "false":
            case "falso":
            case "0":
            case "no":
                parsedValue = false;
                return true;
            default:
                parsedValue = default;
                return false;
        }
    }

    private static bool TryParseProductType(string rawValue, out TipoProducto productType)
    {
        string normalized = Normalize(rawValue)?.ToLowerInvariant() ?? string.Empty;
        if (normalized == "digital")
        {
            productType = TipoProducto.Digital;
            return true;
        }

        if (normalized == "fisico" || normalized == "físico")
        {
            productType = TipoProducto.Fisico;
            return true;
        }

        productType = default;
        return false;
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
