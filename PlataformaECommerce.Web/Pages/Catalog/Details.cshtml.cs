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
    private readonly IProductApplicationService _productApplicationService;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="DetailsModel"/>.
    /// </summary>
    public DetailsModel(IProductApplicationService productApplicationService)
    {
        _productApplicationService = productApplicationService ?? throw new ArgumentNullException(nameof(productApplicationService));
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

        var result = await _productApplicationService.GetProductByIdAsync(
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
        decimal? discountAmount = product.HasPromotion && product.BasePrice > product.Price
            ? decimal.Round(product.BasePrice - product.Price, 2, MidpointRounding.AwayFromZero)
            : null;

        return new ProductDetailsViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Sku = product.Sku,
            Price = product.Price,
            BasePrice = product.BasePrice,
            PromotionalPrice = product.PromotionalPrice,
            Currency = product.Currency,
            Stock = product.Stock,
            MainImageUrl = imageUrls.First(),
            ImageUrls = imageUrls,
            IsAvailable = product.IsAvailable,
            HasPromotion = product.HasPromotion,
            DiscountPercentage = product.CurrentDiscountPercentage,
            DiscountAmount = discountAmount,
            ProductTypeLabel = product.ProductType == TipoProducto.Digital ? "Digital" : "Físico",
            IsDigitalProduct = product.IsDigitalProduct,
            IsPhysicalProduct = product.IsPhysicalProduct,
            CategoryName = ResolveCategoryLabel(product),
            Tags = product.Tags,
            WeightKg = product.WeightKg,
            HeightCm = product.HeightCm,
            WidthCm = product.WidthCm,
            LengthCm = product.LengthCm,
            RequiresShipping = product.RequiresShipping,
            FileFormat = product.FileFormat,
            FileSizeMb = product.FileSizeMb,
            RequiresLicense = product.RequiresLicense
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
        public decimal BasePrice { get; init; }
        public decimal? PromotionalPrice { get; init; }
        public string Currency { get; init; } = string.Empty;
        public int Stock { get; init; }
        public string MainImageUrl { get; init; } = ProductImageDefaults.PlaceholderImageUrl;
        public IReadOnlyCollection<string> ImageUrls { get; init; } = Array.Empty<string>();
        public bool IsAvailable { get; init; }
        public bool HasPromotion { get; init; }
        public decimal? DiscountPercentage { get; init; }
        public decimal? DiscountAmount { get; init; }
        public string ProductTypeLabel { get; init; } = string.Empty;
        public bool IsDigitalProduct { get; init; }
        public bool IsPhysicalProduct { get; init; }
        public string? CategoryName { get; init; }
        public IReadOnlyCollection<string> Tags { get; init; } = Array.Empty<string>();
        public decimal? WeightKg { get; init; }
        public decimal? HeightCm { get; init; }
        public decimal? WidthCm { get; init; }
        public decimal? LengthCm { get; init; }
        public bool? RequiresShipping { get; init; }
        public string? FileFormat { get; init; }
        public decimal? FileSizeMb { get; init; }
        public bool? RequiresLicense { get; init; }
        public string AvailabilityTitle => IsDigitalProduct
            ? IsAvailable
                ? "Disponible para entrega digital"
                : "Entrega digital no disponible"
            : IsAvailable
                ? "Disponible para despacho"
                : "No disponible para envío";
        public string AvailabilityDescription => IsDigitalProduct
            ? IsAvailable
                ? (RequiresLicense == true
                    ? "La compra se entrega por canal digital y requiere activación con licencia."
                    : "La compra se entrega por canal digital sin logística física adicional.")
                : "Este producto digital no puede entregarse comercialmente en este momento."
            : IsAvailable
                ? $"Stock actual: {Stock} unidad(es) listas para preparación y envío."
                : "El producto físico no cuenta con disponibilidad suficiente para iniciar una compra ahora mismo.";
        public string CommercialBadge => HasPromotion
            ? "Promoción activa"
            : IsDigitalProduct
                ? "Entrega digital"
                : "Despacho físico";
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
