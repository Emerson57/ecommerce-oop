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
/// Esta página utiliza el módulo de catálogo comercial para exponer una experiencia pública
/// consistente con el backend, aprovechando sus filtros y proyecciones especializadas.
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

    /// <summary>
    /// Identificador opcional de la categoría principal a filtrar.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public Guid? CategoryId { get; set; }

    /// <summary>
    /// Tipo de producto aplicado al catálogo cuando el visitante desea segmentar la vitrina.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public TipoProducto? ProductType { get; set; }

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
                SearchTerm = string.IsNullOrWhiteSpace(SearchTerm) ? null : SearchTerm.Trim(),
                CategoryId = CategoryId,
                ProductType = ProductType,
                IsActive = true,
                IsAvailable = true,
                SortBy = "relevance",
                SortDescending = true,
                Source = CatalogSource,
                RequestedByUserId = GetAuthenticatedUserId(),
                ExternalReference = CatalogSource
            },
            cancellationToken);

        if (result.IsFailure)
        {
            ErrorMessage = result.Error.Message;
            Products = Array.Empty<CatalogProductViewModel>();
            return;
        }

        Products = result.Value
            .Select(Map)
            .ToArray();
    }

    private Guid? GetAuthenticatedUserId()
    {
        string? rawUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(rawUserId, out Guid userId)
            ? userId
            : null;
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
            IsAvailable = product.IsAvailable,
            HasStock = product.HasStock
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
        public bool IsAvailable { get; init; }
        public bool HasStock { get; init; }
        public string AvailabilityLabel => ProductTypeLabel == "Digital"
            ? "Entrega digital"
            : HasStock
                ? $"Stock: {Stock}"
                : IsAvailable
                    ? "Disponible"
                    : "No disponible";
    }
}
