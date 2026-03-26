using System.Globalization;
using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using PlataformaECommerce.Application.Features.Categories.DTOs;

namespace PlataformaECommerce.Web.Services.Products;

/// <summary>
/// Construye la plantilla Excel profesional utilizada para la importación masiva de productos.
/// </summary>
internal static class ProductExcelTemplateProvider
{
    private const string InstructionsSheetName = "Instrucciones";
    private const string WorkbookSheetName = "Productos";
    private const string CatalogSheetName = "Catalogos";
    private const string ProductTypesRangeName = "ProductTypes";
    private const string CategoryNamesRangeName = "ProductCategories";
    private const string BooleanValuesRangeName = "BooleanValues";
    private const string EmptySubcategoriesRangeName = "ProductSubcategoriesEmpty";
    private const uint HelperColumnIndex = 21;

    /// <summary>
    /// Nombre sugerido para el archivo Excel de productos.
    /// </summary>
    internal const string FileName = "plantilla-productos.xlsx";

    /// <summary>
    /// Tipo de contenido HTTP para la descarga de la plantilla Excel.
    /// </summary>
    internal const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    /// <summary>
    /// Construye la plantilla Excel en memoria usando categorías activas existentes en base de datos.
    /// </summary>
    internal static byte[] BuildTemplateBytes(IReadOnlyCollection<CategoryDto> categories)
    {
        ArgumentNullException.ThrowIfNull(categories);

        CategoryDto[] mainCategories = categories
            .Where(category => category.IsRootCategory && category.IsActive)
            .OrderBy(category => category.Name)
            .ToArray();
        Dictionary<Guid, CategoryDto[]> subcategoriesByParent = categories
            .Where(category => category.IsSubcategory && category.IsActive && category.ParentCategoryId.HasValue)
            .GroupBy(category => category.ParentCategoryId!.Value)
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(category => category.Name).ToArray());

        using MemoryStream stream = new();
        using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook, true))
        {
            WorkbookPart workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            WorksheetPart instructionsWorksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            instructionsWorksheetPart.Worksheet = CreateInstructionsWorksheet();

            WorksheetPart productsWorksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            productsWorksheetPart.Worksheet = CreateProductsWorksheet();

            WorksheetPart catalogWorksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            catalogWorksheetPart.Worksheet = CreateCatalogWorksheet(mainCategories, subcategoriesByParent, out int categoryLookupLastRow);

            Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
            sheets.Append(new Sheet
            {
                Id = workbookPart.GetIdOfPart(instructionsWorksheetPart),
                SheetId = 1,
                Name = InstructionsSheetName
            });
            sheets.Append(new Sheet
            {
                Id = workbookPart.GetIdOfPart(productsWorksheetPart),
                SheetId = 2,
                Name = WorkbookSheetName
            });
            sheets.Append(new Sheet
            {
                Id = workbookPart.GetIdOfPart(catalogWorksheetPart),
                SheetId = 3,
                Name = CatalogSheetName,
                State = SheetStateValues.Hidden
            });

            DefineWorkbookRanges(workbookPart, mainCategories, subcategoriesByParent, categoryLookupLastRow);
            workbookPart.Workbook.Save();
        }

        return stream.ToArray();
    }

    private static Worksheet CreateInstructionsWorksheet()
    {
        Columns columns = new(
            new Column { Min = 1, Max = 1, Width = 28, CustomWidth = true },
            new Column { Min = 2, Max = 2, Width = 95, CustomWidth = true });

        SheetData sheetData = new();
        sheetData.Append(CreateRow(["Seccion", "Detalle"]));
        sheetData.Append(CreateRow(["Objetivo", "Use la hoja Productos para registrar productos fisicos y digitales mediante carga masiva desde el backoffice."]));
        sheetData.Append(CreateRow(["Columnas base", "Complete Nombre, Descripcion, SKU, Precio, Moneda, Stock, Activo, TipoProducto, Slug, Categoria, Subcategoria y EtiquetasSerializadas."]));
        sheetData.Append(CreateRow(["TipoProducto", "Use la lista desplegable y seleccione solo Digital o Fisico."]));
        sheetData.Append(CreateRow(["Categoria", "Seleccione una categoria principal activa existente en la base de datos."]));
        sheetData.Append(CreateRow(["Subcategoria", "Seleccione una subcategoria valida para la categoria elegida. Puede dejarla vacia si el producto no requiere subcategoria."]));
        sheetData.Append(CreateRow(["Etiquetas", "La columna EtiquetasSerializadas admite valores separados por coma, punto y coma o barra vertical. Ejemplo: gaming, premium;inalambrico"]));
        sheetData.Append(CreateRow(["Productos digitales", "Si TipoProducto es Digital, diligencie FormatoArchivo, TamanoMB y RequiereLicencia."]));
        sheetData.Append(CreateRow(["Productos fisicos", "Si TipoProducto es Fisico, diligencie PesoKg, AltoCm, AnchoCm, LargoCm y RequiereEnvio."]));
        sheetData.Append(CreateRow(["Booleanos", "Las columnas Activo, RequiereLicencia y RequiereEnvio usan listas con true o false."]));
        sheetData.Append(CreateRow(["Resolucion interna", "La aplicacion resuelve Categoria y Subcategoria por nombre y guarda sus identificadores reales en base de datos."]));
        sheetData.Append(CreateRow(["Atomicidad", "La importacion es transaccional. Si una fila falla, no se guarda ningun producto parcial."]));
        sheetData.Append(CreateRow(["Ejemplo fisico", "Mouse Gamer | MOUSE-IMPORT-001 | Fisico | Tecnologia | Laptops | PesoKg=0.4 | AltoCm=4 | AnchoCm=6 | LargoCm=11 | RequiereEnvio=true"]));
        sheetData.Append(CreateRow(["Ejemplo digital", "Curso .NET 10 | DIGI-IMPORT-001 | Digital | Tecnologia | sin subcategoria | FormatoArchivo=PDF | TamanoMB=25 | RequiereLicencia=false"]));

        return new Worksheet(columns, sheetData);
    }

    private static Worksheet CreateProductsWorksheet()
    {
        Columns columns = new();
        for (uint index = 1; index <= ProductImportTemplateData.Headers.Count; index++)
        {
            columns.Append(new Column
            {
                Min = index,
                Max = index,
                Width = index switch
                {
                    1 => 26,
                    2 => 40,
                    3 => 18,
                    4 => 14,
                    5 => 12,
                    6 => 10,
                    7 => 12,
                    8 => 16,
                    9 => 24,
                    10 => 22,
                    11 => 22,
                    12 => 28,
                    _ => 16
                },
                CustomWidth = true
            });
        }

        columns.Append(new Column
        {
            Min = HelperColumnIndex,
            Max = HelperColumnIndex,
            Hidden = true,
            Width = 2,
            CustomWidth = true
        });

        SheetData sheetData = new();
        sheetData.Append(CreateRow(ProductImportTemplateData.Headers));

        for (int rowNumber = 2; rowNumber <= ProductImportTemplateData.TemplateRowCount + 1; rowNumber++)
        {
            Row row = new() { RowIndex = (uint)rowNumber };
            for (int column = 0; column < ProductImportTemplateData.Headers.Count; column++)
            {
                row.Append(CreateInlineStringCell(string.Empty));
            }

            string helperFormula = $"IFERROR(VLOOKUP(J{rowNumber},Catalogos!$F$2:$G$500,2,FALSE),\"{EmptySubcategoriesRangeName}\")";
            row.Append(new Cell
            {
                CellFormula = new CellFormula(helperFormula),
                DataType = CellValues.String,
                CellValue = new CellValue(EmptySubcategoriesRangeName)
            });
            sheetData.Append(row);
        }

        DataValidations validations = CreateProductValidations();
        return new Worksheet(columns, sheetData, validations);
    }

    private static Worksheet CreateCatalogWorksheet(
        IReadOnlyCollection<CategoryDto> mainCategories,
        IReadOnlyDictionary<Guid, CategoryDto[]> subcategoriesByParent,
        out int categoryLookupLastRow)
    {
        SheetData sheetData = new();
        HashSet<string> usedRangeKeys = new(StringComparer.OrdinalIgnoreCase);
        List<(string CategoryName, string NamedRangeKey, CategoryDto[] Subcategories)> categoryDefinitions = mainCategories
            .Select(mainCategory =>
            {
                CategoryDto[] subcategories = subcategoriesByParent.TryGetValue(mainCategory.Id, out CategoryDto[]? values)
                    ? values
                    : Array.Empty<CategoryDto>();

                string namedRangeKey = subcategories.Length > 0
                    ? BuildNamedRangeKey(mainCategory.Name, usedRangeKeys)
                    : EmptySubcategoriesRangeName;

                return (mainCategory.Name, namedRangeKey, subcategories);
            })
            .ToList();

        int subcategoryColumns = categoryDefinitions.Count(definition => definition.Subcategories.Length > 0);
        int totalColumns = Math.Max(8 + subcategoryColumns, 7);
        int maxSubcategoryRows = categoryDefinitions.Count == 0
            ? 0
            : categoryDefinitions.Max(definition => definition.Subcategories.Length);
        int dataRows = Math.Max(
            Math.Max(ProductImportTemplateData.ProductTypes.Count, ProductImportTemplateData.BooleanValues.Count),
            Math.Max(mainCategories.Count, maxSubcategoryRows));

        string[] headerValues = Enumerable.Repeat(string.Empty, totalColumns).ToArray();
        headerValues[0] = "TiposProducto";
        headerValues[1] = "Categorias";
        headerValues[2] = "Booleanos";
        headerValues[3] = "SubcategoriasVacias";
        headerValues[5] = "CategoriaLookup";
        headerValues[6] = "NamedRangeKey";

        int dynamicColumnIndex = 8;
        foreach (var definition in categoryDefinitions.Where(definition => definition.Subcategories.Length > 0))
        {
            headerValues[dynamicColumnIndex] = definition.CategoryName;
            dynamicColumnIndex++;
        }

        sheetData.Append(CreateRow(headerValues));

        for (int rowIndex = 0; rowIndex < dataRows; rowIndex++)
        {
            string[] values = Enumerable.Repeat(string.Empty, totalColumns).ToArray();
            if (rowIndex < ProductImportTemplateData.ProductTypes.Count)
            {
                values[0] = ProductImportTemplateData.ProductTypes[rowIndex];
            }

            if (rowIndex < mainCategories.Count)
            {
                values[1] = mainCategories.ElementAt(rowIndex).Name;
                values[5] = categoryDefinitions[rowIndex].CategoryName;
                values[6] = categoryDefinitions[rowIndex].NamedRangeKey;
            }

            if (rowIndex < ProductImportTemplateData.BooleanValues.Count)
            {
                values[2] = ProductImportTemplateData.BooleanValues[rowIndex];
            }

            dynamicColumnIndex = 8;
            foreach (var definition in categoryDefinitions.Where(definition => definition.Subcategories.Length > 0))
            {
                if (rowIndex < definition.Subcategories.Length)
                {
                    values[dynamicColumnIndex] = definition.Subcategories[rowIndex].Name;
                }

                dynamicColumnIndex++;
            }

            sheetData.Append(CreateRow(values));
        }

        categoryLookupLastRow = Math.Max(mainCategories.Count + 1, 2);
        return new Worksheet(sheetData);
    }

    private static void DefineWorkbookRanges(
        WorkbookPart workbookPart,
        IReadOnlyCollection<CategoryDto> mainCategories,
        IReadOnlyDictionary<Guid, CategoryDto[]> subcategoriesByParent,
        int categoryLookupLastRow)
    {
        DefinedNames definedNames = workbookPart.Workbook.DefinedNames ?? workbookPart.Workbook.AppendChild(new DefinedNames());
        definedNames.RemoveAllChildren<DefinedName>();

        definedNames.Append(new DefinedName { Name = ProductTypesRangeName, Text = $"{CatalogSheetName}!$A$2:$A${Math.Max(ProductImportTemplateData.ProductTypes.Count + 1, 2)}" });
        definedNames.Append(new DefinedName { Name = CategoryNamesRangeName, Text = $"{CatalogSheetName}!$B$2:$B${Math.Max(mainCategories.Count + 1, 2)}" });
        definedNames.Append(new DefinedName { Name = BooleanValuesRangeName, Text = $"{CatalogSheetName}!$C$2:$C${Math.Max(ProductImportTemplateData.BooleanValues.Count + 1, 2)}" });
        definedNames.Append(new DefinedName { Name = EmptySubcategoriesRangeName, Text = $"{CatalogSheetName}!$D$2:$D$2" });

        HashSet<string> usedRangeKeys = new(StringComparer.OrdinalIgnoreCase);
        int subcategoryColumnIndex = 9;
        foreach (CategoryDto mainCategory in mainCategories)
        {
            if (!subcategoriesByParent.TryGetValue(mainCategory.Id, out CategoryDto[]? subcategories) || subcategories.Length == 0)
            {
                continue;
            }

            string namedRangeKey = BuildNamedRangeKey(mainCategory.Name, usedRangeKeys);
            string columnLetter = GetColumnLetter(subcategoryColumnIndex);
            definedNames.Append(new DefinedName
            {
                Name = namedRangeKey,
                Text = $"{CatalogSheetName}!${columnLetter}$2:${columnLetter}${subcategories.Length + 1}"
            });
            subcategoryColumnIndex++;
        }
    }

    private static DataValidations CreateProductValidations()
    {
        DataValidation productTypeValidation = CreateListValidation($"H2:H{ProductImportTemplateData.TemplateRowCount + 1}", $"={ProductTypesRangeName}");
        DataValidation categoryValidation = CreateListValidation($"J2:J{ProductImportTemplateData.TemplateRowCount + 1}", $"={CategoryNamesRangeName}");
        DataValidation subcategoryValidation = CreateListValidation($"K2:K{ProductImportTemplateData.TemplateRowCount + 1}", "=INDIRECT($U2)");
        DataValidation activeValidation = CreateListValidation($"G2:G{ProductImportTemplateData.TemplateRowCount + 1}", $"={BooleanValuesRangeName}");
        DataValidation requiresLicenseValidation = CreateListValidation($"O2:O{ProductImportTemplateData.TemplateRowCount + 1}", $"={BooleanValuesRangeName}");
        DataValidation requiresShippingValidation = CreateListValidation($"T2:T{ProductImportTemplateData.TemplateRowCount + 1}", $"={BooleanValuesRangeName}");

        return new DataValidations(productTypeValidation, categoryValidation, subcategoryValidation, activeValidation, requiresLicenseValidation, requiresShippingValidation)
        {
            Count = 6U
        };
    }

    private static DataValidation CreateListValidation(string sequenceOfReferences, string formula)
    {
        return new DataValidation
        {
            Type = DataValidationValues.List,
            AllowBlank = true,
            ShowErrorMessage = true,
            SequenceOfReferences = new ListValue<StringValue> { InnerText = sequenceOfReferences },
            Formula1 = new Formula1(formula)
        };
    }

    private static Row CreateRow(IReadOnlyList<string> values)
    {
        Row row = new();
        foreach (string value in values)
        {
            row.Append(CreateInlineStringCell(value));
        }

        return row;
    }

    private static Cell CreateInlineStringCell(string value)
    {
        return new Cell
        {
            DataType = CellValues.InlineString,
            InlineString = new InlineString(new Text(value ?? string.Empty))
        };
    }

    private static string BuildNamedRangeKey(string categoryName, ISet<string> usedKeys)
    {
        string normalized = RemoveDiacritics(categoryName).ToLowerInvariant();
        StringBuilder builder = new("sub_");
        foreach (char character in normalized)
        {
            builder.Append(char.IsLetterOrDigit(character) ? character : '_');
        }

        string baseKey = builder.ToString().TrimEnd('_');
        if (string.IsNullOrWhiteSpace(baseKey) || string.Equals(baseKey, "sub_", StringComparison.Ordinal))
        {
            baseKey = "sub_categoria";
        }

        string candidate = baseKey;
        int suffix = 2;
        while (!usedKeys.Add(candidate))
        {
            candidate = $"{baseKey}_{suffix}";
            suffix++;
        }

        return candidate;
    }

    private static string RemoveDiacritics(string value)
    {
        string normalized = value.Normalize(NormalizationForm.FormD);
        StringBuilder builder = new();
        foreach (char character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    private static string GetColumnLetter(int columnNumber)
    {
        StringBuilder builder = new();
        while (columnNumber > 0)
        {
            int modulo = (columnNumber - 1) % 26;
            builder.Insert(0, (char)('A' + modulo));
            columnNumber = (columnNumber - modulo) / 26;
        }

        return builder.ToString();
    }
}
