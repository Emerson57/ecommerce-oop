using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Audit.DTOs;
using PlataformaECommerce.Application.Features.Audit.Queries;
using PlataformaECommerce.Application.Features.Categories.Commands;
using PlataformaECommerce.Application.Features.Categories.DTOs;
using PlataformaECommerce.Application.Features.Categories.Queries;
using PlataformaECommerce.Application.Features.Products.Commands;
using PlataformaECommerce.Application.Features.Products.DTOs;
using PlataformaECommerce.Application.Features.Products.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Audit;
using PlataformaECommerce.Application.Interfaces.Services.Categories;
using PlataformaECommerce.Application.Interfaces.Services.Products;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Web.Pages.Admin.Products;
using PlataformaECommerce.Web.Services.Products;

namespace PlataformaECommerce.Tests.Web.Admin.Products;

[TestFixture]
public class AdminProductsEditPageModelTests
{
    [Test]
    public async Task OnGetAsync_ProductoExistente_MapeaFormulario()
    {
        FakeProductApplicationService service = new();
        EditModel pageModel = CreatePageModel(service, new FakeAuditApplicationService());

        IActionResult result = await pageModel.OnGetAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(result, Is.InstanceOf<PageResult>());
        Assert.That(pageModel.Input.Name, Is.EqualTo("Mouse gamer"));
    }

    [Test]
    public async Task OnGetAsync_ProductoExistente_MapeaImagenPrincipalEnContrato()
    {
        FakeProductApplicationService service = new();
        EditModel pageModel = CreatePageModel(service, new FakeAuditApplicationService());

        await pageModel.OnGetAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(pageModel.Input.Images.MainImage.CurrentImageUrl, Is.EqualTo("/uploads/products/mouse-gamer-actual.webp"));
    }

    [Test]
    public async Task OnGetAsync_ProductoExistente_MapeaGaleriaEnTresSlotsFijos()
    {
        FakeProductApplicationService service = new();
        EditModel pageModel = CreatePageModel(service, new FakeAuditApplicationService());

        await pageModel.OnGetAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(pageModel.Input.Images.Gallery.Count, Is.EqualTo(ProductImagesInputModel.DefaultGallerySlots));
        Assert.That(pageModel.Input.Images.Gallery[0].ImageUrl, Is.EqualTo("https://cdn.novashop.com/products/mouse-gamer-side.webp"));
        Assert.That(pageModel.Input.Images.Gallery[1].ImageUrl, Is.EqualTo("/images/products/mouse-gamer-box.webp"));
    }

    [Test]
    public async Task OnGetAsync_ProductoExistente_CargaHistorialPromocional()
    {
        FakeProductApplicationService service = new();
        EditModel pageModel = CreatePageModel(service, new FakeAuditApplicationService());

        await pageModel.OnGetAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(pageModel.PromotionHistory.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task OnPostAsync_FormularioValido_RedireccionaAlListado()
    {
        FakeProductApplicationService service = new();
        EditModel pageModel = CreatePageModel(service, new FakeAuditApplicationService());
        pageModel.Input = new EditModel.InputModel
        {
            Id = Guid.NewGuid(),
            Name = "Mouse gamer",
            Description = "Producto actualizado.",
            Sku = "PROD-010",
            Price = 149900m,
            Currency = "COP",
            Stock = 7,
            Slug = "mouse-gamer",
            Images = new ProductImagesInputModel
            {
                MainImage = new ProductMainImageInputModel
                {
                    CurrentImageUrl = "/uploads/products/mouse-gamer-actual.webp",
                    ExternalImageUrl = "https://cdn.novashop.com/products/mouse-gamer.webp"
                },
                Gallery =
                [
                    new ProductGalleryImageInputModel { ImageUrl = "https://cdn.novashop.com/products/mouse-gamer-side.webp" },
                    new ProductGalleryImageInputModel { ImageUrl = "/images/products/mouse-gamer-box.webp" },
                    new ProductGalleryImageInputModel { ImageUrl = "https://cdn.novashop.com/products/mouse-gamer.webp" }
                ]
            },
            ProductType = TipoProducto.Fisico,
            IsActive = true,
            IsFeatured = false,
            Tags = "gaming, perifericos"
        };

        IActionResult result = await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.InstanceOf<RedirectToPageResult>());
        Assert.That(service.LastUpdateCommand?.MainImageUrl, Is.EqualTo("https://cdn.novashop.com/products/mouse-gamer.webp"));
        Assert.That(service.LastUpdateCommand?.ImageGallery, Is.EqualTo(new[]
        {
            "https://cdn.novashop.com/products/mouse-gamer-side.webp",
            "/images/products/mouse-gamer-box.webp"
        }));
    }

    private static EditModel CreatePageModel(FakeProductApplicationService service, IAuditApplicationService auditApplicationService)
    {
        EditModel pageModel = new(service, service, new FakeCategoryApplicationService(), auditApplicationService, new FakeProductImageStorageService());
        DefaultHttpContext httpContext = new();
        pageModel.PageContext = new PageContext { HttpContext = httpContext };
        pageModel.TempData = new TempDataDictionary(httpContext, new FakeTempDataProvider());
        return pageModel;
    }

    private sealed class FakeProductApplicationService : IProductCommandService, IProductQueryService
    {
        public UpdateProductCommand? LastUpdateCommand { get; private set; }

        public Task<Result<Guid>> CreatePhysicalProductAsync(CreatePhysicalProductCommand command, CancellationToken cancellationToken = default) => Task.FromResult(Result.Success(Guid.NewGuid()));
        public Task<Result<Guid>> CreateDigitalProductAsync(CreateDigitalProductCommand command, CancellationToken cancellationToken = default) => Task.FromResult(Result.Success(Guid.NewGuid()));
        public Task<Result<ProductImportResultDto>> ImportProductsAsync(ImportProductsCommand command, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<Result<ProductResponseDto>> UpdateProductAsync(UpdateProductCommand command, CancellationToken cancellationToken = default)
        {
            LastUpdateCommand = command;
            return Task.FromResult(Result.Success(new ProductResponseDto { Id = command.Id, Name = command.Name }));
        }

        public Task<Result<ProductDetailDto>> GetProductByIdAsync(GetProductByIdQuery query, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result.Success(new ProductDetailDto
            {
                Id = query.ProductId,
                Name = "Mouse gamer",
                Description = "Producto de prueba.",
                Sku = "PROD-010",
                Slug = "mouse-gamer",
                MainImageUrl = "/uploads/products/mouse-gamer-actual.webp",
                ImageGallery = new[]
                {
                    "https://cdn.novashop.com/products/mouse-gamer-side.webp",
                    "/images/products/mouse-gamer-box.webp"
                },
                Price = 129900m,
                BasePrice = 149900m,
                PromotionalPrice = 129900m,
                CurrentDiscountPercentage = 13.33m,
                HasPromotion = true,
                Currency = "COP",
                Stock = 8,
                IsActive = true,
                IsFeatured = false,
                ProductType = TipoProducto.Fisico,
                Tags = new[] { "gaming", "perifericos" },
                WeightKg = 0.45m,
                RequiresShipping = true
            }));
        }

        public Task<Result<ProductQueryResultDto>> GetProductsAsync(GetProductsQuery query, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(new ProductQueryResultDto()));
    }

    private sealed class FakeProductImageStorageService : IProductImageStorageService
    {
        public Task<ProductImageProcessResult> ProcessMainImageAsync(IFormFile? uploadedImage, string? externalImageUrl, string? currentImageUrl, string productSlug, bool removeCurrentImage, CancellationToken cancellationToken = default)
            => Task.FromResult(ProductImageProcessResult.Success(removeCurrentImage ? null : externalImageUrl ?? currentImageUrl));

        public Task DeleteIfManagedAsync(string? imageUrl, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeAuditApplicationService : IAuditApplicationService
    {
        public Task<Result<AuditQueryResultDto>> GetAuditTrailAsync(GetAuditTrailQuery query, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result.Success(new AuditQueryResultDto
            {
                Items =
                [
                    new AuditEntryDto
                    {
                        AggregateId = query.AggregateId ?? Guid.NewGuid(),
                        AggregateType = "Producto",
                        Module = "Products",
                        Action = "product.promotion.applied",
                        Detail = "Se aplicó una promoción sobre el producto.",
                        PerformedBy = "Administrador",
                        OccurredAtUtc = DateTime.UtcNow,
                        Metadata = new Dictionary<string, string>
                        {
                            ["discountPercentage"] = "13.33",
                            ["previousPrice"] = "149900",
                            ["newPrice"] = "129900",
                            ["currency"] = "COP"
                        }
                    },
                    new AuditEntryDto
                    {
                        AggregateId = query.AggregateId ?? Guid.NewGuid(),
                        AggregateType = "Producto",
                        Module = "Products",
                        Action = "product.promotion.removed",
                        Detail = "Se retiró la promoción activa del producto.",
                        PerformedBy = "Administrador",
                        OccurredAtUtc = DateTime.UtcNow.AddMinutes(-30),
                        Metadata = new Dictionary<string, string>
                        {
                            ["previousPromotionalPrice"] = "129900",
                            ["restoredBasePrice"] = "149900",
                            ["currency"] = "COP"
                        }
                    }
                ],
                TotalCount = 2,
                ReturnedCount = 2,
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1
            }));
        }
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
