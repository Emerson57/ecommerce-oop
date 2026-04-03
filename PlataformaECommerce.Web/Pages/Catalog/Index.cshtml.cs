using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlataformaECommerce.Application.Features.Products.DTOs;
using PlataformaECommerce.Application.Features.Products.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Products;
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
    private readonly IProductQueryService _productQueryService;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="IndexModel"/>.
    /// </summary>
    public IndexModel(IProductQueryService productQueryService)
    {
        _productQueryService = productQueryService ?? throw new ArgumentNullException(nameof(productQueryService));
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
        var result = await _productQueryService.GetProductsAsync(
            new GetProductsQuery
            {
                SearchTerm = string.IsNullOrWhiteSpace(SearchTerm) ? null : SearchTerm.Trim(),
                CategoryId = CategoryId,
                IsActive = true,
                HasStock = true,
                SortBy = "createdAt",
                SortDescending = true,
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

        Products = result.Value.Items
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

    private static CatalogProductViewModel Map(ProductDto product)
    {
        IReadOnlyCollection<string> imageUrls = ProductImageDefaults.ResolveDisplayGallery(product.MainImageUrl, product.ImageGallery);

        return new CatalogProductViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Currency = product.Currency,
            Stock = product.Stock,
            IsFeatured = product.IsFeatured,
            HasPromotion = product.HasPromotion,
            MainImageUrl = imageUrls.First(),
            ImageUrls = imageUrls,
            ProductTypeLabel = product.ProductType == PlataformaECommerce.Domain.Enums.TipoProducto.Digital ? "Digital" : "Físico"
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
    }
}
