using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlataformaECommerce.Application.Features.Products.Commands;
using PlataformaECommerce.Application.Features.Products.DTOs;
using PlataformaECommerce.Application.Features.Products.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Products;
using PlataformaECommerce.Domain.Enums;

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
    public sealed class IndexModel : PageModel
    {
        private const int MaxVisiblePageLinks = 5;
        private readonly IProductApplicationService _productApplicationService;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="IndexModel"/>.
        /// </summary>
        /// <param name="productApplicationService">Servicio de aplicación de productos.</param>
        public IndexModel(IProductApplicationService productApplicationService)
        {
            _productApplicationService = productApplicationService ?? throw new ArgumentNullException(nameof(productApplicationService));
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
            var result = await _productApplicationService.GetProductsAsync(
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
        /// Activa un producto desde el listado administrativo.
        /// </summary>
        public Task<IActionResult> OnPostActivateAsync(Guid productId, CancellationToken cancellationToken)
        {
            return ExecuteCatalogActionAsync(
                () => _productApplicationService.ActivateProductAsync(
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
                () => _productApplicationService.DeactivateProductAsync(
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
                () => _productApplicationService.FeatureProductAsync(
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
                () => _productApplicationService.UnfeatureProductAsync(
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
                () => _productApplicationService.UpdateProductStockAsync(
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
                () => _productApplicationService.ApplyProductPromotionAsync(
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
                () => _productApplicationService.RemoveProductPromotionAsync(
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
    }
}
