using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Catalog.DTOs;
using PlataformaECommerce.Application.Features.Catalog.Queries;
using PlataformaECommerce.Application.Features.Categories.Commands;
using PlataformaECommerce.Application.Features.Categories.DTOs;
using PlataformaECommerce.Application.Features.Categories.Queries;
using PlataformaECommerce.Application.Features.Products.DTOs;
using PlataformaECommerce.Application.Interfaces.Services.Catalog;
using PlataformaECommerce.Application.Interfaces.Services.Categories;
using PlataformaECommerce.Web.Pages;

namespace PlataformaECommerce.Tests.Web.Home;

[TestFixture]
public class IndexPageModelTests
{
    [Test]
    public async Task OnGetAsync_DatosDisponibles_CargaCategoriasYProductosDestacados()
    {
        FakeCatalogApplicationService catalogApplicationService = new(
            featuredProducts:
            [
                new FeaturedProductDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Teclado mecánico",
                    Sku = "HOME-001",
                    Price = 199900m,
                    Currency = "COP",
                    CategoryName = "Tecnología",
                    BadgeText = "Nuevo",
                    MainImageUrl = "https://cdn.novashop.com/products/teclado-main.webp",
                    ImageUrls = ["https://cdn.novashop.com/products/teclado-side.webp"],
                    IsAvailable = true,
                    HasStock = true,
                    ProductType = PlataformaECommerce.Domain.Enums.TipoProducto.Fisico
                }
            ]);
        FakeCategoryApplicationService categoryApplicationService = new(
        [
            new CategoryDto
            {
                Id = Guid.NewGuid(),
                Name = "Tecnología",
                Description = "Accesorios y dispositivos.",
                IsActive = true,
                IsRootCategory = true,
                Slug = "tecnologia"
            }
        ]);
        IndexModel pageModel = CreatePageModel(catalogApplicationService, categoryApplicationService);

        await pageModel.OnGetAsync(CancellationToken.None);

        Assert.That(pageModel.FeaturedProducts.Count, Is.EqualTo(1));
        Assert.That(pageModel.FeaturedCategories.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task OnGetAsync_SinDestacados_UsaFallbackDeCatalogo()
    {
        FakeCatalogApplicationService catalogApplicationService = new(
            featuredProducts: [],
            catalogProducts:
            [
                new CatalogProductDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Curso .NET 10",
                    Sku = "HOME-002",
                    Price = 129900m,
                    Currency = "COP",
                    MainImageUrl = "https://cdn.novashop.com/products/curso-main.webp",
                    ImageUrls = ["/images/products/curso-box.webp"],
                    IsActive = true,
                    IsAvailable = true,
                    HasStock = true,
                    CategoryName = "Productos digitales",
                    ProductType = PlataformaECommerce.Domain.Enums.TipoProducto.Digital
                }
            ]);
        FakeCategoryApplicationService categoryApplicationService = new([]);
        IndexModel pageModel = CreatePageModel(catalogApplicationService, categoryApplicationService);

        await pageModel.OnGetAsync(CancellationToken.None);

        Assert.That(pageModel.FeaturedProducts.Single().Name, Is.EqualTo("Curso .NET 10"));
    }

    [Test]
    public async Task OnGetAsync_EjecutaLaCargaDePortadaDeFormaSecuencial()
    {
        List<string> executionOrder = [];
        FakeCatalogApplicationService catalogApplicationService = new(
            featuredProducts: [],
            onGetFeaturedProductsAsync: async _ =>
            {
                executionOrder.Add("featured:start");
                await Task.CompletedTask;
            });
        FakeCategoryApplicationService categoryApplicationService = new(
            [],
            onGetCategoriesAsync: async _ =>
            {
                executionOrder.Add("categories:start");
                await Task.Yield();
                executionOrder.Add("categories:end");
            });
        IndexModel pageModel = CreatePageModel(catalogApplicationService, categoryApplicationService);

        await pageModel.OnGetAsync(CancellationToken.None);

        Assert.That(executionOrder, Is.EqualTo(new[] { "categories:start", "categories:end", "featured:start" }));
    }

    private static IndexModel CreatePageModel(
        FakeCatalogApplicationService catalogApplicationService,
        FakeCategoryApplicationService categoryApplicationService)
    {
        IndexModel pageModel = new(catalogApplicationService, categoryApplicationService)
        {
            PageContext = new PageContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        return pageModel;
    }

    private sealed class FakeCatalogApplicationService(
        IReadOnlyCollection<FeaturedProductDto>? featuredProducts = null,
        IReadOnlyCollection<CatalogProductDto>? catalogProducts = null,
        Func<GetFeaturedProductsQuery, Task>? onGetFeaturedProductsAsync = null) : ICatalogApplicationService
    {
        public Task<Result<CatalogQueryResultDto>> GetCatalogProductsAsync(
            GetCatalogProductsQuery query,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(new CatalogQueryResultDto
            {
                Items = catalogProducts ?? Array.Empty<CatalogProductDto>(),
                TotalCount = catalogProducts?.Count ?? 0,
                ReturnedCount = catalogProducts?.Count ?? 0,
                PageNumber = 1,
                PageSize = Math.Max(1, catalogProducts?.Count ?? 1),
                TotalPages = catalogProducts is { Count: > 0 } ? 1 : 0,
                HasPreviousPage = false,
                HasNextPage = false
            }));

        public async Task<Result<IReadOnlyCollection<FeaturedProductDto>>> GetFeaturedProductsAsync(
            GetFeaturedProductsQuery query,
            CancellationToken cancellationToken = default)
        {
            if (onGetFeaturedProductsAsync is not null)
            {
                await onGetFeaturedProductsAsync(query);
            }

            return Result.Success(featuredProducts ?? Array.Empty<FeaturedProductDto>());
        }
    }

    private sealed class FakeCategoryApplicationService(
        IReadOnlyCollection<CategoryDto> categories,
        Func<GetCategoriesQuery, Task>? onGetCategoriesAsync = null) : ICategoryApplicationService
    {
        public async Task<Result<IReadOnlyCollection<CategoryDto>>> GetCategoriesAsync(GetCategoriesQuery query, CancellationToken cancellationToken = default)
        {
            if (onGetCategoriesAsync is not null)
            {
                await onGetCategoriesAsync(query);
            }

            return Result.Success(categories);
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
}
