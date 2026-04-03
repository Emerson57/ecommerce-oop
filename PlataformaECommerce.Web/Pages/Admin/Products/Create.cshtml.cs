using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using PlataformaECommerce.Application.Features.Categories.DTOs;
using PlataformaECommerce.Application.Features.Categories.Queries;
using PlataformaECommerce.Application.Features.Products.Commands;
using PlataformaECommerce.Application.Interfaces.Services.Categories;
using PlataformaECommerce.Application.Interfaces.Services.Products;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Web.Configuration;
using PlataformaECommerce.Web.Services.Products;

namespace PlataformaECommerce.Web.Pages.Admin.Products
{
    /// <summary>
    /// Proporciona el registro administrativo de nuevos productos dentro del backoffice.
    /// </summary>
    /// <remarks>
    /// Esta página unifica la captura de datos comunes del catálogo y deriva la creación
    /// hacia el comando físico o digital correspondiente según el tipo seleccionado.
    /// </remarks>
    [EnableRateLimiting(WebRateLimitingOptions.SensitiveApiPolicyName)]
    public sealed class CreateModel : PageModel
    {
        private readonly IProductCommandService _productCommandService;
        private readonly ICategoryApplicationService _categoryApplicationService;
        private readonly IProductImageStorageService _productImageStorageService;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="CreateModel"/>.
        /// </summary>
        /// <param name="productCommandService">Servicio de escritura de productos.</param>
        public CreateModel(
            IProductCommandService productCommandService,
            ICategoryApplicationService categoryApplicationService,
            IProductImageStorageService productImageStorageService)
        {
            _productCommandService = productCommandService ?? throw new ArgumentNullException(nameof(productCommandService));
            _categoryApplicationService = categoryApplicationService ?? throw new ArgumentNullException(nameof(categoryApplicationService));
            _productImageStorageService = productImageStorageService ?? throw new ArgumentNullException(nameof(productImageStorageService));
        }

        /// <summary>
        /// Obtiene o establece el modelo de entrada del formulario de creación.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; } = new();

        /// <summary>
        /// Obtiene el mensaje de error funcional asociado a la creación cuando la operación falla.
        /// </summary>
        public string? ErrorMessage { get; private set; }

        /// <summary>
        /// Obtiene o establece el mensaje de éxito mostrado tras una creación exitosa.
        /// </summary>
        [TempData]
        public string? SuccessMessage { get; set; }

        /// <summary>
        /// Obtiene las categorías principales disponibles para clasificación.
        /// </summary>
        public IReadOnlyCollection<CategoryOptionViewModel> MainCategories { get; private set; } = Array.Empty<CategoryOptionViewModel>();

        /// <summary>
        /// Obtiene las subcategorías disponibles para clasificación.
        /// </summary>
        public IReadOnlyCollection<CategoryOptionViewModel> Subcategories { get; private set; } = Array.Empty<CategoryOptionViewModel>();

        /// <summary>
        /// Obtiene la URL visible utilizada para previsualizar la imagen principal actual del producto.
        /// </summary>
        public string MainImagePreviewUrl => ProductImageDefaults.ResolveDisplayUrl(Input.Images.MainImage.ResolvePreviewUrl());

        /// <summary>
        /// Inicializa el formulario administrativo de creación con el tipo indicado o el valor por defecto.
        /// </summary>
        /// <param name="productType">Tipo de producto inicialmente seleccionado.</param>
        public async Task OnGetAsync(TipoProducto? productType = null)
        {
            Input.ProductType = productType ?? TipoProducto.Fisico;
            Input.Currency = "COP";
            Input.IsActive = true;
            Input.RequiresShipping = true;
            EnsureImageContracts();
            await LoadCategoryOptionsAsync(CancellationToken.None);
        }

        /// <summary>
        /// Procesa el formulario unificado y crea un producto físico o digital según el tipo seleccionado.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
        /// <returns>Resultado de navegación correspondiente al flujo de creación.</returns>
        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
        {
            EnsureImageContracts();

            if (!ModelState.IsValid)
            {
                await LoadCategoryOptionsAsync(cancellationToken);
                return Page();
            }

            ProductImageProcessResult imageResult = await _productImageStorageService.ProcessMainImageAsync(
                Input.Images.MainImage.UploadedFile,
                Normalize(Input.Images.MainImage.ExternalImageUrl),
                currentImageUrl: null,
                Input.Slug,
                removeCurrentImage: false,
                cancellationToken);

            if (!imageResult.IsSuccess)
            {
                ErrorMessage = imageResult.ErrorMessage;
                await LoadCategoryOptionsAsync(cancellationToken);
                return Page();
            }

            return Input.ProductType switch
            {
                TipoProducto.Fisico => await CreatePhysicalProductAsync(imageResult.ImageUrl, cancellationToken),
                TipoProducto.Digital => await CreateDigitalProductAsync(imageResult.ImageUrl, cancellationToken),
                _ => Page()
            };
        }

        private async Task<IActionResult> CreatePhysicalProductAsync(string? mainImageUrl, CancellationToken cancellationToken)
        {
            var result = await _productCommandService.CreatePhysicalProductAsync(
                new CreatePhysicalProductCommand
                {
                    Name = Input.Name,
                    Description = Input.Description,
                    Sku = Input.Sku,
                    Price = Input.Price,
                    Currency = Input.Currency.Trim().ToUpperInvariant(),
                    Stock = Input.Stock,
                    Slug = Input.Slug,
                    MainImageUrl = mainImageUrl,
                    ImageGallery = Input.Images.GetPersistableGalleryUrls(mainImageUrl),
                    IsActive = Input.IsActive,
                    IsFeatured = Input.IsFeatured,
                    CategoryId = Input.CategoryId,
                    SubcategoryId = Input.SubcategoryId,
                    Tags = ParseTags(Input.Tags),
                    WeightKg = Input.WeightKg ?? 0,
                    HeightCm = Input.HeightCm ?? 0,
                    WidthCm = Input.WidthCm ?? 0,
                    LengthCm = Input.LengthCm ?? 0,
                    RequiresShipping = Input.RequiresShipping ?? true
                },
                cancellationToken);

            if (result.IsFailure)
            {
                await _productImageStorageService.DeleteIfManagedAsync(mainImageUrl, cancellationToken);
                ErrorMessage = result.Error.Message;
                await LoadCategoryOptionsAsync(cancellationToken);
                return Page();
            }

            SuccessMessage = "Producto físico creado correctamente.";
            return RedirectToPage("./Edit", new { id = result.Value });
        }

        private async Task<IActionResult> CreateDigitalProductAsync(string? mainImageUrl, CancellationToken cancellationToken)
        {
            var result = await _productCommandService.CreateDigitalProductAsync(
                new CreateDigitalProductCommand
                {
                    Name = Input.Name,
                    Description = Input.Description,
                    Sku = Input.Sku,
                    Price = Input.Price,
                    Currency = Input.Currency.Trim().ToUpperInvariant(),
                    Stock = Input.Stock,
                    Slug = Input.Slug,
                    MainImageUrl = mainImageUrl,
                    ImageGallery = Input.Images.GetPersistableGalleryUrls(mainImageUrl),
                    IsActive = Input.IsActive,
                    IsFeatured = Input.IsFeatured,
                    CategoryId = Input.CategoryId,
                    SubcategoryId = Input.SubcategoryId,
                    Tags = ParseTags(Input.Tags),
                    FileFormat = Normalize(Input.FileFormat) ?? string.Empty,
                    FileSizeMb = Input.FileSizeMb,
                    RequiresLicense = Input.RequiresLicense ?? false
                },
                cancellationToken);

            if (result.IsFailure)
            {
                await _productImageStorageService.DeleteIfManagedAsync(mainImageUrl, cancellationToken);
                ErrorMessage = result.Error.Message;
                await LoadCategoryOptionsAsync(cancellationToken);
                return Page();
            }

            SuccessMessage = "Producto digital creado correctamente.";
            return RedirectToPage("./Edit", new { id = result.Value });
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

        private static CategoryOptionViewModel MapCategoryOption(CategoryDto category)
        {
            return new CategoryOptionViewModel
            {
                Id = category.Id,
                Name = category.Name,
                ParentCategoryId = category.ParentCategoryId
            };
        }

        /// <summary>
        /// Representa el modelo de entrada del formulario administrativo de creación.
        /// </summary>
        public sealed class InputModel
        {
            /// <summary>
            /// Obtiene o establece el tipo funcional del producto a crear.
            /// </summary>
            public TipoProducto ProductType { get; set; } = TipoProducto.Fisico;

            /// <summary>
            /// Obtiene o establece el nombre comercial del producto.
            /// </summary>
            [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
            public string Name { get; set; } = string.Empty;

            /// <summary>
            /// Obtiene o establece la descripción del producto.
            /// </summary>
            [Required(ErrorMessage = "La descripción del producto es obligatoria.")]
            public string Description { get; set; } = string.Empty;

            /// <summary>
            /// Obtiene o establece el SKU del producto.
            /// </summary>
            [Required(ErrorMessage = "El SKU del producto es obligatorio.")]
            public string Sku { get; set; } = string.Empty;

            /// <summary>
            /// Obtiene o establece el precio unitario del producto.
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
            /// Obtiene o establece el código de moneda del producto.
            /// </summary>
            [Required(ErrorMessage = "La moneda del producto es obligatoria.")]
            public string Currency { get; set; } = "COP";

            /// <summary>
            /// Obtiene o establece el stock inicial del producto.
            /// </summary>
            [Range(0, int.MaxValue, ErrorMessage = "El stock inicial no puede ser negativo.")]
            public int Stock { get; set; }

            /// <summary>
            /// Obtiene o establece el slug del producto.
            /// </summary>
            [Required(ErrorMessage = "El slug del producto es obligatorio.")]
            public string Slug { get; set; } = string.Empty;

            /// <summary>
            /// Obtiene o establece el contrato de imágenes utilizado por el formulario administrativo.
            /// </summary>
            public ProductImagesInputModel Images { get; set; } = new();

            /// <summary>
            /// Obtiene o establece el estado inicial del producto.
            /// </summary>
            public bool IsActive { get; set; } = true;

            /// <summary>
            /// Obtiene o establece la marca inicial de destacado.
            /// </summary>
            public bool IsFeatured { get; set; }

            /// <summary>
            /// Obtiene o establece la categoría principal seleccionada.
            /// </summary>
            public Guid? CategoryId { get; set; }

            /// <summary>
            /// Obtiene o establece la subcategoría seleccionada.
            /// </summary>
            public Guid? SubcategoryId { get; set; }

            /// <summary>
            /// Obtiene o establece las etiquetas separadas por comas.
            /// </summary>
            public string? Tags { get; set; }

            /// <summary>
            /// Obtiene o establece el peso en kilogramos de un producto físico.
            /// </summary>
            public decimal? WeightKg { get; set; }

            /// <summary>
            /// Obtiene o establece el alto en centímetros de un producto físico.
            /// </summary>
            public decimal? HeightCm { get; set; }

            /// <summary>
            /// Obtiene o establece el ancho en centímetros de un producto físico.
            /// </summary>
            public decimal? WidthCm { get; set; }

            /// <summary>
            /// Obtiene o establece el largo en centímetros de un producto físico.
            /// </summary>
            public decimal? LengthCm { get; set; }

            /// <summary>
            /// Obtiene o establece un valor que indica si el producto físico requiere envío.
            /// </summary>
            public bool? RequiresShipping { get; set; } = true;

            /// <summary>
            /// Obtiene o establece el formato principal de un producto digital.
            /// </summary>
            public string? FileFormat { get; set; }

            /// <summary>
            /// Obtiene o establece el tamaño del archivo digital en megabytes.
            /// </summary>
            public decimal? FileSizeMb { get; set; }

            /// <summary>
            /// Obtiene o establece un valor que indica si el producto digital requiere licencia.
            /// </summary>
            public bool? RequiresLicense { get; set; }

            /// <summary>
            /// Obtiene un valor que indica si el formulario está configurado para un producto físico.
            /// </summary>
            public bool IsPhysicalProduct => ProductType == TipoProducto.Fisico;

            /// <summary>
            /// Obtiene un valor que indica si el formulario está configurado para un producto digital.
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
