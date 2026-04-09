using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
    public void Controller_ApiPublica_ExcluyeAntiforgeryDeFormaExplicita()
    {
        IgnoreAntiforgeryTokenAttribute? attribute = typeof(ProductsController)
            .GetCustomAttributes(typeof(IgnoreAntiforgeryTokenAttribute), inherit: true)
            .OfType<IgnoreAntiforgeryTokenAttribute>()
            .SingleOrDefault();

        Assert.That(attribute, Is.Not.Null);
    }

    [Test]
    public async Task GetAll_ConsultaValida_RetornaOk()
    {
        FakeProductQueryService service = new();
        ProductsController controller = new(service);

        ActionResult<ProductQueryResultDto> result = await controller.GetAll(new GetProductsQuery(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task GetById_IdValido_RetornaOk()
    {
        FakeProductQueryService service = new();
        ProductsController controller = new(service);

        ActionResult<ProductDetailDto> result = await controller.GetById(Guid.NewGuid(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
    }

    private sealed class FakeProductQueryService : IProductQueryService
    {
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

    }
}
