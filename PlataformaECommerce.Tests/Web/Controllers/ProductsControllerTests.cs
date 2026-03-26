using Microsoft.AspNetCore.Mvc;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Products.Commands;
using PlataformaECommerce.Application.Features.Products.DTOs;
using PlataformaECommerce.Application.Features.Products.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Products;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Web.Controllers;
using PlataformaECommerce.Web.OpenApi;

namespace PlataformaECommerce.Tests.Web.Controllers;

[TestFixture]
public class ProductsControllerTests
{
    [Test]
    public void Controller_GrupoSwaggerEsPublico()
    {
        ApiExplorerSettingsAttribute? attribute = typeof(ProductsController)
            .GetCustomAttributes(typeof(ApiExplorerSettingsAttribute), inherit: true)
            .OfType<ApiExplorerSettingsAttribute>()
            .SingleOrDefault();

        Assert.That(attribute?.GroupName, Is.EqualTo(SwaggerGroups.Public));
    }

    [Test]
    public async Task GetAll_ConsultaValida_RetornaOk()
    {
        FakeProductApplicationService service = new();
        ProductsController controller = new(service);

        ActionResult<ProductQueryResultDto> result = await controller.GetAll(new GetProductsQuery(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task GetById_IdValido_RetornaOk()
    {
        FakeProductApplicationService service = new();
        ProductsController controller = new(service);

        ActionResult<ProductDetailDto> result = await controller.GetById(Guid.NewGuid(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
    }

    private sealed class FakeProductApplicationService : IProductApplicationService
    {
        public Task<Result<Guid>> CreatePhysicalProductAsync(CreatePhysicalProductCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(Guid.NewGuid()));

        public Task<Result<Guid>> CreateDigitalProductAsync(CreateDigitalProductCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(Guid.NewGuid()));

        public Task<Result<ProductImportResultDto>> ImportProductsAsync(ImportProductsCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<ProductResponseDto>> UpdateProductAsync(UpdateProductCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(CreateResponse()));

        public Task<Result<ProductResponseDto>> UpdateProductStockAsync(UpdateProductStockCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(CreateResponse()));

        public Task<Result<ProductResponseDto>> ActivateProductAsync(ActivateProductCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(CreateResponse()));

        public Task<Result<ProductResponseDto>> DeactivateProductAsync(DeactivateProductCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(CreateResponse()));

        public Task<Result<ProductResponseDto>> ApplyProductPromotionAsync(ApplyProductPromotionCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(CreateResponse()));

        public Task<Result<ProductResponseDto>> RemoveProductPromotionAsync(RemoveProductPromotionCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(CreateResponse()));

        public Task<Result<ProductResponseDto>> FeatureProductAsync(FeatureProductCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(CreateResponse()));

        public Task<Result<ProductResponseDto>> UnfeatureProductAsync(UnfeatureProductCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(CreateResponse()));

        public Task<Result<ProductDetailDto>> GetProductByIdAsync(GetProductByIdQuery query, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(new ProductDetailDto
            {
                Id = query.ProductId,
                Name = "Producto público",
                Description = "Producto de catálogo.",
                Sku = "PROD-001",
                Slug = "producto-publico",
                Price = 100m,
                BasePrice = 100m,
                Currency = "COP",
                Stock = 10,
                IsActive = true,
                ProductType = TipoProducto.Fisico
            }));

        public Task<Result<ProductQueryResultDto>> GetProductsAsync(GetProductsQuery query, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(new ProductQueryResultDto
            {
                Items =
                [
                    new ProductDto
                    {
                        Id = Guid.NewGuid(),
                        Name = "Producto público",
                        Description = "Producto de catálogo.",
                        Sku = "PROD-001",
                        Slug = "producto-publico",
                        Price = 100m,
                        BasePrice = 100m,
                        Currency = "COP",
                        Stock = 10,
                        IsActive = true,
                        ProductType = TipoProducto.Fisico
                    }
                ],
                TotalCount = 1,
                ReturnedCount = 1,
                PageNumber = 1,
                PageSize = 20,
                TotalPages = 1
            }));

        private static ProductResponseDto CreateResponse()
        {
            return new ProductResponseDto
            {
                Id = Guid.NewGuid(),
                Name = "Producto prueba",
                Description = "Producto de prueba.",
                Sku = "PROD-001",
                Slug = "producto-prueba",
                Price = 90m,
                BasePrice = 100m,
                PromotionalPrice = 90m,
                Currency = "COP",
                Stock = 10,
                IsActive = true,
                IsFeatured = false,
                HasPromotion = true,
                CurrentDiscountPercentage = 10m,
                ProductType = TipoProducto.Fisico
            };
        }
    }
}
