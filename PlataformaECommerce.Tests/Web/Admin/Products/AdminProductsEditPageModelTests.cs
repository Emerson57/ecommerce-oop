using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Audit.DTOs;
using PlataformaECommerce.Application.Features.Audit.Queries;
using PlataformaECommerce.Application.Features.Products.Commands;
using PlataformaECommerce.Application.Features.Products.DTOs;
using PlataformaECommerce.Application.Features.Products.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Audit;
using PlataformaECommerce.Application.Interfaces.Services.Products;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Web.Pages.Admin.Products;

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
            ProductType = TipoProducto.Fisico,
            IsActive = true,
            IsFeatured = false,
            Tags = "gaming, perifericos"
        };

        IActionResult result = await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.InstanceOf<RedirectToPageResult>());
    }

    private static EditModel CreatePageModel(IProductApplicationService service, IAuditApplicationService auditApplicationService)
    {
        EditModel pageModel = new(service, auditApplicationService);
        DefaultHttpContext httpContext = new();
        pageModel.PageContext = new PageContext { HttpContext = httpContext };
        pageModel.TempData = new TempDataDictionary(httpContext, new FakeTempDataProvider());
        return pageModel;
    }

    private sealed class FakeProductApplicationService : IProductApplicationService
    {
        public Task<Result<Guid>> CreatePhysicalProductAsync(CreatePhysicalProductCommand command, CancellationToken cancellationToken = default) => Task.FromResult(Result.Success(Guid.NewGuid()));
        public Task<Result<Guid>> CreateDigitalProductAsync(CreateDigitalProductCommand command, CancellationToken cancellationToken = default) => Task.FromResult(Result.Success(Guid.NewGuid()));
        public Task<Result<ProductResponseDto>> UpdateProductAsync(UpdateProductCommand command, CancellationToken cancellationToken = default) => Task.FromResult(Result.Success(new ProductResponseDto { Id = command.Id, Name = command.Name }));
        public Task<Result<ProductResponseDto>> UpdateProductStockAsync(UpdateProductStockCommand command, CancellationToken cancellationToken = default) => Task.FromResult(Result.Success(new ProductResponseDto()));
        public Task<Result<ProductResponseDto>> ActivateProductAsync(ActivateProductCommand command, CancellationToken cancellationToken = default) => Task.FromResult(Result.Success(new ProductResponseDto()));
        public Task<Result<ProductResponseDto>> DeactivateProductAsync(DeactivateProductCommand command, CancellationToken cancellationToken = default) => Task.FromResult(Result.Success(new ProductResponseDto()));
        public Task<Result<ProductResponseDto>> ApplyProductPromotionAsync(ApplyProductPromotionCommand command, CancellationToken cancellationToken = default) => Task.FromResult(Result.Success(new ProductResponseDto()));
        public Task<Result<ProductResponseDto>> RemoveProductPromotionAsync(RemoveProductPromotionCommand command, CancellationToken cancellationToken = default) => Task.FromResult(Result.Success(new ProductResponseDto()));
        public Task<Result<ProductResponseDto>> FeatureProductAsync(FeatureProductCommand command, CancellationToken cancellationToken = default) => Task.FromResult(Result.Success(new ProductResponseDto()));
        public Task<Result<ProductResponseDto>> UnfeatureProductAsync(UnfeatureProductCommand command, CancellationToken cancellationToken = default) => Task.FromResult(Result.Success(new ProductResponseDto()));

        public Task<Result<ProductDetailDto>> GetProductByIdAsync(GetProductByIdQuery query, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result.Success(new ProductDetailDto
            {
                Id = query.ProductId,
                Name = "Mouse gamer",
                Description = "Producto de prueba.",
                Sku = "PROD-010",
                Slug = "mouse-gamer",
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

    private sealed class FakeTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
