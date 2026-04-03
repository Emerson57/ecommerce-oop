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
using PlataformaECommerce.Web.Services.Products;

namespace PlataformaECommerce.Tests.Web.Admin.Products;

[TestFixture]
public class AdminProductsCreatePageModelTests
{
    [Test]
    public async Task OnGetAsync_SinTipoSeleccionado_InicializaProductoFisico()
    {
        CreateModel pageModel = CreatePageModel(new FakeProductApplicationService());

        await pageModel.OnGetAsync();

        Assert.That(pageModel.Input.ProductType, Is.EqualTo(TipoProducto.Fisico));
    }

    [Test]
    public async Task OnGetAsync_SinTipoSeleccionado_InicializaTresSlotsDeGaleria()
    {
        CreateModel pageModel = CreatePageModel(new FakeProductApplicationService());

        await pageModel.OnGetAsync();

        Assert.That(pageModel.Input.Images.Gallery.Count, Is.EqualTo(ProductImagesInputModel.DefaultGallerySlots));
    }

    [Test]
    public async Task OnPostAsync_ProductoFisicoValido_RedireccionaAEdicion()
    {
        FakeProductApplicationService service = new();
        CreateModel pageModel = CreatePageModel(service);
        pageModel.Input = new CreateModel.InputModel
        {
            ProductType = TipoProducto.Fisico,
            Name = "Monitor 4K",
            Description = "Monitor profesional.",
            Sku = "MON-4K-001",
            Price = 1500000m,
            Currency = "COP",
            Stock = 8,
            Slug = "monitor-4k",
            Images = new ProductImagesInputModel
            {
                MainImage = new ProductMainImageInputModel
                {
                    ExternalImageUrl = "https://cdn.novashop.com/products/monitor-4k.webp"
                },
                Gallery =
                [
                    new ProductGalleryImageInputModel { ImageUrl = "https://cdn.novashop.com/products/monitor-4k-side.webp" },
                    new ProductGalleryImageInputModel { ImageUrl = "/images/products/monitor-4k-back.webp" },
                    new ProductGalleryImageInputModel { ImageUrl = "https://cdn.novashop.com/products/monitor-4k.webp" }
                ]
            },
            IsActive = true,
            WeightKg = 4.5m,
            HeightCm = 40m,
            WidthCm = 60m,
            LengthCm = 12m,
            RequiresShipping = true
        };

        IActionResult result = await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.InstanceOf<RedirectToPageResult>());
        Assert.That(service.LastPhysicalCreateCommand?.MainImageUrl, Is.EqualTo("https://cdn.novashop.com/products/monitor-4k.webp"));
        Assert.That(service.LastPhysicalCreateCommand?.ImageGallery, Is.EqualTo(new[]
        {
            "https://cdn.novashop.com/products/monitor-4k-side.webp",
            "/images/products/monitor-4k-back.webp"
        }));
    }

    [Test]
    public async Task OnPostAsync_ProductoDigitalValido_RedireccionaAEdicion()
    {
        FakeProductApplicationService service = new();
        CreateModel pageModel = CreatePageModel(service);
        pageModel.Input = new CreateModel.InputModel
        {
            ProductType = TipoProducto.Digital,
            Name = "Curso .NET 10",
            Description = "Contenido digital profesional.",
            Sku = "CURSO-DOTNET10",
            Price = 299900m,
            Currency = "COP",
            Stock = 100,
            Slug = "curso-dotnet-10",
            IsActive = true,
            FileFormat = "MP4",
            FileSizeMb = 2048m,
            RequiresLicense = true
        };

        IActionResult result = await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.InstanceOf<RedirectToPageResult>());
        Assert.That(service.LastDigitalCreateCommand?.Name, Is.EqualTo("Curso .NET 10"));
    }

    private static CreateModel CreatePageModel(IProductCommandService service)
    {
        CreateModel pageModel = new(service, new FakeCategoryApplicationService(), new FakeProductImageStorageService());
        DefaultHttpContext httpContext = new();
        pageModel.PageContext = new PageContext { HttpContext = httpContext };
        pageModel.TempData = new TempDataDictionary(httpContext, new FakeTempDataProvider());
        return pageModel;
    }

    private sealed class FakeProductApplicationService : IProductCommandService
    {
        public CreatePhysicalProductCommand? LastPhysicalCreateCommand { get; private set; }

        public CreateDigitalProductCommand? LastDigitalCreateCommand { get; private set; }

        public Task<Result<Guid>> CreatePhysicalProductAsync(CreatePhysicalProductCommand command, CancellationToken cancellationToken = default)
        {
            LastPhysicalCreateCommand = command;
            return Task.FromResult(Result.Success(Guid.NewGuid()));
        }

        public Task<Result<Guid>> CreateDigitalProductAsync(CreateDigitalProductCommand command, CancellationToken cancellationToken = default)
        {
            LastDigitalCreateCommand = command;
            return Task.FromResult(Result.Success(Guid.NewGuid()));
        }

        public Task<Result<ProductImportResultDto>> ImportProductsAsync(ImportProductsCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<ProductResponseDto>> UpdateProductAsync(UpdateProductCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(new ProductResponseDto()));
    }

    private sealed class FakeProductImageStorageService : IProductImageStorageService
    {
        public Task<ProductImageProcessResult> ProcessMainImageAsync(IFormFile? uploadedImage, string? externalImageUrl, string? currentImageUrl, string productSlug, bool removeCurrentImage, CancellationToken cancellationToken = default)
            => Task.FromResult(ProductImageProcessResult.Success(externalImageUrl));

        public Task DeleteIfManagedAsync(string? imageUrl, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeCategoryApplicationService : ICategoryApplicationService
    {
        public Task<Result<IReadOnlyCollection<CategoryDto>>> GetCategoriesAsync(GetCategoriesQuery query, CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<CategoryDto> categories =
            [
                new CategoryDto { Id = Guid.NewGuid(), Name = "Periféricos", IsActive = true, IsRootCategory = true },
                new CategoryDto { Id = Guid.NewGuid(), Name = "Mouse", ParentCategoryId = Guid.NewGuid(), IsActive = true, IsRootCategory = false }
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
