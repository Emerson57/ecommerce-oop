using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Catalog.DTOs;
using PlataformaECommerce.Application.Features.Catalog.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Catalog;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Web.Pages.Catalog;

namespace PlataformaECommerce.Tests.Web.Catalog;

[TestFixture]
public class CatalogIndexPageModelTests
{
    [Test]
    public async Task OnGetAsync_ProductoConGaleria_ProyectaImagenesComplementariasEnCatalogo()
    {
        FakeCatalogApplicationService catalogApplicationService = new(
        [
            new CatalogProductDto
            {
                Id = Guid.NewGuid(),
                Name = "Teclado mecánico",
                Description = "Descripción de prueba.",
                Sku = "CAT-100",
                Slug = "teclado-mecanico",
                Price = 199900m,
                Currency = "COP",
                AvailableStock = 5,
                IsActive = true,
                IsAvailable = true,
                HasStock = true,
                ProductType = TipoProducto.Fisico,
                MainImageUrl = "https://cdn.novashop.com/products/teclado-main.webp",
                ImageUrls =
                [
                    "https://cdn.novashop.com/products/teclado-side.webp",
                    "/images/products/teclado-box.webp"
                ]
            }
        ]);

        IndexModel pageModel = CreatePageModel(catalogApplicationService);

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
        FakeCatalogApplicationService catalogApplicationService = new([]);
        IndexModel pageModel = CreatePageModel(catalogApplicationService);
        Guid categoryId = Guid.NewGuid();
        pageModel.CategoryId = categoryId;

        await pageModel.OnGetAsync(CancellationToken.None);

        Assert.That(catalogApplicationService.LastQuery?.CategoryId, Is.EqualTo(categoryId));
        Assert.That(catalogApplicationService.LastQuery?.IsAvailable, Is.True);
    }

    [Test]
    public async Task OnGetAsync_ConProductType_EnviaFiltroPublicoAlServicioDeCatalogo()
    {
        FakeCatalogApplicationService catalogApplicationService = new([]);
        IndexModel pageModel = CreatePageModel(catalogApplicationService);
        pageModel.ProductType = TipoProducto.Digital;

        await pageModel.OnGetAsync(CancellationToken.None);

        Assert.That(catalogApplicationService.LastQuery?.ProductType, Is.EqualTo(TipoProducto.Digital));
    }

    private static IndexModel CreatePageModel(FakeCatalogApplicationService catalogApplicationService)
    {
        IndexModel pageModel = new(catalogApplicationService)
        {
            PageContext = new PageContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        return pageModel;
    }

    private sealed class FakeCatalogApplicationService(IReadOnlyCollection<CatalogProductDto> catalogProducts) : ICatalogApplicationService
    {
        public GetCatalogProductsQuery? LastQuery { get; private set; }

        public Task<Result<IReadOnlyCollection<CatalogProductDto>>> GetCatalogProductsAsync(GetCatalogProductsQuery query, CancellationToken cancellationToken = default)
        {
            LastQuery = query;
            return Task.FromResult(Result.Success(catalogProducts));
        }

        public Task<Result<IReadOnlyCollection<FeaturedProductDto>>> GetFeaturedProductsAsync(GetFeaturedProductsQuery query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
