using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Products.Commands;
using PlataformaECommerce.Application.Features.Products.DTOs;
using PlataformaECommerce.Application.Features.Products.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Products;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Web.Pages.Catalog;

namespace PlataformaECommerce.Tests.Web.Catalog;

[TestFixture]
public class CatalogDetailsPageModelTests
{
    [Test]
    public async Task OnGetAsync_ProductoConCategoriaRaiz_ProyectaEtiquetaDeCategoria()
    {
        FakeProductApplicationService productApplicationService = new(new ProductDetailDto
        {
            Id = Guid.NewGuid(),
            Name = "Teclado mecánico",
            Description = "Descripción de prueba.",
            Sku = "CAT-001",
            Slug = "teclado-mecanico",
            Price = 199900m,
            BasePrice = 199900m,
            Currency = "COP",
            Stock = 5,
            IsActive = true,
            ProductType = TipoProducto.Fisico,
            CategoryId = Guid.NewGuid(),
            Tags = ["gaming"]
        });

        DetailsModel pageModel = CreatePageModel(productApplicationService);

        IActionResult result = await pageModel.OnGetAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(pageModel.Product.CategoryName, Is.EqualTo("Categoría asignada"));
    }

    [Test]
    public async Task OnGetAsync_ProductoConSubcategoria_ProyectaEtiquetaDeSubcategoria()
    {
        FakeProductApplicationService productApplicationService = new(new ProductDetailDto
        {
            Id = Guid.NewGuid(),
            Name = "Mouse inalámbrico",
            Description = "Descripción de prueba.",
            Sku = "CAT-002",
            Slug = "mouse-inalambrico",
            Price = 99900m,
            BasePrice = 99900m,
            Currency = "COP",
            Stock = 8,
            IsActive = true,
            ProductType = TipoProducto.Fisico,
            CategoryId = Guid.NewGuid(),
            SubcategoryId = Guid.NewGuid(),
            Tags = ["periféricos"]
        });

        DetailsModel pageModel = CreatePageModel(productApplicationService);

        IActionResult result = await pageModel.OnGetAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(pageModel.Product.CategoryName, Is.EqualTo("Subcategoría asignada"));
    }

    [Test]
    public async Task OnGetAsync_ProductoConGaleria_ProyectaTodasLasImagenesVisibles()
    {
        FakeProductApplicationService productApplicationService = new(new ProductDetailDto
        {
            Id = Guid.NewGuid(),
            Name = "Mouse inalámbrico",
            Description = "Descripción de prueba.",
            Sku = "CAT-003",
            Slug = "mouse-inalambrico-pro",
            Price = 129900m,
            BasePrice = 129900m,
            Currency = "COP",
            Stock = 8,
            IsActive = true,
            ProductType = TipoProducto.Fisico,
            MainImageUrl = "https://cdn.novashop.com/products/mouse-main.webp",
            ImageGallery =
            [
                "https://cdn.novashop.com/products/mouse-side.webp",
                "/images/products/mouse-box.webp"
            ]
        });

        DetailsModel pageModel = CreatePageModel(productApplicationService);

        IActionResult result = await pageModel.OnGetAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(pageModel.Product.ImageUrls, Is.EqualTo(new[]
        {
            "https://cdn.novashop.com/products/mouse-main.webp",
            "https://cdn.novashop.com/products/mouse-side.webp",
            "/images/products/mouse-box.webp"
        }));
    }

    [Test]
    public async Task OnGetAsync_ProductoConPromocion_ProyectaIndicadoresComerciales()
    {
        FakeProductApplicationService productApplicationService = new(new ProductDetailDto
        {
            Id = Guid.NewGuid(),
            Name = "Monitor gamer",
            Description = "Descripción de prueba.",
            Sku = "CAT-004",
            Slug = "monitor-gamer",
            Price = 799900m,
            BasePrice = 999900m,
            PromotionalPrice = 799900m,
            CurrentDiscountPercentage = 20m,
            Currency = "COP",
            Stock = 3,
            IsActive = true,
            HasPromotion = true,
            ProductType = TipoProducto.Fisico
        });

        DetailsModel pageModel = CreatePageModel(productApplicationService);

        IActionResult result = await pageModel.OnGetAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(pageModel.Product.HasPromotion, Is.True);
        Assert.That(pageModel.Product.DiscountAmount, Is.EqualTo(200000m));
        Assert.That(pageModel.Product.CommercialBadge, Is.EqualTo("Promoción activa"));
    }

    [Test]
    public async Task OnGetAsync_ProductoDigital_ProyectaAtributosTecnicosYDisponibilidadComercial()
    {
        FakeProductApplicationService productApplicationService = new(new ProductDetailDto
        {
            Id = Guid.NewGuid(),
            Name = "Curso .NET",
            Description = "Descripción de prueba.",
            Sku = "CAT-005",
            Slug = "curso-dotnet",
            Price = 129900m,
            BasePrice = 129900m,
            Currency = "COP",
            Stock = 10,
            IsActive = true,
            ProductType = TipoProducto.Digital,
            FileFormat = "PDF",
            FileSizeMb = 250m,
            RequiresLicense = true
        });

        DetailsModel pageModel = CreatePageModel(productApplicationService);

        IActionResult result = await pageModel.OnGetAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(pageModel.Product.IsDigitalProduct, Is.True);
        Assert.That(pageModel.Product.FileFormat, Is.EqualTo("PDF"));
        Assert.That(pageModel.Product.RequiresLicense, Is.True);
        Assert.That(pageModel.Product.AvailabilityTitle, Is.EqualTo("Disponible para entrega digital"));
    }

    private static DetailsModel CreatePageModel(FakeProductApplicationService productApplicationService)
    {
        DetailsModel pageModel = new(productApplicationService)
        {
            PageContext = new PageContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        return pageModel;
    }

    private sealed class FakeProductApplicationService(ProductDetailDto productDetailDto) : IProductApplicationService
    {
        public Task<Result<Guid>> CreatePhysicalProductAsync(CreatePhysicalProductCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<Guid>> CreateDigitalProductAsync(CreateDigitalProductCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<ProductImportResultDto>> ImportProductsAsync(ImportProductsCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<ProductResponseDto>> UpdateProductAsync(UpdateProductCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<ProductResponseDto>> UpdateProductStockAsync(UpdateProductStockCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<ProductResponseDto>> ActivateProductAsync(ActivateProductCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<ProductResponseDto>> DeactivateProductAsync(DeactivateProductCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<ProductResponseDto>> ApplyProductPromotionAsync(ApplyProductPromotionCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<ProductResponseDto>> RemoveProductPromotionAsync(RemoveProductPromotionCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<ProductResponseDto>> FeatureProductAsync(FeatureProductCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<ProductResponseDto>> UnfeatureProductAsync(UnfeatureProductCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<ProductDetailDto>> GetProductByIdAsync(GetProductByIdQuery query, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(productDetailDto));

        public Task<Result<ProductQueryResultDto>> GetProductsAsync(GetProductsQuery query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
