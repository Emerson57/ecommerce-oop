using Microsoft.AspNetCore.Http;
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
public class CatalogIndexPageModelTests
{
    [Test]
    public async Task OnGetAsync_ProductoConGaleria_ProyectaImagenesComplementariasEnCatalogo()
    {
        FakeProductQueryService productApplicationService = new(
            new ProductQueryResultDto
            {
                Items =
                [
                    new ProductDto
                    {
                        Id = Guid.NewGuid(),
                        Name = "Teclado mecánico",
                        Description = "Descripción de prueba.",
                        Sku = "CAT-100",
                        Slug = "teclado-mecanico",
                        Price = 199900m,
                        BasePrice = 199900m,
                        Currency = "COP",
                        Stock = 5,
                        IsActive = true,
                        ProductType = TipoProducto.Fisico,
                        MainImageUrl = "https://cdn.novashop.com/products/teclado-main.webp",
                        ImageGallery =
                        [
                            "https://cdn.novashop.com/products/teclado-side.webp",
                            "/images/products/teclado-box.webp"
                        ]
                    }
                ],
                TotalCount = 1,
                ReturnedCount = 1,
                PageNumber = 1,
                PageSize = 20,
                TotalPages = 1
            });

        IndexModel pageModel = CreatePageModel(productApplicationService);

        await pageModel.OnGetAsync(CancellationToken.None);

        Assert.That(pageModel.Products.Single().ImageUrls, Is.EqualTo(new[]
        {
            "https://cdn.novashop.com/products/teclado-main.webp",
            "https://cdn.novashop.com/products/teclado-side.webp",
            "/images/products/teclado-box.webp"
        }));
    }

    [Test]
    public async Task OnGetAsync_ConCategoryId_EnviaFiltroDeCategoriaAlServicio()
    {
        FakeProductQueryService productApplicationService = new(new ProductQueryResultDto());
        IndexModel pageModel = CreatePageModel(productApplicationService);
        Guid categoryId = Guid.NewGuid();
        pageModel.CategoryId = categoryId;

        await pageModel.OnGetAsync(CancellationToken.None);

        Assert.That(productApplicationService.LastQuery?.CategoryId, Is.EqualTo(categoryId));
    }

    private static IndexModel CreatePageModel(FakeProductQueryService productApplicationService)
    {
        IndexModel pageModel = new(productApplicationService)
        {
            PageContext = new PageContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        return pageModel;
    }

    private sealed class FakeProductQueryService(ProductQueryResultDto queryResult) : IProductQueryService
    {
        public GetProductsQuery? LastQuery { get; private set; }

        public Task<Result<ProductDetailDto>> GetProductByIdAsync(GetProductByIdQuery query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<ProductQueryResultDto>> GetProductsAsync(GetProductsQuery query, CancellationToken cancellationToken = default)
        {
            LastQuery = query;
            return Task.FromResult(Result.Success(queryResult));
        }
    }
}
