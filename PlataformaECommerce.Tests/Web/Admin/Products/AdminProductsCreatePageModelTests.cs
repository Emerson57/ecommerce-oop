using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Products.Commands;
using PlataformaECommerce.Application.Features.Products.DTOs;
using PlataformaECommerce.Application.Features.Products.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Products;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Web.Pages.Admin.Products;

namespace PlataformaECommerce.Tests.Web.Admin.Products;

[TestFixture]
public class AdminProductsCreatePageModelTests
{
    [Test]
    public void OnGet_SinTipoSeleccionado_InicializaProductoFisico()
    {
        CreateModel pageModel = CreatePageModel(new FakeProductApplicationService());

        pageModel.OnGet();

        Assert.That(pageModel.Input.ProductType, Is.EqualTo(TipoProducto.Fisico));
    }

    [Test]
    public async Task OnPostAsync_ProductoFisicoValido_RedireccionaAEdicion()
    {
        CreateModel pageModel = CreatePageModel(new FakeProductApplicationService());
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
            IsActive = true,
            WeightKg = 4.5m,
            HeightCm = 40m,
            WidthCm = 60m,
            LengthCm = 12m,
            RequiresShipping = true
        };

        IActionResult result = await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.InstanceOf<RedirectToPageResult>());
    }

    [Test]
    public async Task OnPostAsync_ProductoDigitalValido_RedireccionaAEdicion()
    {
        CreateModel pageModel = CreatePageModel(new FakeProductApplicationService());
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
    }

    private static CreateModel CreatePageModel(IProductApplicationService service)
    {
        CreateModel pageModel = new(service);
        DefaultHttpContext httpContext = new();
        pageModel.PageContext = new PageContext { HttpContext = httpContext };
        pageModel.TempData = new TempDataDictionary(httpContext, new FakeTempDataProvider());
        return pageModel;
    }

    private sealed class FakeProductApplicationService : IProductApplicationService
    {
        public Task<Result<Guid>> CreatePhysicalProductAsync(CreatePhysicalProductCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(Guid.NewGuid()));

        public Task<Result<Guid>> CreateDigitalProductAsync(CreateDigitalProductCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(Guid.NewGuid()));

        public Task<Result<ProductResponseDto>> UpdateProductAsync(UpdateProductCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(new ProductResponseDto()));

        public Task<Result<ProductResponseDto>> UpdateProductStockAsync(UpdateProductStockCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(new ProductResponseDto()));

        public Task<Result<ProductResponseDto>> ActivateProductAsync(ActivateProductCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(new ProductResponseDto()));

        public Task<Result<ProductResponseDto>> DeactivateProductAsync(DeactivateProductCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(new ProductResponseDto()));

        public Task<Result<ProductResponseDto>> ApplyProductPromotionAsync(ApplyProductPromotionCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(new ProductResponseDto()));

        public Task<Result<ProductResponseDto>> RemoveProductPromotionAsync(RemoveProductPromotionCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(new ProductResponseDto()));

        public Task<Result<ProductResponseDto>> FeatureProductAsync(FeatureProductCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(new ProductResponseDto()));

        public Task<Result<ProductResponseDto>> UnfeatureProductAsync(UnfeatureProductCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(new ProductResponseDto()));

        public Task<Result<ProductDetailDto>> GetProductByIdAsync(GetProductByIdQuery query, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(new ProductDetailDto()));

        public Task<Result<ProductQueryResultDto>> GetProductsAsync(GetProductsQuery query, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(new ProductQueryResultDto()));
    }

    private sealed class FakeTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
