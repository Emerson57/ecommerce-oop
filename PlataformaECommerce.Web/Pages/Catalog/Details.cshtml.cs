using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlataformaECommerce.Application.Features.Products.DTOs;
using PlataformaECommerce.Application.Features.Products.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Products;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Web.Services.Products;

namespace PlataformaECommerce.Web.Pages.Catalog;

/// <summary>
/// Proporciona la ficha pública detallada de un producto del catálogo.
/// </summary>
/// <remarks>
/// Esta página consume el servicio de productos para proyectar información completa del producto
/// y facilitar la transición del catálogo al carrito del cliente autenticado.
/// </remarks>
public sealed class DetailsModel : PageModel
{
    private const string CatalogDetailsSource = "Web.Catalog.Details";
    private readonly IProductQueryService _productQueryService;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="DetailsModel"/>.
    /// </summary>
    public DetailsModel(IProductQueryService productQueryService)
    {
        _productQueryService = productQueryService ?? throw new ArgumentNullException(nameof(productQueryService));
    }

    /// <summary>
    /// Producto detallado mostrado al usuario.
    /// </summary>
    public ProductDetailsViewModel Product { get; private set; } = new();

    /// <summary>
    /// Captura la cantidad deseada al agregar el producto al carrito.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public AddToCartInputModel Input { get; set; } = new();

    /// <summary>
    /// Mensaje funcional asociado a la consulta del detalle.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Indica si la sesión autenticada corresponde a un cliente que puede comprar.
    /// </summary>
    public bool CanPurchaseAsCustomer => User.Identity?.IsAuthenticated == true && User.IsInRole("Cliente");

    /// <summary>
    /// Carga el detalle de un producto específico del catálogo.
    /// </summary>
    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return RedirectToPage("/Catalog/Index");
        }

        var result = await _productQueryService.GetProductByIdAsync(
            new GetProductByIdQuery(id)
            {
                RequestedByUserId = GetAuthenticatedUserId(),
                ExternalReference = CatalogDetailsSource
            },
            cancellationToken);

        if (result.IsFailure)
        {
            ErrorMessage = result.Error.Message;
            return Page();
        }

        Product = Map(result.Value);
        if (Input.Quantity <= 0)
        {
            Input.Quantity = 1;
        }

        return Page();
    }

    private Guid? GetAuthenticatedUserId()
    {
        string? rawUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(rawUserId, out Guid userId)
            ? userId
            : null;
    }

    private static ProductDetailsViewModel Map(ProductDetailDto product)
    {
        IReadOnlyCollection<string> imageUrls = ProductImageDefaults.ResolveDisplayGallery(product.MainImageUrl, product.ImageGallery);

        return new ProductDetailsViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Sku = product.Sku,
            Price = product.Price,
            Currency = product.Currency,
            Stock = product.Stock,
            MainImageUrl = imageUrls.First(),
            ImageUrls = imageUrls,
            IsAvailable = product.IsAvailable,
            HasPromotion = product.HasPromotion,
            ProductTypeLabel = product.ProductType == TipoProducto.Digital ? "Digital" : "Físico",
            CategoryName = ResolveCategoryLabel(product),
            Tags = product.Tags
        };
    }

    private static string? ResolveCategoryLabel(ProductDetailDto product)
    {
        if (product.SubcategoryId.HasValue)
        {
            return "Subcategoría asignada";
        }

        return product.CategoryId.HasValue
            ? "Categoría asignada"
            : null;
    }

    /// <summary>
    /// Proyección detallada del producto mostrada en la ficha pública.
    /// </summary>
    public sealed class ProductDetailsViewModel
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Sku { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public string Currency { get; init; } = string.Empty;
        public int Stock { get; init; }
        public string MainImageUrl { get; init; } = ProductImageDefaults.PlaceholderImageUrl;
        public IReadOnlyCollection<string> ImageUrls { get; init; } = Array.Empty<string>();
        public bool IsAvailable { get; init; }
        public bool HasPromotion { get; init; }
        public string ProductTypeLabel { get; init; } = string.Empty;
        public string? CategoryName { get; init; }
        public IReadOnlyCollection<string> Tags { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Captura la cantidad deseada del producto antes de enviarlo al carrito.
    /// </summary>
    public sealed class AddToCartInputModel
    {
        [Display(Name = "Cantidad")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero.")]
        public int Quantity { get; set; } = 1;
    }
}
