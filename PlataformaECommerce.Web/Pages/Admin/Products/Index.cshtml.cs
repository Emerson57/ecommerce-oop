using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using PlataformaECommerce.Application.Features.Categories.DTOs;
using PlataformaECommerce.Application.Features.Categories.Queries;
using PlataformaECommerce.Application.Features.Products.Commands;
using PlataformaECommerce.Application.Features.Products.DTOs;
using PlataformaECommerce.Application.Features.Products.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Categories;
using PlataformaECommerce.Application.Interfaces.Services.Products;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Web.Configuration;
using PlataformaECommerce.Web.Services.Products;

namespace PlataformaECommerce.Web.Pages.Admin.Products
{
    /// <summary>
    /// Proporciona el listado administrativo real del catálogo de productos.
    /// </summary>
    /// <remarks>
    /// Esta página permite consultar productos con filtros útiles de backoffice y ejecutar
    /// acciones rápidas sobre el estado, destacado, inventario y promociones del catálogo
    /// sin abandonar el listado.
    /// </remarks>
    [EnableRateLimiting(WebRateLimitingOptions.SensitiveApiPolicyName)]
    public sealed class IndexModel : PageModel
    {
        private const int MaxVisiblePageLinks = 5;
        private const long MaxImportFileSizeInBytes = 2 * 1024 * 1024;
        private readonly ICategoryApplicationService _categoryApplicationService;
        private readonly IProductCommandService _productCommandService;
        private readonly IProductPromotionService _productPromotionService;
        private readonly IProductQueryService _productQueryService;
        private readonly IProductStockService _productStockService;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="IndexModel"/>.
        /// </summary>
        /// <param name="productCommandService">Servicio de escritura de productos.</param>
        /// <param name="productQueryService">Servicio de consulta de productos.</param>
        /// <param name="productStockService">Servicio de inventario y disponibilidad.</param>
        /// <param name="productPromotionService">Servicio promocional y de merchandising.</param>
        public IndexModel(
            IProductCommandService productCommandService,
            IProductQueryService productQueryService,
            IProductStockService productStockService,
            IProductPromotionService productPromotionService,
            ICategoryApplicationService categoryApplicationService)
        {
            _productCommandService = productCommandService ?? throw new ArgumentNullException(nameof(productCommandService));
            _productQueryService = productQueryService ?? throw new ArgumentNullException(nameof(productQueryService));
            _productStockService = productStockService ?? throw new ArgumentNullException(nameof(productStockService));
            _productPromotionService = productPromotionService ?? throw new ArgumentNullException(nameof(productPromotionService));
            _categoryApplicationService = categoryApplicationService ?? throw new ArgumentNullException(nameof(categoryApplicationService));
        }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public TipoProducto? ProductType { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? IsActive { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? IsFeatured { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? HasStock { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "createdAt";

        [BindProperty(SupportsGet = true)]
        public bool SortDescending { get; set; } = true;

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// Modelo de entrada utilizado para la importación Excel de productos.
        /// </summary>
        [BindProperty]
        public ImportInputModel ImportInput { get; set; } = new();

        /// <summary>
        /// Obtiene la colección de productos proyectados para el listado administrativo.
        /// </summary>
        public IReadOnlyCollection<ProductDto> Products { get; private set; } = Array.Empty<ProductDto>();

        /// <summary>
        /// Obtiene un mensaje de error funcional asociado a la consulta actual.
        /// </summary>
        public string? ErrorMessage { get; private set; }

        /// <summary>
        /// Obtiene o establece el mensaje de éxito mostrado después de una operación administrativa.
        /// </summary>
        [TempData]
        public string? SuccessMessage { get; set; }

        /// <summary>
        /// Obtiene o establece el mensaje de error mostrado después de una operación administrativa.
        /// </summary>
        [TempData]
        public string? StatusErrorMessage { get; set; }

        /// <summary>
        /// Obtiene la cantidad total de coincidencias del catálogo actual.
        /// </summary>
        public int TotalCount { get; private set; }

        /// <summary>
        /// Obtiene la cantidad total de páginas calculadas para la consulta actual.
        /// </summary>
        public int TotalPages { get; private set; }

        /// <summary>
        /// Obtiene un valor que indica si existe una página anterior disponible.
        /// </summary>
        public bool HasPreviousPage { get; private set; }

        /// <summary>
        /// Obtiene un valor que indica si existe una página siguiente disponible.
        /// </summary>
        public bool HasNextPage { get; private set; }

        /// <summary>
        /// Obtiene la colección de números de página visibles en la navegación.
        /// </summary>
        public IReadOnlyCollection<int> VisiblePageNumbers { get; private set; } = Array.Empty<int>();

        /// <summary>
        /// Obtiene el número de página normalizado para la vista actual.
        /// </summary>
        public int NormalizedPageNumber => PageNumber < 1 ? 1 : PageNumber;

        /// <summary>
        /// Obtiene el tamaño de página normalizado para la vista actual.
        /// </summary>
        public int NormalizedPageSize => PageSize switch
        {
            < 1 => 20,
            > 100 => 100,
            _ => PageSize
        };

        /// <summary>
        /// Obtiene la posición inicial del rango actual de resultados.
        /// </summary>
        public int FirstItemNumber => TotalCount == 0 ? 0 : ((PageNumber - 1) * PageSize) + 1;

        /// <summary>
        /// Obtiene la posición final del rango actual de resultados.
        /// </summary>
        public int LastItemNumber => TotalCount == 0 ? 0 : FirstItemNumber + Products.Count - 1;

        /// <summary>
        /// Ejecuta la consulta del catálogo administrativo aplicando filtros, ordenamiento y paginación.
        /// </summary>
        public async Task OnGetAsync(CancellationToken cancellationToken)
        {
            var result = await _productQueryService.GetProductsAsync(
                new GetProductsQuery
                {
                    SearchTerm = Normalize(SearchTerm),
                    ProductType = ProductType,
                    IsActive = IsActive,
                    IsFeatured = IsFeatured,
                    HasStock = HasStock,
                    PageNumber = NormalizedPageNumber,
                    PageSize = NormalizedPageSize,
                    SortBy = Normalize(SortBy) ?? "createdAt",
                    SortDescending = SortDescending,
                    RequestedByUserId = ResolveCurrentUserId()
                },
                cancellationToken);

            if (result.IsFailure)
            {
                ErrorMessage = result.Error.Message;
                return;
            }

            Products = result.Value.Items;
            TotalCount = result.Value.TotalCount;
            PageNumber = result.Value.PageNumber;
            PageSize = result.Value.PageSize;
            TotalPages = result.Value.TotalPages;
            HasPreviousPage = result.Value.HasPreviousPage;
            HasNextPage = result.Value.HasNextPage;
            VisiblePageNumbers = BuildVisiblePageNumbers(PageNumber, TotalPages);
        }

        /// <summary>
        /// Descarga la plantilla Excel oficial de importación de productos.
        /// </summary>
        public async Task<IActionResult> OnGetDownloadImportTemplateAsync(CancellationToken cancellationToken)
        {
            var categoriesResult = await _categoryApplicationService.GetCategoriesAsync(
                new GetCategoriesQuery { OnlyActive = true },
                cancellationToken);

            if (categoriesResult.IsFailure)
            {
                StatusErrorMessage = categoriesResult.Error.Message;
                return RedirectToPage("./Index", BuildRouteValues());
            }

            if (!categoriesResult.Value.Any(category => category.IsRootCategory))
            {
                StatusErrorMessage = "Debe registrar al menos una categoría principal activa antes de descargar la plantilla Excel de productos.";
                return RedirectToPage("./Index", BuildRouteValues());
            }

            byte[] templateBytes = ProductExcelTemplateProvider.BuildTemplateBytes(categoriesResult.Value);
            return File(templateBytes, ProductExcelTemplateProvider.ContentType, ProductExcelTemplateProvider.FileName);
        }

        /// <summary>
        /// Procesa la importación Excel de productos desde el backoffice administrativo.
        /// </summary>
        public async Task<IActionResult> OnPostImportAsync(CancellationToken cancellationToken)
        {
            if (ImportInput.ImportFile is null || ImportInput.ImportFile.Length == 0)
            {
                StatusErrorMessage = "Seleccione un archivo Excel válido para importar productos.";
                return RedirectToPage("./Index", BuildRouteValues());
            }

            if (ImportInput.ImportFile.Length > MaxImportFileSizeInBytes)
            {
                StatusErrorMessage = "El archivo Excel de productos supera el tamaño máximo permitido de 2 MB.";
                return RedirectToPage("./Index", BuildRouteValues());
            }

            var conversionResult = await ProductExcelImportFileConverter.ConvertAsync(ImportInput.ImportFile, cancellationToken);
            if (conversionResult.IsFailure)
            {
                StatusErrorMessage = conversionResult.Error.Message;
                return RedirectToPage("./Index", BuildRouteValues());
            }

            var importResult = await _productCommandService.ImportProductsAsync(
                new ImportProductsCommand
                {
                    Rows = conversionResult.Value,
                    RequestedByUserId = ResolveCurrentUserId()
                },
                cancellationToken);

            if (importResult.IsFailure)
            {
                StatusErrorMessage = importResult.Error.Message;
                return RedirectToPage("./Index", BuildRouteValues());
            }

            SuccessMessage = $"Importación completada correctamente. Productos físicos creados: {importResult.Value.PhysicalProductsCreated}. Productos digitales creados: {importResult.Value.DigitalProductsCreated}.";
            return RedirectToPage("./Index", BuildRouteValues());
        }

        /// <summary>
        /// Activa un producto desde el listado administrativo.
        /// </summary>
        public Task<IActionResult> OnPostActivateAsync(Guid productId, CancellationToken cancellationToken)
        {
            return ExecuteCatalogActionAsync(
                () => _productStockService.ActivateProductAsync(
                    new ActivateProductCommand
                    {
                        ProductId = productId,
                        RequestedByUserId = ResolveCurrentUserId(),
                        Reason = "Backoffice.AdminProducts.Index.Activate"
                    },
                    cancellationToken),
                _ => "Producto activado correctamente.");
        }

        /// <summary>
        /// Desactiva un producto desde el listado administrativo.
        /// </summary>
        public Task<IActionResult> OnPostDeactivateAsync(Guid productId, CancellationToken cancellationToken)
        {
            return ExecuteCatalogActionAsync(
                () => _productStockService.DeactivateProductAsync(
                    new DeactivateProductCommand
                    {
                        ProductId = productId,
                        RequestedByUserId = ResolveCurrentUserId(),
                        Reason = "Backoffice.AdminProducts.Index.Deactivate"
                    },
                    cancellationToken),
                _ => "Producto desactivado correctamente.");
        }

        /// <summary>
        /// Destaca un producto desde el listado administrativo.
        /// </summary>
        public Task<IActionResult> OnPostFeatureAsync(Guid productId, CancellationToken cancellationToken)
        {
            return ExecuteCatalogActionAsync(
                () => _productPromotionService.FeatureProductAsync(
                    new FeatureProductCommand
                    {
                        ProductId = productId,
                        RequestedByUserId = ResolveCurrentUserId(),
                        Reason = "Backoffice.AdminProducts.Index.Feature"
                    },
                    cancellationToken),
                _ => "Producto destacado correctamente.");
        }

        /// <summary>
        /// Retira la marca de destacado de un producto desde el listado administrativo.
        /// </summary>
        public Task<IActionResult> OnPostUnfeatureAsync(Guid productId, CancellationToken cancellationToken)
        {
            return ExecuteCatalogActionAsync(
                () => _productPromotionService.UnfeatureProductAsync(
                    new UnfeatureProductCommand
                    {
                        ProductId = productId,
                        RequestedByUserId = ResolveCurrentUserId(),
                        Reason = "Backoffice.AdminProducts.Index.Unfeature"
                    },
                    cancellationToken),
                _ => "Marca de destacado retirada correctamente.");
        }

        /// <summary>
        /// Ajusta el inventario de un producto desde el listado administrativo.
        /// </summary>
        public Task<IActionResult> OnPostUpdateStockAsync(
            Guid productId,
            StockUpdateType updateType,
            int quantity,
            string? reason,
            CancellationToken cancellationToken)
        {
            return ExecuteCatalogActionAsync(
                () => _productStockService.UpdateProductStockAsync(
                    new UpdateProductStockCommand
                    {
                        ProductId = productId,
                        UpdateType = updateType,
                        Quantity = quantity,
                        Reason = Normalize(reason) ?? "Backoffice.AdminProducts.Index.UpdateStock",
                        RequestedByUserId = ResolveCurrentUserId()
                    },
                    cancellationToken),
                result => $"Inventario actualizado correctamente. Stock actual: {result.Stock} unidad(es).");
        }

        /// <summary>
        /// Aplica una promoción porcentual a un producto desde el listado administrativo.
        /// </summary>
        public Task<IActionResult> OnPostApplyPromotionAsync(
            Guid productId,
            decimal discountPercentage,
            string? reason,
            CancellationToken cancellationToken)
        {
            return ExecuteCatalogActionAsync(
                () => _productPromotionService.ApplyProductPromotionAsync(
                    new ApplyProductPromotionCommand
                    {
                        ProductId = productId,
                        DiscountPercentage = discountPercentage,
                        Reason = Normalize(reason) ?? "Backoffice.AdminProducts.Index.ApplyPromotion",
                        RequestedByUserId = ResolveCurrentUserId()
                    },
                    cancellationToken),
                result => $"Promoción aplicada correctamente. Precio vigente: {result.Currency} {result.Price:N2}.");
        }

        /// <summary>
        /// Retira una promoción activa y restaura el precio base del producto desde el listado administrativo.
        /// </summary>
        public Task<IActionResult> OnPostRemovePromotionAsync(
            Guid productId,
            string? reason,
            CancellationToken cancellationToken)
        {
            return ExecuteCatalogActionAsync(
                () => _productPromotionService.RemoveProductPromotionAsync(
                    new RemoveProductPromotionCommand
                    {
                        ProductId = productId,
                        Reason = Normalize(reason) ?? "Backoffice.AdminProducts.Index.RemovePromotion",
                        RequestedByUserId = ResolveCurrentUserId()
                    },
                    cancellationToken),
                result => $"Promoción retirada correctamente. Precio restaurado: {result.Currency} {result.Price:N2}.");
        }

        private async Task<IActionResult> ExecuteCatalogActionAsync(
            Func<Task<PlataformaECommerce.Application.Common.Results.Result<ProductResponseDto>>> operation,
            Func<ProductResponseDto, string> successMessageFactory)
        {
            var result = await operation();

            if (result.IsFailure)
            {
                StatusErrorMessage = result.Error.Message;
            }
            else
            {
                SuccessMessage = successMessageFactory(result.Value);
            }

            return RedirectToPage("./Index", BuildRouteValues());
        }

        private Guid? ResolveCurrentUserId()
        {
            string? rawUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(rawUserId, out Guid userId)
                ? userId
                : null;
        }

        private Dictionary<string, object?> BuildRouteValues()
        {
            return new Dictionary<string, object?>
            {
                [nameof(SearchTerm)] = SearchTerm,
                [nameof(ProductType)] = ProductType,
                [nameof(IsActive)] = IsActive,
                [nameof(IsFeatured)] = IsFeatured,
                [nameof(HasStock)] = HasStock,
                [nameof(SortBy)] = SortBy,
                [nameof(SortDescending)] = SortDescending,
                [nameof(PageNumber)] = PageNumber,
                [nameof(PageSize)] = PageSize
            };
        }

        private static IReadOnlyCollection<int> BuildVisiblePageNumbers(int pageNumber, int totalPages)
        {
            if (totalPages <= 0)
            {
                return Array.Empty<int>();
            }

            int halfWindow = MaxVisiblePageLinks / 2;
            int start = Math.Max(1, pageNumber - halfWindow);
            int end = Math.Min(totalPages, start + MaxVisiblePageLinks - 1);

            if ((end - start + 1) < MaxVisiblePageLinks)
            {
                start = Math.Max(1, end - MaxVisiblePageLinks + 1);
            }

            return Enumerable.Range(start, end - start + 1).ToArray();
        }

        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        /// <summary>
        /// Representa el formulario de carga Excel de productos.
        /// </summary>
        public sealed class ImportInputModel
        {
            /// <summary>
            /// Archivo Excel suministrado por el administrador.
            /// </summary>
            public IFormFile? ImportFile { get; set; }
        }
    }
}
