using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlataformaECommerce.Application.Features.Audit.DTOs;
using PlataformaECommerce.Application.Features.Audit.Queries;
using PlataformaECommerce.Application.Features.Categories.DTOs;
using PlataformaECommerce.Application.Features.Categories.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Audit;
using PlataformaECommerce.Application.Features.Products.Commands;
using PlataformaECommerce.Application.Features.Products.DTOs;
using PlataformaECommerce.Application.Features.Products.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Categories;
using PlataformaECommerce.Application.Interfaces.Services.Products;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Web.Services.Products;

namespace PlataformaECommerce.Web.Pages.Admin.Products
{
    /// <summary>
    /// Proporciona la edición administrativa de un producto dentro del backoffice.
    /// </summary>
    /// <remarks>
    /// Esta página permite modificar la información principal del catálogo reutilizando
    /// el caso de uso de actualización ya disponible en la capa Application y mostrar
    /// trazabilidad resumida de las promociones aplicadas sobre el producto.
    /// </remarks>
    public sealed class EditModel : PageModel
    {
        private const int PromotionHistoryPageSize = 10;
        private static readonly string[] PromotionAuditActions = ["product.promotion.applied", "product.promotion.removed"];
        private readonly IProductApplicationService _productApplicationService;
        private readonly ICategoryApplicationService _categoryApplicationService;
        private readonly IAuditApplicationService _auditApplicationService;
        private readonly IProductImageStorageService _productImageStorageService;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="EditModel"/>.
        /// </summary>
        /// <param name="productApplicationService">Servicio de aplicación de productos.</param>
        /// <param name="auditApplicationService">Servicio público del módulo de auditoría.</param>
        public EditModel(
            IProductApplicationService productApplicationService,
            ICategoryApplicationService categoryApplicationService,
            IAuditApplicationService auditApplicationService,
            IProductImageStorageService productImageStorageService)
        {
            _productApplicationService = productApplicationService ?? throw new ArgumentNullException(nameof(productApplicationService));
            _categoryApplicationService = categoryApplicationService ?? throw new ArgumentNullException(nameof(categoryApplicationService));
            _auditApplicationService = auditApplicationService ?? throw new ArgumentNullException(nameof(auditApplicationService));
            _productImageStorageService = productImageStorageService ?? throw new ArgumentNullException(nameof(productImageStorageService));
        }

        /// <summary>
        /// Obtiene o establece el modelo de entrada del formulario de edición.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; } = new();

        /// <summary>
        /// Obtiene el mensaje de error funcional asociado a la operación actual.
        /// </summary>
        public string? ErrorMessage { get; private set; }

        /// <summary>
        /// Obtiene el resumen promocional actual del producto cuando existe.
        /// </summary>
        public PromotionSummaryViewModel? PromotionSummary { get; private set; }

        /// <summary>
        /// Obtiene las categorías principales disponibles para clasificación.
        /// </summary>
        public IReadOnlyCollection<CategoryOptionViewModel> MainCategories { get; private set; } = Array.Empty<CategoryOptionViewModel>();

        /// <summary>
        /// Obtiene las subcategorías disponibles para clasificación.
        /// </summary>
        public IReadOnlyCollection<CategoryOptionViewModel> Subcategories { get; private set; } = Array.Empty<CategoryOptionViewModel>();

        /// <summary>
        /// Obtiene el historial resumido de promociones asociado al producto actual.
        /// </summary>
        public IReadOnlyCollection<PromotionHistoryItemViewModel> PromotionHistory { get; private set; } = Array.Empty<PromotionHistoryItemViewModel>();

        /// <summary>
        /// Obtiene la URL visible utilizada para previsualizar la imagen principal actual del producto.
        /// </summary>
        public string MainImagePreviewUrl => ProductImageDefaults.ResolveDisplayUrl(Input.Images.MainImage.ResolvePreviewUrl());

        /// <summary>
        /// Obtiene la etiqueta visible del origen actualmente asociado a la imagen principal.
        /// </summary>
        public string MainImageOriginLabel => ProductImageOriginResolver.ToDisplayName(Input.Images.MainImage.CurrentOrigin);

        /// <summary>
        /// Carga el detalle del producto solicitado para inicializar el formulario de edición.
        /// </summary>
        public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
        {
            var result = await _productApplicationService.GetProductByIdAsync(
                new GetProductByIdQuery(id),
                cancellationToken);

            if (result.IsFailure)
            {
                TempData["StatusErrorMessage"] = result.Error.Message;
                return RedirectToPage("./Index");
            }

            MapToInput(result.Value);
            await LoadCategoryOptionsAsync(cancellationToken);
            await LoadPromotionContextAsync(result.Value, cancellationToken);
            return Page();
        }

        /// <summary>
        /// Procesa la actualización administrativa del producto actual.
        /// </summary>
        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
        {
            EnsureImageContracts();

            if (!ModelState.IsValid)
            {
                await LoadCategoryOptionsAsync(cancellationToken);
                await LoadPromotionContextAsync(Input.Id, cancellationToken);
                return Page();
            }

            string? currentMainImageUrl = Input.Images.MainImage.CurrentImageUrl;
            ProductImageProcessResult imageResult = await _productImageStorageService.ProcessMainImageAsync(
                Input.Images.MainImage.UploadedFile,
                Normalize(Input.Images.MainImage.ExternalImageUrl),
                currentMainImageUrl,
                Input.Slug,
                Input.Images.MainImage.RemoveCurrentImage,
                cancellationToken);

            if (!imageResult.IsSuccess)
            {
                ErrorMessage = imageResult.ErrorMessage;
                await LoadCategoryOptionsAsync(cancellationToken);
                await LoadPromotionContextAsync(Input.Id, cancellationToken);
                return Page();
            }

            var result = await _productApplicationService.UpdateProductAsync(
                new UpdateProductCommand
                {
                    Id = Input.Id,
                    Name = Input.Name,
                    Description = Input.Description,
                    Sku = Input.Sku,
                    Price = Input.Price,
                    Currency = Input.Currency.Trim().ToUpperInvariant(),
                    Stock = Input.Stock,
                    Slug = Input.Slug,
                    MainImageUrl = imageResult.ImageUrl,
                    ImageGallery = Input.Images.GetPersistableGalleryUrls(imageResult.ImageUrl),
                    IsActive = Input.IsActive,
                    IsFeatured = Input.IsFeatured,
                    ProductType = Input.ProductType,
                    CategoryId = Input.CategoryId,
                    SubcategoryId = Input.SubcategoryId,
                    Tags = ParseTags(Input.Tags),
                    WeightKg = Input.IsPhysicalProduct ? Input.WeightKg : null,
                    HeightCm = Input.IsPhysicalProduct ? Input.HeightCm : null,
                    WidthCm = Input.IsPhysicalProduct ? Input.WidthCm : null,
                    LengthCm = Input.IsPhysicalProduct ? Input.LengthCm : null,
                    RequiresShipping = Input.IsPhysicalProduct ? Input.RequiresShipping : null,
                    FileFormat = Input.IsDigitalProduct ? Normalize(Input.FileFormat) : null,
                    FileSizeMb = Input.IsDigitalProduct ? Input.FileSizeMb : null,
                    RequiresLicense = Input.IsDigitalProduct ? Input.RequiresLicense : null
                },
                cancellationToken);

            if (result.IsFailure)
            {
                if (!string.Equals(imageResult.ImageUrl, currentMainImageUrl, StringComparison.Ordinal))
                {
                    await _productImageStorageService.DeleteIfManagedAsync(imageResult.ImageUrl, cancellationToken);
                }

                ErrorMessage = result.Error.Message;
                await LoadCategoryOptionsAsync(cancellationToken);
                await LoadPromotionContextAsync(Input.Id, cancellationToken);
                return Page();
            }

            if (!string.Equals(imageResult.ImageUrl, currentMainImageUrl, StringComparison.Ordinal))
            {
                await _productImageStorageService.DeleteIfManagedAsync(currentMainImageUrl, cancellationToken);
            }

            TempData["SuccessMessage"] = "Producto actualizado correctamente.";
            return RedirectToPage("./Index");
        }

        private void MapToInput(ProductDetailDto product)
        {
            List<ProductGalleryImageInputModel> gallery = product.ImageGallery
                .Select(imageUrl => new ProductGalleryImageInputModel
                {
                    ImageUrl = imageUrl
                })
                .ToList();

            Input = new InputModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Sku = product.Sku,
                Price = product.BasePrice,
                Currency = product.Currency,
                Stock = product.Stock,
                Slug = product.Slug,
                Images = new ProductImagesInputModel
                {
                    MainImage = new ProductMainImageInputModel
                    {
                        CurrentImageUrl = product.MainImageUrl,
                        ExternalImageUrl = product.MainImageUrl is not null && ProductImageOriginResolver.Resolve(product.MainImageUrl) == ProductImageOrigin.External
                            ? product.MainImageUrl
                            : null
                    },
                    Gallery = gallery
                },
                IsActive = product.IsActive,
                IsFeatured = product.IsFeatured,
                ProductType = product.ProductType,
                CategoryId = product.CategoryId,
                SubcategoryId = product.SubcategoryId,
                Tags = string.Join(", ", product.Tags),
                WeightKg = product.WeightKg,
                HeightCm = product.HeightCm,
                WidthCm = product.WidthCm,
                LengthCm = product.LengthCm,
                RequiresShipping = product.RequiresShipping,
                FileFormat = product.FileFormat,
                FileSizeMb = product.FileSizeMb,
                RequiresLicense = product.RequiresLicense
            };

            EnsureImageContracts();
        }

        private async Task LoadPromotionContextAsync(ProductDetailDto product, CancellationToken cancellationToken)
        {
            PromotionSummary = product.HasPromotion
                ? new PromotionSummaryViewModel
                {
                    BasePrice = product.BasePrice,
                    PromotionalPrice = product.PromotionalPrice,
                    CurrentPrice = product.Price,
                    Currency = product.Currency,
                    CurrentDiscountPercentage = product.CurrentDiscountPercentage
                }
                : null;

            await LoadPromotionHistoryAsync(product.Id, cancellationToken);
        }

        private async Task LoadPromotionContextAsync(Guid productId, CancellationToken cancellationToken)
        {
            if (productId == Guid.Empty)
            {
                PromotionSummary = null;
                PromotionHistory = Array.Empty<PromotionHistoryItemViewModel>();
                return;
            }

            var result = await _productApplicationService.GetProductByIdAsync(
                new GetProductByIdQuery(productId),
                cancellationToken);

            if (result.IsFailure)
            {
                PromotionSummary = null;
                PromotionHistory = Array.Empty<PromotionHistoryItemViewModel>();
                return;
            }

            await LoadPromotionContextAsync(result.Value, cancellationToken);
        }

        private async Task LoadCategoryOptionsAsync(CancellationToken cancellationToken)
        {
            var result = await _categoryApplicationService.GetCategoriesAsync(
                new GetCategoriesQuery { OnlyActive = true },
                cancellationToken);

            if (result.IsFailure)
            {
                MainCategories = Array.Empty<CategoryOptionViewModel>();
                Subcategories = Array.Empty<CategoryOptionViewModel>();
                ErrorMessage ??= result.Error.Message;
                return;
            }

            IReadOnlyCollection<CategoryDto> categories = result.Value;
            MainCategories = categories
                .Where(category => category.IsRootCategory)
                .Select(MapCategoryOption)
                .ToArray();

            Subcategories = categories
                .Where(category => category.IsSubcategory)
                .Select(MapCategoryOption)
                .ToArray();
        }

        private async Task LoadPromotionHistoryAsync(Guid productId, CancellationToken cancellationToken)
        {
            var auditResult = await _auditApplicationService.GetAuditTrailAsync(
                new GetAuditTrailQuery
                {
                    AggregateId = productId,
                    AggregateType = "Producto",
                    Module = "Products",
                    PageNumber = 1,
                    PageSize = PromotionHistoryPageSize,
                    SortDescending = true
                },
                cancellationToken);

            if (auditResult.IsFailure)
            {
                PromotionHistory = Array.Empty<PromotionHistoryItemViewModel>();
                return;
            }

            PromotionHistory = auditResult.Value.Items
                .Where(item => PromotionAuditActions.Contains(item.Action, StringComparer.OrdinalIgnoreCase))
                .Select(MapPromotionHistory)
                .ToArray();
        }

        private static PromotionHistoryItemViewModel MapPromotionHistory(AuditEntryDto entry)
        {
            return new PromotionHistoryItemViewModel
            {
                Action = entry.Action.Equals("product.promotion.removed", StringComparison.OrdinalIgnoreCase)
                    ? "Promoción retirada"
                    : "Promoción aplicada",
                Detail = entry.Detail,
                PerformedBy = string.IsNullOrWhiteSpace(entry.PerformedBy) ? "Sistema" : entry.PerformedBy,
                OccurredAtUtc = entry.OccurredAtUtc,
                DiscountPercentage = TryGetDecimal(entry.Metadata, "discountPercentage"),
                FromPrice = TryGetDecimal(entry.Metadata, "previousPrice") ?? TryGetDecimal(entry.Metadata, "previousPromotionalPrice"),
                ToPrice = TryGetDecimal(entry.Metadata, "newPrice") ?? TryGetDecimal(entry.Metadata, "restoredBasePrice"),
                Currency = TryGetCurrency(entry.Metadata),
                IsRemoval = entry.Action.Equals("product.promotion.removed", StringComparison.OrdinalIgnoreCase)
            };
        }

        private static decimal? TryGetDecimal(IReadOnlyDictionary<string, string> metadata, string key)
        {
            if (!metadata.TryGetValue(key, out string? rawValue) || string.IsNullOrWhiteSpace(rawValue))
            {
                return null;
            }

            return decimal.TryParse(rawValue, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal parsedValue)
                ? parsedValue
                : null;
        }

        private static string? TryGetCurrency(IReadOnlyDictionary<string, string> metadata)
        {
            return metadata.TryGetValue("currency", out string? currency) && !string.IsNullOrWhiteSpace(currency)
                ? currency.Trim().ToUpperInvariant()
                : null;
        }

        private static CategoryOptionViewModel MapCategoryOption(CategoryDto category)
        {
            return new CategoryOptionViewModel
            {
                Id = category.Id,
                Name = category.Name,
                ParentCategoryId = category.ParentCategoryId
            };
        }

        private static IReadOnlyCollection<string> ParseTags(string? tags)
        {
            if (string.IsNullOrWhiteSpace(tags))
            {
                return Array.Empty<string>();
            }

            return tags
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private void EnsureImageContracts()
        {
            Input.Images ??= new ProductImagesInputModel();
            Input.Images.EnsureGallerySlots();
        }

        /// <summary>
        /// Representa el resumen promocional vigente del producto.
        /// </summary>
        public sealed class PromotionSummaryViewModel
        {
            /// <summary>
            /// Precio base del producto.
            /// </summary>
            public decimal BasePrice { get; init; }

            /// <summary>
            /// Precio promocional vigente del producto.
            /// </summary>
            public decimal? PromotionalPrice { get; init; }

            /// <summary>
            /// Precio actualmente visible del producto.
            /// </summary>
            public decimal CurrentPrice { get; init; }

            /// <summary>
            /// Moneda del producto.
            /// </summary>
            public string Currency { get; init; } = string.Empty;

            /// <summary>
            /// Porcentaje de descuento promocional vigente.
            /// </summary>
            public decimal? CurrentDiscountPercentage { get; init; }
        }

        /// <summary>
        /// Representa un evento resumido de actividad promocional en auditoría.
        /// </summary>
        public sealed class PromotionHistoryItemViewModel
        {
            /// <summary>
            /// Acción promocional mostrada al administrador.
            /// </summary>
            public string Action { get; init; } = string.Empty;

            /// <summary>
            /// Detalle descriptivo del evento.
            /// </summary>
            public string Detail { get; init; } = string.Empty;

            /// <summary>
            /// Actor visible del evento.
            /// </summary>
            public string PerformedBy { get; init; } = string.Empty;

            /// <summary>
            /// Fecha UTC en la que ocurrió el evento.
            /// </summary>
            public DateTime OccurredAtUtc { get; init; }

            /// <summary>
            /// Porcentaje de descuento aplicado cuando exista.
            /// </summary>
            public decimal? DiscountPercentage { get; init; }

            /// <summary>
            /// Precio anterior al cambio promocional.
            /// </summary>
            public decimal? FromPrice { get; init; }

            /// <summary>
            /// Precio resultante del cambio promocional.
            /// </summary>
            public decimal? ToPrice { get; init; }

            /// <summary>
            /// Moneda asociada al cambio promocional.
            /// </summary>
            public string? Currency { get; init; }

            /// <summary>
            /// Indica si el evento corresponde al retiro de una promoción.
            /// </summary>
            public bool IsRemoval { get; init; }
        }

        /// <summary>
        /// Representa el modelo de entrada del formulario administrativo de edición.
        /// </summary>
        public sealed class InputModel
        {
            /// <summary>
            /// Identificador del producto a modificar.
            /// </summary>
            public Guid Id { get; set; }

            /// <summary>
            /// Nombre comercial del producto.
            /// </summary>
            [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
            public string Name { get; set; } = string.Empty;

            /// <summary>
            /// Descripción del producto.
            /// </summary>
            [Required(ErrorMessage = "La descripción del producto es obligatoria.")]
            public string Description { get; set; } = string.Empty;

            /// <summary>
            /// SKU del producto.
            /// </summary>
            [Required(ErrorMessage = "El SKU del producto es obligatorio.")]
            public string Sku { get; set; } = string.Empty;

            /// <summary>
            /// Precio unitario del producto.
            /// </summary>
            [Range(
                typeof(decimal),
                "0.01",
                "79228162514264337593543950335",
                ParseLimitsInInvariantCulture = true,
                ConvertValueInInvariantCulture = true,
                ErrorMessage = "El precio debe ser mayor que cero.")]
            public decimal Price { get; set; }

            /// <summary>
            /// Código de moneda del producto.
            /// </summary>
            [Required(ErrorMessage = "La moneda es obligatoria.")]
            public string Currency { get; set; } = "COP";

            /// <summary>
            /// Stock del producto.
            /// </summary>
            [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
            public int Stock { get; set; }

            /// <summary>
            /// Slug del producto.
            /// </summary>
            [Required(ErrorMessage = "El slug del producto es obligatorio.")]
            public string Slug { get; set; } = string.Empty;

            /// <summary>
            /// Contrato de imágenes utilizado por el formulario de edición.
            /// </summary>
            public ProductImagesInputModel Images { get; set; } = new();

            /// <summary>
            /// Estado actual del producto.
            /// </summary>
            public bool IsActive { get; set; }

            /// <summary>
            /// Marca actual de destacado del producto.
            /// </summary>
            public bool IsFeatured { get; set; }

            /// <summary>
            /// Tipo funcional del producto.
            /// </summary>
            public TipoProducto ProductType { get; set; }

            /// <summary>
            /// Categoría principal seleccionada para el producto.
            /// </summary>
            public Guid? CategoryId { get; set; }

            /// <summary>
            /// Subcategoría seleccionada para el producto.
            /// </summary>
            public Guid? SubcategoryId { get; set; }

            /// <summary>
            /// Etiquetas del producto expresadas como una lista separada por comas.
            /// </summary>
            public string? Tags { get; set; }

            /// <summary>
            /// Peso del producto físico en kilogramos.
            /// </summary>
            public decimal? WeightKg { get; set; }

            /// <summary>
            /// Alto del producto físico en centímetros.
            /// </summary>
            public decimal? HeightCm { get; set; }

            /// <summary>
            /// Ancho del producto físico en centímetros.
            /// </summary>
            public decimal? WidthCm { get; set; }

            /// <summary>
            /// Largo del producto físico en centímetros.
            /// </summary>
            public decimal? LengthCm { get; set; }

            /// <summary>
            /// Indica si el producto físico requiere envío.
            /// </summary>
            public bool? RequiresShipping { get; set; }

            /// <summary>
            /// Formato del archivo digital.
            /// </summary>
            public string? FileFormat { get; set; }

            /// <summary>
            /// Tamaño del archivo digital en megabytes.
            /// </summary>
            public decimal? FileSizeMb { get; set; }

            /// <summary>
            /// Indica si el producto digital requiere licencia.
            /// </summary>
            public bool? RequiresLicense { get; set; }

            /// <summary>
            /// Indica si el formulario corresponde a un producto físico.
            /// </summary>
            public bool IsPhysicalProduct => ProductType == TipoProducto.Fisico;

            /// <summary>
            /// Indica si el formulario corresponde a un producto digital.
            /// </summary>
            public bool IsDigitalProduct => ProductType == TipoProducto.Digital;
        }

        /// <summary>
        /// Representa una opción de categoría consumida por la UI administrativa.
        /// </summary>
        public sealed class CategoryOptionViewModel
        {
            /// <summary>
            /// Identificador de la categoría.
            /// </summary>
            public Guid Id { get; init; }

            /// <summary>
            /// Nombre visible de la categoría.
            /// </summary>
            public string Name { get; init; } = string.Empty;

            /// <summary>
            /// Identificador de la categoría padre cuando se trata de una subcategoría.
            /// </summary>
            public Guid? ParentCategoryId { get; init; }
        }
    }
}
