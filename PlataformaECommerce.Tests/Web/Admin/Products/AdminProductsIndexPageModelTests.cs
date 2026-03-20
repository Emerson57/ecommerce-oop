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

    private static IndexModel CreatePageModel(IProductApplicationService service)
    {
        IndexModel pageModel = new(service);
        DefaultHttpContext httpContext = new();
        pageModel.PageContext = new PageContext { HttpContext = httpContext };
        pageModel.TempData = new TempDataDictionary(httpContext, new FakeTempDataProvider());
        return pageModel;
    }

    private sealed class FakeProductApplicationService : IProductApplicationService
    {
        public Task<Result<Guid>> CreatePhysicalProductAsync(CreatePhysicalProductCommand command, CancellationToken cancellationToken = default) => Task.FromResult(Result.Success(Guid.NewGuid()));
        public Task<Result<Guid>> CreateDigitalProductAsync(CreateDigitalProductCommand command, CancellationToken cancellationToken = default) => Task.FromResult(Result.Success(Guid.NewGuid()));
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

    private sealed class FakeTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
