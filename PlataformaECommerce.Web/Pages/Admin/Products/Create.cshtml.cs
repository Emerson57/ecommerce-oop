using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlataformaECommerce.Application.Features.Products.Commands;
using PlataformaECommerce.Application.Interfaces.Services.Products;
using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Web.Pages.Admin.Products
{
    /// <summary>
    /// Proporciona el registro administrativo de nuevos productos dentro del backoffice.
    /// </summary>
    /// <remarks>
    /// Esta página unifica la captura de datos comunes del catálogo y deriva la creación
    /// hacia el comando físico o digital correspondiente según el tipo seleccionado.
    /// </remarks>
    public sealed class CreateModel : PageModel
    {
        private readonly IProductApplicationService _productApplicationService;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="CreateModel"/>.
        /// </summary>
        /// <param name="productApplicationService">Servicio de aplicación de productos.</param>
        public CreateModel(IProductApplicationService productApplicationService)
        {
            _productApplicationService = productApplicationService ?? throw new ArgumentNullException(nameof(productApplicationService));
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
        /// Inicializa el formulario administrativo de creación con el tipo indicado o el valor por defecto.
        /// </summary>
        /// <param name="productType">Tipo de producto inicialmente seleccionado.</param>
        public void OnGet(TipoProducto? productType = null)
        {
            Input.ProductType = productType ?? TipoProducto.Fisico;
            Input.Currency = "COP";
            Input.IsActive = true;
            Input.RequiresShipping = true;
        }

        /// <summary>
        /// Procesa el formulario unificado y crea un producto físico o digital según el tipo seleccionado.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
        /// <returns>Resultado de navegación correspondiente al flujo de creación.</returns>
        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (!TryParseGuid(Input.CategoryId, out Guid? categoryId))
            {
                ModelState.AddModelError(nameof(Input.CategoryId), "La categoría debe ser un GUID válido o permanecer vacía.");
                return Page();
            }

            if (!TryParseGuid(Input.SubcategoryId, out Guid? subcategoryId))
            {
                ModelState.AddModelError(nameof(Input.SubcategoryId), "La subcategoría debe ser un GUID válido o permanecer vacía.");
                return Page();
            }

            return Input.ProductType switch
            {
                TipoProducto.Fisico => await CreatePhysicalProductAsync(categoryId, subcategoryId, cancellationToken),
                TipoProducto.Digital => await CreateDigitalProductAsync(categoryId, cancellationToken),
                _ => Page()
            };
        }

        private async Task<IActionResult> CreatePhysicalProductAsync(Guid? categoryId, Guid? subcategoryId, CancellationToken cancellationToken)
        {
            var result = await _productApplicationService.CreatePhysicalProductAsync(
                new CreatePhysicalProductCommand
                {
                    Name = Input.Name,
                    Description = Input.Description,
                    Sku = Input.Sku,
                    Price = Input.Price,
                    Currency = Input.Currency.Trim().ToUpperInvariant(),
                    Stock = Input.Stock,
                    Slug = Input.Slug,
                    MainImageUrl = Normalize(Input.MainImageUrl),
                    IsActive = Input.IsActive,
                    IsFeatured = Input.IsFeatured,
                    CategoryId = categoryId,
                    SubcategoryId = subcategoryId,
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
                ErrorMessage = result.Error.Message;
                return Page();
            }

            SuccessMessage = "Producto físico creado correctamente.";
            return RedirectToPage("./Edit", new { id = result.Value });
        }

        private async Task<IActionResult> CreateDigitalProductAsync(Guid? categoryId, CancellationToken cancellationToken)
        {
            var result = await _productApplicationService.CreateDigitalProductAsync(
                new CreateDigitalProductCommand
                {
                    Name = Input.Name,
                    Description = Input.Description,
                    Sku = Input.Sku,
                    Price = Input.Price,
                    Currency = Input.Currency.Trim().ToUpperInvariant(),
                    Stock = Input.Stock,
                    Slug = Input.Slug,
                    MainImageUrl = Normalize(Input.MainImageUrl),
                    IsActive = Input.IsActive,
                    IsFeatured = Input.IsFeatured,
                    CategoryId = categoryId,
                    Tags = ParseTags(Input.Tags),
                    FileFormat = Normalize(Input.FileFormat) ?? string.Empty,
                    FileSizeMb = Input.FileSizeMb,
                    RequiresLicense = Input.RequiresLicense ?? false
                },
                cancellationToken);

            if (result.IsFailure)
            {
                ErrorMessage = result.Error.Message;
                return Page();
            }

            SuccessMessage = "Producto digital creado correctamente.";
            return RedirectToPage("./Edit", new { id = result.Value });
        }

        private static bool TryParseGuid(string? value, out Guid? parsedValue)
        {
            parsedValue = null;

            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            if (!Guid.TryParse(value.Trim(), out Guid guidValue))
            {
                return false;
            }

            parsedValue = guidValue;
            return true;
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
            /// Obtiene o establece la imagen principal del producto.
            /// </summary>
            public string? MainImageUrl { get; set; }

            /// <summary>
            /// Obtiene o establece el estado inicial del producto.
            /// </summary>
            public bool IsActive { get; set; } = true;

            /// <summary>
            /// Obtiene o establece la marca inicial de destacado.
            /// </summary>
            public bool IsFeatured { get; set; }

            /// <summary>
            /// Obtiene o establece la categoría principal en formato GUID opcional.
            /// </summary>
            public string? CategoryId { get; set; }

            /// <summary>
            /// Obtiene o establece la subcategoría en formato GUID opcional.
            /// </summary>
            public string? SubcategoryId { get; set; }

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
    }
}
