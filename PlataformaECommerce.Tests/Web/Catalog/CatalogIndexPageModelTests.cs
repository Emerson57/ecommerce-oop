using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
            new CatalogQueryResultDto
            {
                Items =
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
                        ProductType = TipoProducto.Fisico,
                        MainImageUrl = "https://cdn.novashop.com/products/teclado-main.webp",
                        ImageUrls =
                        [
                            "https://cdn.novashop.com/products/teclado-side.webp",
                            "/images/products/teclado-box.webp"
                        ]
                    }
                ],
                TotalCount = 1,
                ReturnedCount = 1,
                PageNumber = 1,
                PageSize = 12,
                TotalPages = 1
            });

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
    public async Task OnGetAsync_ConCategoryName_EnviaFiltroDeCategoriaAlServicio()
    {
        FakeCatalogApplicationService catalogApplicationService = new(new CatalogQueryResultDto());
        IndexModel pageModel = CreatePageModel(catalogApplicationService);
        pageModel.CategoryName = "Tecnología";

        await pageModel.OnGetAsync(CancellationToken.None);

        Assert.That(catalogApplicationService.LastQuery?.CategoryName, Is.EqualTo("Tecnología"));
    }

    [Test]
    public async Task OnGetAsync_ResultadoPaginado_ProyectaMetadatosDeNavegacion()
    {
        FakeCatalogApplicationService catalogApplicationService = new(new CatalogQueryResultDto
        {
            TotalCount = 30,
            ReturnedCount = 12,
            PageNumber = 2,
            PageSize = 12,
            TotalPages = 3,
            HasPreviousPage = true,
            HasNextPage = true
        });
        IndexModel pageModel = CreatePageModel(catalogApplicationService);
        pageModel.PageNumber = 2;
        pageModel.PageSize = 12;

        await pageModel.OnGetAsync(CancellationToken.None);

        Assert.That(pageModel.TotalPages, Is.EqualTo(3));
        Assert.That(pageModel.HasPreviousPage, Is.True);
        Assert.That(pageModel.HasNextPage, Is.True);
    }

    [Test]
    public void BuildPageUrl_ConFiltros_ConservaEstadoDeConsulta()
    {
        FakeCatalogApplicationService catalogApplicationService = new(new CatalogQueryResultDto());
        IndexModel pageModel = CreatePageModel(catalogApplicationService);
        pageModel.SearchTerm = "teclado";
        pageModel.Brand = "Nova";
        pageModel.PageSize = 24;

        string url = pageModel.BuildPageUrl(3);

        Assert.That(url, Does.Contain("pageNumber=3"));
        Assert.That(url, Does.Contain("pageSize=24"));
        Assert.That(url, Does.Contain("searchTerm=teclado"));
    }

    private static IndexModel CreatePageModel(FakeCatalogApplicationService catalogApplicationService)
    {
        DefaultHttpContext httpContext = new();
        IndexModel pageModel = new(catalogApplicationService)
        {
            PageContext = new PageContext
            {
                HttpContext = httpContext
            },
            Url = new FakeUrlHelper()
        };

        pageModel.TempData = new TempDataDictionary(httpContext, new FakeTempDataProvider());
        return pageModel;
    }

    private sealed class FakeCatalogApplicationService(CatalogQueryResultDto queryResult) : ICatalogApplicationService
    {
        public GetCatalogProductsQuery? LastQuery { get; private set; }

        public Task<Result<CatalogQueryResultDto>> GetCatalogProductsAsync(GetCatalogProductsQuery query, CancellationToken cancellationToken = default)
        {
            LastQuery = query;
            return Task.FromResult(Result.Success(queryResult));
        }

        public Task<Result<IReadOnlyCollection<FeaturedProductDto>>> GetFeaturedProductsAsync(GetFeaturedProductsQuery query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class FakeUrlHelper : Microsoft.AspNetCore.Mvc.IUrlHelper
    {
        public Microsoft.AspNetCore.Mvc.ActionContext ActionContext { get; } = new();

        public string? Action(Microsoft.AspNetCore.Mvc.Routing.UrlActionContext actionContext) => null;
        public string? Content(string? contentPath) => contentPath;
        public bool IsLocalUrl(string? url) => !string.IsNullOrWhiteSpace(url) && url.StartsWith("/", StringComparison.Ordinal);
        public string? Link(string? routeName, object? values) => null;

        public string? RouteUrl(Microsoft.AspNetCore.Mvc.Routing.UrlRouteContext routeContext)
        {
            Microsoft.AspNetCore.Routing.RouteValueDictionary routeValues = new(routeContext.Values);
            return "/Catalog/Index?"
                + string.Join("&", routeValues
                    .Where(kvp => kvp.Value is not null)
                    .Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value!.ToString()!)}"));
        }
    }

    private sealed class FakeTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
