using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Categories.Commands;
using PlataformaECommerce.Application.Features.Categories.DTOs;
using PlataformaECommerce.Application.Features.Categories.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Categories;
using CreateCategoryPageModel = PlataformaECommerce.Web.Pages.Admin.Categories.CreateModel;
using EditCategoryPageModel = PlataformaECommerce.Web.Pages.Admin.Categories.EditModel;
using IndexCategoryPageModel = PlataformaECommerce.Web.Pages.Admin.Categories.IndexModel;

namespace PlataformaECommerce.Tests.Web.Admin.Categories;

[TestFixture]
public class AdminCategoriesPageModelTests
{
    [Test]
    public async Task CreateOnGetAsync_PadreValido_PreseleccionaFlujoDeSubcategoria()
    {
        Guid parentCategoryId = Guid.NewGuid();
        FakeCategoryApplicationService service = new();
        service.Categories =
        [
            new CategoryDto { Id = parentCategoryId, Name = "Tecnología", IsActive = true, IsRootCategory = true }
        ];

        CreateCategoryPageModel pageModel = CreateCreatePageModel(service);

        await pageModel.OnGetAsync(parentCategoryId, CancellationToken.None);

        Assert.That(pageModel.Input.ParentCategoryId, Is.EqualTo(parentCategoryId));
        Assert.That(pageModel.IsCreatingSubcategory, Is.True);
        Assert.That(pageModel.SelectedParentCategoryName, Is.EqualTo("Tecnología"));
    }

    [Test]
    public async Task CreateOnPostAsync_SubcategoriaValida_EnviaPadreYRedireccionaAlListado()
    {
        Guid parentCategoryId = Guid.NewGuid();
        FakeCategoryApplicationService service = new();
        CreateCategoryPageModel pageModel = CreateCreatePageModel(service);
        pageModel.Input = new CreateCategoryPageModel.InputModel
        {
            Name = "Laptops",
            Slug = "laptops",
            Description = "Computadores portátiles.",
            ParentCategoryId = parentCategoryId,
            IsActive = true
        };

        IActionResult result = await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<RedirectToPageResult>());
        RedirectToPageResult redirectResult = (RedirectToPageResult)result;
        Assert.That(redirectResult.PageName, Is.EqualTo("./Index"));
        Assert.That(service.LastCreateCommand?.ParentCategoryId, Is.EqualTo(parentCategoryId));
        Assert.That(service.LastCreateCommand?.Name, Is.EqualTo("Laptops"));
        Assert.That(pageModel.SuccessMessage, Is.EqualTo("Categoría registrada correctamente."));
    }

    [Test]
    public async Task EditOnGetAsync_CategoriaRaiz_HabilitaAltaDeSubcategoriaHija()
    {
        Guid categoryId = Guid.NewGuid();
        FakeCategoryApplicationService service = new();
        service.CategoryByIdResult = Result.Success(new CategoryDto
        {
            Id = categoryId,
            Name = "Tecnología",
            Slug = "tecnologia",
            IsActive = true,
            IsRootCategory = true
        });
        service.Categories =
        [
            new CategoryDto { Id = Guid.NewGuid(), Name = "Hogar", IsActive = true, IsRootCategory = true }
        ];

        EditCategoryPageModel pageModel = CreateEditPageModel(service);

        IActionResult result = await pageModel.OnGetAsync(categoryId, CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(pageModel.IsRootCategory, Is.True);
        Assert.That(pageModel.CanCreateSubcategory, Is.True);
    }

    [Test]
    public async Task EditOnGetAsync_Subcategoria_CargaNombreDelPadreYDeshabilitaAltaHija()
    {
        Guid categoryId = Guid.NewGuid();
        Guid parentCategoryId = Guid.NewGuid();
        FakeCategoryApplicationService service = new();
        service.CategoryByIdResult = Result.Success(new CategoryDto
        {
            Id = categoryId,
            Name = "Laptops",
            Slug = "laptops",
            ParentCategoryId = parentCategoryId,
            IsActive = true,
            IsRootCategory = false
        });
        service.Categories =
        [
            new CategoryDto { Id = parentCategoryId, Name = "Tecnología", IsActive = true, IsRootCategory = true },
            new CategoryDto { Id = Guid.NewGuid(), Name = "Hogar", IsActive = true, IsRootCategory = true }
        ];

        EditCategoryPageModel pageModel = CreateEditPageModel(service);

        IActionResult result = await pageModel.OnGetAsync(categoryId, CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(pageModel.IsRootCategory, Is.False);
        Assert.That(pageModel.CanCreateSubcategory, Is.False);
        Assert.That(pageModel.ParentCategoryName, Is.EqualTo("Tecnología"));
    }

    [Test]
    public async Task IndexOnPostDeactivateAsync_FalloFuncional_PublicaMensajeDeEstado()
    {
        FakeCategoryApplicationService service = new
        FakeCategoryApplicationService
        {
            ChangeStatusResult = Result.Failure<CategoryDto>(Error.Validation("Categories.HasActiveChildren", "No es posible desactivar una categoría que aún tiene subcategorías activas."))
        };
        IndexCategoryPageModel pageModel = CreateIndexPageModel(service);

        IActionResult result = await pageModel.OnPostDeactivateAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(result, Is.TypeOf<RedirectToPageResult>());
        Assert.That(pageModel.StatusErrorMessage, Is.EqualTo("No es posible desactivar una categoría que aún tiene subcategorías activas."));
    }

    [Test]
    public void IndexOnGetDownloadTemplate_RetornaArchivoXml()
    {
        FakeCategoryApplicationService service = new();
        IndexCategoryPageModel pageModel = CreateIndexPageModel(service);

        FileContentResult result = pageModel.OnGetDownloadTemplate();

        Assert.That(result.ContentType, Is.EqualTo("application/xml"));
        Assert.That(result.FileDownloadName, Is.EqualTo("plantilla-categorias.xml"));
    }

    [Test]
    public void IndexOnGetDownloadCsvTemplate_RetornaArchivoCsv()
    {
        FakeCategoryApplicationService service = new();
        IndexCategoryPageModel pageModel = CreateIndexPageModel(service);

        FileContentResult result = pageModel.OnGetDownloadCsvTemplate();

        Assert.That(result.ContentType, Is.EqualTo("text/csv"));
        Assert.That(result.FileDownloadName, Is.EqualTo("plantilla-categorias.csv"));
    }

    [Test]
    public void IndexOnGetDownloadExcelTemplate_RetornaArchivoExcel()
    {
        FakeCategoryApplicationService service = new();
        IndexCategoryPageModel pageModel = CreateIndexPageModel(service);

        FileContentResult result = pageModel.OnGetDownloadExcelTemplate();

        Assert.That(result.ContentType, Is.EqualTo("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
        Assert.That(result.FileDownloadName, Is.EqualTo("plantilla-categorias.xlsx"));
    }

    [Test]
    public async Task IndexOnPostImportAsync_ArchivoValido_InvocaImportacionYPublicaResumen()
    {
        FakeCategoryApplicationService service = new();
        service.ImportResult = Result.Success(new CategoryImportResultDto
        {
            RootCategoriesCreated = 2,
            SubcategoriesCreated = 3
        });
        IndexCategoryPageModel pageModel = CreateIndexPageModel(service);
        pageModel.ImportInput = new IndexCategoryPageModel.ImportInputModel
        {
            ImportFile = CreateImportFile("categorias.xml", "application/xml", "<Categories><Category><Name>Tecnologia</Name><Slug>tecnologia</Slug><Description>Categoria principal.</Description><IsActive>true</IsActive><ParentCategoryName></ParentCategoryName></Category></Categories>")
        };

        IActionResult result = await pageModel.OnPostImportAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<RedirectToPageResult>());
        Assert.That(service.LastImportCommand?.XmlContent, Does.Contain("<Categories>"));
        Assert.That(pageModel.SuccessMessage, Is.EqualTo("Importación completada correctamente. Categorías principales creadas: 2. Subcategorías creadas: 3."));
    }

    [Test]
    public async Task IndexOnPostImportAsync_ArchivoCsvValido_ConvierteAContratoXmlAntesDeImportar()
    {
        FakeCategoryApplicationService service = new();
        IndexCategoryPageModel pageModel = CreateIndexPageModel(service);
        pageModel.ImportInput = new IndexCategoryPageModel.ImportInputModel
        {
            ImportFile = CreateImportFile(
                "categorias.csv",
                "text/csv",
                "Name,Slug,Description,IsActive,ParentCategoryName\r\nTecnologia,tecnologia,Categoria principal,true,\r\nLaptops,laptops,Equipos portatiles,VERDADERO,Tecnologia")
        };

        IActionResult result = await pageModel.OnPostImportAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<RedirectToPageResult>());
        Assert.That(service.LastImportCommand?.XmlContent, Does.Contain("<ParentCategoryName>Tecnologia</ParentCategoryName>"));
    }

    [Test]
    public async Task IndexOnPostImportAsync_ArchivoExcelValido_ConvierteAContratoXmlAntesDeImportar()
    {
        FakeCategoryApplicationService service = new();
        IndexCategoryPageModel pageModel = CreateIndexPageModel(service);
        FileContentResult template = pageModel.OnGetDownloadExcelTemplate();
        pageModel.ImportInput = new IndexCategoryPageModel.ImportInputModel
        {
            ImportFile = CreateImportFile("categorias.xlsx", template.ContentType!, template.FileContents)
        };

        IActionResult result = await pageModel.OnPostImportAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<RedirectToPageResult>());
        Assert.That(service.LastImportCommand?.XmlContent, Does.Contain("<Name>Tecnologia</Name>"));
    }

    private static CreateCategoryPageModel CreateCreatePageModel(FakeCategoryApplicationService service)
    {
        DefaultHttpContext httpContext = new();
        return new CreateCategoryPageModel(service)
        {
            PageContext = new PageContext
            {
                HttpContext = httpContext
            },
            TempData = new TempDataDictionary(httpContext, new FakeTempDataProvider())
        };
    }

    private static IFormFile CreateImportFile(string fileName, string contentType, string content)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(content);
        return CreateImportFile(fileName, contentType, bytes);
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

    private static EditCategoryPageModel CreateEditPageModel(FakeCategoryApplicationService service)
    {
        DefaultHttpContext httpContext = new();
        return new EditCategoryPageModel(service)
        {
            PageContext = new PageContext
            {
                HttpContext = httpContext
            },
            TempData = new TempDataDictionary(httpContext, new FakeTempDataProvider())
        };
    }

    private static IndexCategoryPageModel CreateIndexPageModel(FakeCategoryApplicationService service)
    {
        DefaultHttpContext httpContext = new();
        return new IndexCategoryPageModel(service)
        {
            PageContext = new PageContext
            {
                HttpContext = httpContext
            },
            TempData = new TempDataDictionary(httpContext, new FakeTempDataProvider())
        };
    }

    private sealed class FakeCategoryApplicationService : ICategoryApplicationService
    {
        public IReadOnlyCollection<CategoryDto> Categories { get; set; } = Array.Empty<CategoryDto>();

        public Result<CategoryDto> CategoryByIdResult { get; set; } = Result.Success(new CategoryDto());

        public Result<Guid> CreateResult { get; set; } = Result.Success(Guid.NewGuid());

        public Result<CategoryDto> UpdateResult { get; set; } = Result.Success(new CategoryDto());

        public Result<CategoryDto> ChangeStatusResult { get; set; } = Result.Success(new CategoryDto());

        public Result<CategoryImportResultDto> ImportResult { get; set; } = Result.Success(new CategoryImportResultDto());

        public CreateCategoryCommand? LastCreateCommand { get; private set; }

        public UpdateCategoryCommand? LastUpdateCommand { get; private set; }

        public ChangeCategoryStatusCommand? LastChangeStatusCommand { get; private set; }

        public ImportCategoriesFromXmlCommand? LastImportCommand { get; private set; }

        public Task<Result<IReadOnlyCollection<CategoryDto>>> GetCategoriesAsync(GetCategoriesQuery query, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(Categories));

        public Task<Result<CategoryDto>> GetCategoryByIdAsync(GetCategoryByIdQuery query, CancellationToken cancellationToken = default)
            => Task.FromResult(CategoryByIdResult);

        public Task<Result<Guid>> CreateCategoryAsync(CreateCategoryCommand command, CancellationToken cancellationToken = default)
        {
            LastCreateCommand = command;
            return Task.FromResult(CreateResult);
        }

        public Task<Result<CategoryImportResultDto>> ImportCategoriesFromXmlAsync(ImportCategoriesFromXmlCommand command, CancellationToken cancellationToken = default)
        {
            LastImportCommand = command;
            return Task.FromResult(ImportResult);
        }

        public Task<Result<CategoryDto>> UpdateCategoryAsync(UpdateCategoryCommand command, CancellationToken cancellationToken = default)
        {
            LastUpdateCommand = command;
            return Task.FromResult(UpdateResult);
        }

        public Task<Result<CategoryDto>> ChangeCategoryStatusAsync(ChangeCategoryStatusCommand command, CancellationToken cancellationToken = default)
        {
            LastChangeStatusCommand = command;
            return Task.FromResult(ChangeStatusResult);
        }
    }

    private sealed class FakeTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
