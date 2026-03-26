using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Categories.Commands;
using PlataformaECommerce.Application.Features.Categories.DTOs;
using PlataformaECommerce.Application.Features.Categories.Queries;
using PlataformaECommerce.Application.Features.Products.Commands;
using PlataformaECommerce.Application.Features.Products.DTOs;
using PlataformaECommerce.Application.Features.Products.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Categories;
using PlataformaECommerce.Application.Interfaces.Services.Products;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Web.Pages.Admin.Products;

namespace PlataformaECommerce.Tests.Web.Admin.Products;

[TestFixture]
public class AdminProductsIndexPageModelTests
{
    [Test]
    public async Task OnGetAsync_FiltrosValidos_CargaProductosDelCatalogo()
    {
        FakeProductApplicationService service = new();
        IndexModel pageModel = CreatePageModel(service);
        pageModel.SearchTerm = "teclado";
        pageModel.PageSize = 10;

        await pageModel.OnGetAsync(CancellationToken.None);

        Assert.That(pageModel.Products.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task OnPostActivateAsync_OperacionExitosa_RedireccionaAlListado()
    {
        FakeProductApplicationService service = new();
        IndexModel pageModel = CreatePageModel(service);

        IActionResult result = await pageModel.OnPostActivateAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(result, Is.InstanceOf<RedirectToPageResult>());
    }

    [Test]
    public async Task OnPostRemovePromotionAsync_OperacionExitosa_RedireccionaAlListado()
    {
        FakeProductApplicationService service = new();
        IndexModel pageModel = CreatePageModel(service);

        IActionResult result = await pageModel.OnPostRemovePromotionAsync(Guid.NewGuid(), "Fin de campaña", CancellationToken.None);

        Assert.That(result, Is.InstanceOf<RedirectToPageResult>());
    }

    [Test]
    public async Task OnPostUpdateStockAsync_OperacionExitosa_RedireccionaAlListado()
    {
        FakeProductApplicationService service = new();
        IndexModel pageModel = CreatePageModel(service);

        IActionResult result = await pageModel.OnPostUpdateStockAsync(Guid.NewGuid(), StockUpdateType.Increase, 5, "Ajuste manual", CancellationToken.None);

        Assert.That(result, Is.InstanceOf<RedirectToPageResult>());
    }

    [Test]
    public async Task OnPostApplyPromotionAsync_OperacionExitosa_RedireccionaAlListado()
    {
        FakeProductApplicationService service = new();
        IndexModel pageModel = CreatePageModel(service);

        IActionResult result = await pageModel.OnPostApplyPromotionAsync(Guid.NewGuid(), 10m, "Campaña", CancellationToken.None);

        Assert.That(result, Is.InstanceOf<RedirectToPageResult>());
    }

    [Test]
    public async Task OnGetDownloadImportTemplateAsync_CategoriasDisponibles_RetornaPlantillaExcel()
    {
        FakeProductApplicationService productService = new();
        FakeCategoryApplicationService categoryService = new();
        IndexModel pageModel = CreatePageModel(productService, categoryService);

        IActionResult result = await pageModel.OnGetDownloadImportTemplateAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<FileContentResult>());
        FileContentResult fileResult = (FileContentResult)result;
        Assert.That(fileResult.ContentType, Is.EqualTo("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
        Assert.That(fileResult.FileDownloadName, Is.EqualTo("plantilla-productos.xlsx"));

        using MemoryStream stream = new(fileResult.FileContents);
        using SpreadsheetDocument document = SpreadsheetDocument.Open(stream, false);
        Sheet[] sheets = document.WorkbookPart!.Workbook.Sheets!.Elements<Sheet>().ToArray();

        Assert.That(sheets.Select(sheet => sheet.Name!.Value), Is.EqualTo(new[] { "Instrucciones", "Productos", "Catalogos" }));
        Assert.That(sheets[0].State?.Value, Is.Not.EqualTo(SheetStateValues.Hidden));
        Assert.That(sheets[1].State?.Value, Is.Not.EqualTo(SheetStateValues.Hidden));
        Assert.That(sheets[2].State?.Value, Is.EqualTo(SheetStateValues.Hidden));
    }

    [Test]
    public async Task OnPostImportAsync_ArchivoExcelValido_InvocaImportacionMasiva()
    {
        FakeProductApplicationService productService = new();
        productService.ImportResult = Result.Success(new ProductImportResultDto
        {
            PhysicalProductsCreated = 1,
            DigitalProductsCreated = 1
        });
        FakeCategoryApplicationService categoryService = new();
        IndexModel pageModel = CreatePageModel(productService, categoryService);
        pageModel.ImportInput = new IndexModel.ImportInputModel
        {
            ImportFile = CreateExcelImportFile()
        };

        IActionResult result = await pageModel.OnPostImportAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<RedirectToPageResult>());
        Assert.That(productService.LastImportCommand?.Rows.Count, Is.GreaterThan(0));
        Assert.That(pageModel.SuccessMessage, Is.EqualTo("Importación completada correctamente. Productos físicos creados: 1. Productos digitales creados: 1."));
    }

    private static IndexModel CreatePageModel(IProductApplicationService service, ICategoryApplicationService? categoryService = null)
    {
        IndexModel pageModel = new(service, categoryService ?? new FakeCategoryApplicationService());
        DefaultHttpContext httpContext = new();
        pageModel.PageContext = new PageContext { HttpContext = httpContext };
        pageModel.TempData = new TempDataDictionary(httpContext, new FakeTempDataProvider());
        return pageModel;
    }

    private static IFormFile CreateImportFile(string fileName, string contentType, byte[] bytes)
    {
        MemoryStream stream = new(bytes);
        return new FormFile(stream, 0, bytes.Length, "ImportInput.ImportFile", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    private static IFormFile CreateExcelImportFile()
    {
        using MemoryStream stream = new();
        using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook, true))
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
                Name = "Productos"
            });

            sheetData.Append(CreateRow([
                "Nombre", "Descripcion", "SKU", "Precio", "Moneda", "Stock", "Activo", "TipoProducto", "Slug", "Categoria", "Subcategoria", "EtiquetasSerializadas", "FormatoArchivo", "TamanoMB", "RequiereLicencia", "PesoKg", "AltoCm", "AnchoCm", "LargoCm", "RequiereEnvio"
            ]));
            sheetData.Append(CreateRow([
                "Mouse Gamer", "Mouse de precision.", "MOUSE-IMPORT-001", "100", "COP", "5", "true", "Fisico", "mouse-gamer", "Tecnologia", "Laptops", "gaming,precision", string.Empty, string.Empty, string.Empty, "0.4", "4", "6", "11", "true"
            ]));

            workbookPart.Workbook.Save();
        }

        return CreateImportFile("productos.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", stream.ToArray());
    }

    private static Row CreateRow(IReadOnlyList<string> values)
    {
        Row row = new();
        foreach (string value in values)
        {
            row.Append(new Cell
            {
                DataType = CellValues.InlineString,
                InlineString = new InlineString(new Text(value))
            });
        }

        return row;
    }

    private sealed class FakeProductApplicationService : IProductApplicationService
    {
        public Result<ProductImportResultDto> ImportResult { get; set; } = Result.Success(new ProductImportResultDto());
        public ImportProductsCommand? LastImportCommand { get; private set; }

        public Task<Result<Guid>> CreatePhysicalProductAsync(CreatePhysicalProductCommand command, CancellationToken cancellationToken = default) => Task.FromResult(Result.Success(Guid.NewGuid()));
        public Task<Result<Guid>> CreateDigitalProductAsync(CreateDigitalProductCommand command, CancellationToken cancellationToken = default) => Task.FromResult(Result.Success(Guid.NewGuid()));
        public Task<Result<ProductImportResultDto>> ImportProductsAsync(ImportProductsCommand command, CancellationToken cancellationToken = default)
        {
            LastImportCommand = command;
            return Task.FromResult(ImportResult);
        }
        public Task<Result<ProductResponseDto>> UpdateProductAsync(UpdateProductCommand command, CancellationToken cancellationToken = default) => Task.FromResult(Result.Success(new ProductResponseDto()));
        public Task<Result<ProductResponseDto>> UpdateProductStockAsync(UpdateProductStockCommand command, CancellationToken cancellationToken = default) => Task.FromResult(Result.Success(new ProductResponseDto()));
        public Task<Result<ProductResponseDto>> ActivateProductAsync(ActivateProductCommand command, CancellationToken cancellationToken = default) => Task.FromResult(Result.Success(new ProductResponseDto()));
        public Task<Result<ProductResponseDto>> DeactivateProductAsync(DeactivateProductCommand command, CancellationToken cancellationToken = default) => Task.FromResult(Result.Success(new ProductResponseDto()));
        public Task<Result<ProductResponseDto>> ApplyProductPromotionAsync(ApplyProductPromotionCommand command, CancellationToken cancellationToken = default) => Task.FromResult(Result.Success(new ProductResponseDto()));
        public Task<Result<ProductResponseDto>> RemoveProductPromotionAsync(RemoveProductPromotionCommand command, CancellationToken cancellationToken = default) => Task.FromResult(Result.Success(new ProductResponseDto { Price = 100m, Currency = "COP" }));
        public Task<Result<ProductResponseDto>> FeatureProductAsync(FeatureProductCommand command, CancellationToken cancellationToken = default) => Task.FromResult(Result.Success(new ProductResponseDto()));
        public Task<Result<ProductResponseDto>> UnfeatureProductAsync(UnfeatureProductCommand command, CancellationToken cancellationToken = default) => Task.FromResult(Result.Success(new ProductResponseDto()));
        public Task<Result<ProductDetailDto>> GetProductByIdAsync(GetProductByIdQuery query, CancellationToken cancellationToken = default) => Task.FromResult(Result.Success(new ProductDetailDto()));

        public Task<Result<ProductQueryResultDto>> GetProductsAsync(GetProductsQuery query, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result.Success(new ProductQueryResultDto
            {
                Items =
                [
                    new ProductDto
                    {
                        Id = Guid.NewGuid(),
                        Name = "Teclado mecánico",
                        Description = "Producto de prueba.",
                        Sku = "PROD-001",
                        Price = 199900m,
                        Currency = "COP",
                        Stock = 12,
                        IsActive = true,
                        IsFeatured = true,
                        Slug = "teclado-mecanico",
                        ProductType = TipoProducto.Fisico,
                        CreatedAtUtc = DateTime.UtcNow
                    }
                ],
                TotalCount = 1,
                ReturnedCount = 1,
                PageNumber = query.NormalizedPageNumber,
                PageSize = query.NormalizedPageSize,
                TotalPages = 1,
                HasPreviousPage = false,
                HasNextPage = false
            }));
        }
    }

    private sealed class FakeCategoryApplicationService : ICategoryApplicationService
    {
        public Task<Result<IReadOnlyCollection<CategoryDto>>> GetCategoriesAsync(GetCategoriesQuery query, CancellationToken cancellationToken = default)
        {
            Guid rootCategoryId = Guid.NewGuid();
            IReadOnlyCollection<CategoryDto> categories =
            [
                new CategoryDto { Id = rootCategoryId, Name = "Tecnologia", IsActive = true, IsRootCategory = true },
                new CategoryDto { Id = Guid.NewGuid(), Name = "Laptops", ParentCategoryId = rootCategoryId, IsActive = true, IsRootCategory = false },
                new CategoryDto { Id = Guid.NewGuid(), Name = "Monitores", ParentCategoryId = rootCategoryId, IsActive = true, IsRootCategory = false }
            ];

            return Task.FromResult(Result.Success(categories));
        }

        public Task<Result<CategoryDto>> GetCategoryByIdAsync(GetCategoryByIdQuery query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<Guid>> CreateCategoryAsync(CreateCategoryCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<CategoryImportResultDto>> ImportCategoriesFromXmlAsync(ImportCategoriesFromXmlCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<CategoryDto>> UpdateCategoryAsync(UpdateCategoryCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<CategoryDto>> ChangeCategoryStatusAsync(ChangeCategoryStatusCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class FakeTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
