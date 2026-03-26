using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlataformaECommerce.Application.Features.Catalog.DTOs;
using PlataformaECommerce.Application.Features.Catalog.Queries;
using PlataformaECommerce.Application.Features.Categories.DTOs;
using PlataformaECommerce.Application.Features.Categories.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Catalog;
using PlataformaECommerce.Application.Interfaces.Services.Categories;
using PlataformaECommerce.Web.Services.Products;

namespace PlataformaECommerce.Web.Pages;

/// <summary>
/// Orquesta la experiencia comercial de la página principal del storefront.
/// </summary>
public sealed class IndexModel : PageModel
{
    private const int HomeFeaturedProductsCount = 4;
    private const int HomeCategoriesCount = 4;
    private readonly ICatalogApplicationService _catalogApplicationService;
    private readonly ICategoryApplicationService _categoryApplicationService;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="IndexModel"/>.
    /// </summary>
    public IndexModel(
        ICatalogApplicationService catalogApplicationService,
        ICategoryApplicationService categoryApplicationService)
    {
        _catalogApplicationService = catalogApplicationService ?? throw new ArgumentNullException(nameof(catalogApplicationService));
        _categoryApplicationService = categoryApplicationService ?? throw new ArgumentNullException(nameof(categoryApplicationService));
    }

    /// <summary>
    /// Productos destacados visibles en la portada comercial.
    /// </summary>
    public IReadOnlyCollection<HomeFeaturedProductViewModel> FeaturedProducts { get; private set; } = Array.Empty<HomeFeaturedProductViewModel>();

    /// <summary>
    /// Categorías visibles en la portada comercial.
    /// </summary>
    public IReadOnlyCollection<HomeCategoryViewModel> FeaturedCategories { get; private set; } = Array.Empty<HomeCategoryViewModel>();

    /// <summary>
    /// Mensaje funcional asociado a la carga de la página principal.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Indica si la sesión autenticada corresponde a un cliente que puede comprar.
    /// </summary>
    public bool CanPurchaseAsCustomer => User.Identity?.IsAuthenticated == true && User.IsInRole("Cliente");

    /// <summary>
    /// Carga la información comercial de la portada.
    /// </summary>
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadCategoriesAsync(cancellationToken);
        await LoadFeaturedProductsAsync(cancellationToken);
    }

    private async Task LoadCategoriesAsync(CancellationToken cancellationToken)
    {
        var categoriesResult = await _categoryApplicationService.GetCategoriesAsync(
            new GetCategoriesQuery
            {
                OnlyActive = true,
                RootOnly = true
            },
            cancellationToken);

        if (categoriesResult.IsFailure)
        {
            ErrorMessage ??= categoriesResult.Error.Message;
            FeaturedCategories = Array.Empty<HomeCategoryViewModel>();
            return;
        }

        FeaturedCategories = categoriesResult.Value
            .Where(category => category.IsRootCategory)
            .Take(HomeCategoriesCount)
            .Select(MapCategory)
            .ToArray();
    }

    private async Task LoadFeaturedProductsAsync(CancellationToken cancellationToken)
    {
        var featuredResult = await _catalogApplicationService.GetFeaturedProductsAsync(
            new GetFeaturedProductsQuery(HomeFeaturedProductsCount)
            {
                OnlyAvailable = true,
                OnlyWithStock = true,
                IncludeVisualAssets = true,
                Source = "Web.Home",
                Placement = "home-grid"
            },
            cancellationToken);

        if (featuredResult.IsSuccess && featuredResult.Value.Count > 0)
        {
            FeaturedProducts = featuredResult.Value
                .Select(MapFeaturedProduct)
                .ToArray();
            return;
        }

        var catalogResult = await _catalogApplicationService.GetCatalogProductsAsync(
            new GetCatalogProductsQuery
            {
                IsActive = true,
                IsAvailable = true,
                HasStock = true,
                SortBy = "createdAt",
                SortDescending = true,
                PageSize = HomeFeaturedProductsCount,
                IncludeImageGallery = true,
                Source = "Web.Home",
                RequestedByUserId = GetAuthenticatedUserId()
            },
            cancellationToken);

        if (catalogResult.IsFailure)
        {
            ErrorMessage ??= featuredResult.IsFailure
                ? featuredResult.Error.Message
                : catalogResult.Error.Message;
            FeaturedProducts = Array.Empty<HomeFeaturedProductViewModel>();
            return;
        }

        FeaturedProducts = catalogResult.Value
            .Select(MapCatalogProduct)
            .ToArray();
    }

    private Guid? GetAuthenticatedUserId()
    {
        string? rawUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(rawUserId, out Guid userId)
            ? userId
            : null;
    }

    private static HomeCategoryViewModel MapCategory(CategoryDto category)
    {
        return new HomeCategoryViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Description = string.IsNullOrWhiteSpace(category.Description)
                ? "Explora productos disponibles dentro de esta categoría destacada."
                : category.Description,
            AccentCssClass = ResolveCategoryAccentCssClass(category.Name)
        };
    }

    private static HomeFeaturedProductViewModel MapFeaturedProduct(FeaturedProductDto product)
    {
        IReadOnlyCollection<string> imageUrls = ProductImageDefaults.ResolveDisplayGallery(product.MainImageUrl, product.ImageUrls);

        return new HomeFeaturedProductViewModel
        {
            Id = product.Id,
            Name = product.Name,
            CategoryName = product.CategoryName ?? (product.ProductType == Domain.Enums.TipoProducto.Digital ? "Productos digitales" : "Productos físicos"),
            Price = product.Price,
            PreviousPrice = product.PreviousPrice,
            Currency = product.Currency,
            BadgeText = string.IsNullOrWhiteSpace(product.BadgeText) ? "Destacado" : product.BadgeText!,
            MainImageUrl = imageUrls.First(),
            ProductUrl = string.IsNullOrWhiteSpace(product.ProductUrl)
                ? $"/Catalog/Details/{product.Id}"
                : product.ProductUrl!
        };
    }

    private static HomeFeaturedProductViewModel MapCatalogProduct(CatalogProductDto product)
    {
        IReadOnlyCollection<string> imageUrls = ProductImageDefaults.ResolveDisplayGallery(product.MainImageUrl, product.ImageUrls);

        return new HomeFeaturedProductViewModel
        {
            Id = product.Id,
            Name = product.Name,
            CategoryName = product.CategoryName ?? (product.ProductType == Domain.Enums.TipoProducto.Digital ? "Productos digitales" : "Productos físicos"),
            Price = product.Price,
            PreviousPrice = product.PreviousPrice,
            Currency = product.Currency,
            BadgeText = product.IsNew ? "Nuevo" : product.IsFeatured ? "Destacado" : "Disponible",
            MainImageUrl = imageUrls.First(),
            ProductUrl = string.IsNullOrWhiteSpace(product.Slug)
                ? $"/Catalog/Details/{product.Id}"
                : $"/Catalog/Details/{product.Id}"
        };
    }

    private static string ResolveCategoryAccentCssClass(string categoryName)
    {
        string normalizedCategoryName = categoryName.Trim().ToLowerInvariant();

        if (normalizedCategoryName.Contains("tec") || normalizedCategoryName.Contains("digit"))
        {
            return "placeholder-tech";
        }

        if (normalizedCategoryName.Contains("hog") || normalizedCategoryName.Contains("casa"))
        {
            return "placeholder-home";
        }

        if (normalizedCategoryName.Contains("mod") || normalizedCategoryName.Contains("ropa"))
        {
            return "placeholder-fashion";
        }

        if (normalizedCategoryName.Contains("deport") || normalizedCategoryName.Contains("fit"))
        {
            return "placeholder-sport";
        }

        return "placeholder-tech";
    }

    /// <summary>
    /// Proyección visual de una categoría destacada en la portada.
    /// </summary>
    public sealed class HomeCategoryViewModel
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string AccentCssClass { get; init; } = "placeholder-tech";
    }

    /// <summary>
    /// Proyección visual de un producto destacado en la portada.
    /// </summary>
    public sealed class HomeFeaturedProductViewModel
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string CategoryName { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public decimal? PreviousPrice { get; init; }
        public string Currency { get; init; } = string.Empty;
        public string BadgeText { get; init; } = string.Empty;
        public string MainImageUrl { get; init; } = ProductImageDefaults.PlaceholderImageUrl;
        public string ProductUrl { get; init; } = string.Empty;
    }
}
