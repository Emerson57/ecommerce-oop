using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlataformaECommerce.Application.Features.Catalog.DTOs;
using PlataformaECommerce.Application.Features.Catalog.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Catalog;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Web.Services.Products;

namespace PlataformaECommerce.Web.Pages.Catalog;

/// <summary>
/// Proporciona la vista pública del catálogo de productos del e-commerce.
/// </summary>
/// <remarks>
/// Esta página reutiliza el servicio de aplicación de productos para exponer un catálogo navegable
/// desde Razor Pages, manteniendo filtros básicos y acceso directo al circuito comercial del cliente.
/// </remarks>
public sealed class IndexModel : PageModel
{
    private const string CatalogSource = "Web.Catalog.Index";
    private readonly ICatalogApplicationService _catalogApplicationService;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="IndexModel"/>.
    /// </summary>
    public IndexModel(ICatalogApplicationService catalogApplicationService)
    {
        _catalogApplicationService = catalogApplicationService ?? throw new ArgumentNullException(nameof(catalogApplicationService));
    }

    /// <summary>
    /// Productos visibles del catálogo.
    /// </summary>
    public IReadOnlyCollection<CatalogProductViewModel> Products { get; private set; } = Array.Empty<CatalogProductViewModel>();

    /// <summary>
    /// Texto libre de búsqueda aplicado al catálogo.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Brand { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? CategoryName { get; set; }

    [BindProperty(SupportsGet = true)]
    public TipoProducto? ProductType { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool? IsFeatured { get; set; }

    [BindProperty(SupportsGet = true)]
    public decimal? MinPrice { get; set; }

    [BindProperty(SupportsGet = true)]
    public decimal? MaxPrice { get; set; }

    [BindProperty(SupportsGet = true)]
    public string SortBy { get; set; } = "relevance";

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 12;

    public int TotalCount { get; private set; }
    public int TotalPages { get; private set; }
    public bool HasPreviousPage { get; private set; }
    public bool HasNextPage { get; private set; }
    public IReadOnlyCollection<int> VisiblePageNumbers { get; private set; } = Array.Empty<int>();

    /// <summary>
    /// Mensaje funcional asociado a la consulta del catálogo.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Indica si la sesión autenticada corresponde a un cliente que puede comprar.
    /// </summary>
    public bool CanPurchaseAsCustomer => User.Identity?.IsAuthenticated == true && User.IsInRole("Cliente");

    /// <summary>
    /// Carga los productos públicos visibles del catálogo.
    /// </summary>
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var result = await _catalogApplicationService.GetCatalogProductsAsync(
            new GetCatalogProductsQuery
            {
                SearchTerm = Normalize(SearchTerm),
                Brand = Normalize(Brand),
                CategoryName = Normalize(CategoryName),
                ProductType = ProductType,
                IsFeatured = IsFeatured,
                IsAvailable = true,
                HasStock = true,
                MinPrice = MinPrice,
                MaxPrice = MaxPrice,
                SortBy = NormalizeSortBy(SortBy),
                SortDescending = ShouldSortDescending(SortBy),
                PageNumber = PageNumber,
                PageSize = PageSize,
                RequestedByUserId = GetAuthenticatedUserId(),
                Source = CatalogSource,
                ExternalReference = CatalogSource
            },
            cancellationToken);

        if (result.IsFailure)
        {
            ErrorMessage = result.Error.Message;
            Products = Array.Empty<CatalogProductViewModel>();
            return;
        }

        TotalCount = result.Value.TotalCount;
        PageNumber = result.Value.PageNumber;
        PageSize = result.Value.PageSize;
        TotalPages = result.Value.TotalPages;
        HasPreviousPage = result.Value.HasPreviousPage;
        HasNextPage = result.Value.HasNextPage;
        VisiblePageNumbers = BuildVisiblePageNumbers(PageNumber, TotalPages);

        Products = result.Value.Items
            .Select(Map)
            .ToArray();
    }

    public string BuildPageUrl(int pageNumber)
    {
        return Url.RouteUrl(
            new
            {
                page = "/Catalog/Index",
                searchTerm = SearchTerm,
                brand = Brand,
                categoryName = CategoryName,
                productType = ProductType,
                isFeatured = IsFeatured,
                minPrice = MinPrice,
                maxPrice = MaxPrice,
                sortBy = SortBy,
                pageNumber,
                pageSize = PageSize
            }) ?? "/Catalog/Index";
    }

    private Guid? GetAuthenticatedUserId()
    {
        string? rawUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(rawUserId, out Guid userId)
            ? userId
            : null;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string NormalizeSortBy(string? sortBy)
    {
        return string.IsNullOrWhiteSpace(sortBy) ? "relevance" : sortBy.Trim();
    }

    private static bool ShouldSortDescending(string? sortBy)
    {
        string normalizedSortBy = NormalizeSortBy(sortBy);
        return !normalizedSortBy.Equals("name", StringComparison.OrdinalIgnoreCase)
            && !normalizedSortBy.Equals("price", StringComparison.OrdinalIgnoreCase);
    }

    private static IReadOnlyCollection<int> BuildVisiblePageNumbers(int pageNumber, int totalPages)
    {
        if (totalPages <= 0)
        {
            return Array.Empty<int>();
        }

        int startPage = Math.Max(1, pageNumber - 2);
        int endPage = Math.Min(totalPages, pageNumber + 2);

        return Enumerable.Range(startPage, endPage - startPage + 1).ToArray();
    }

    private static CatalogProductViewModel Map(CatalogProductDto product)
    {
        IReadOnlyCollection<string> imageUrls = ProductImageDefaults.ResolveDisplayGallery(product.MainImageUrl, product.ImageUrls);

        return new CatalogProductViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Currency = product.Currency,
            Stock = product.AvailableStock ?? 0,
            IsFeatured = product.IsFeatured,
            HasPromotion = product.IsOnSale,
            MainImageUrl = imageUrls.First(),
            ImageUrls = imageUrls,
            ProductTypeLabel = product.ProductType == TipoProducto.Digital ? "Digital" : "Físico",
            Brand = product.Brand,
            CategoryName = product.CategoryName
        };
    }

    /// <summary>
    /// Proyección resumida del producto mostrada en el catálogo.
    /// </summary>
    public sealed class CatalogProductViewModel
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public string Currency { get; init; } = string.Empty;
        public int Stock { get; init; }
        public bool IsFeatured { get; init; }
        public bool HasPromotion { get; init; }
        public string? MainImageUrl { get; init; }
        public IReadOnlyCollection<string> ImageUrls { get; init; } = Array.Empty<string>();
        public int AdditionalImageCount => Math.Max(0, ImageUrls.Count - 1);
        public string ProductTypeLabel { get; init; } = string.Empty;
        public string? Brand { get; init; }
        public string? CategoryName { get; init; }
    }
}
