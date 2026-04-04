using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Application.Features.Categories.DTOs;
using PlataformaECommerce.Application.Features.Categories.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Categories;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Web.ViewComponents;

/// <summary>
/// Renderiza el footer comercial del storefront combinando navegación pública, accesos contextuales y categorías activas.
/// </summary>
public sealed class StoreFooterViewComponent : ViewComponent
{
    private const int MaxFooterCategories = 4;
    private readonly ICategoryApplicationService _categoryApplicationService;
    private readonly ClientExperienceOptions _clientExperienceOptions;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="StoreFooterViewComponent"/>.
    /// </summary>
    /// <param name="categoryApplicationService">Servicio de aplicación de categorías.</param>
    public StoreFooterViewComponent(ICategoryApplicationService categoryApplicationService, IOptions<ClientExperienceOptions> clientExperienceOptions)
    {
        ArgumentNullException.ThrowIfNull(clientExperienceOptions);

        _categoryApplicationService = categoryApplicationService ?? throw new ArgumentNullException(nameof(categoryApplicationService));
        _clientExperienceOptions = clientExperienceOptions.Value;
    }

    /// <summary>
    /// Construye el footer comercial a partir de la información disponible del storefront.
    /// </summary>
    /// <returns>Vista del componente con las secciones del footer.</returns>
    public async Task<IViewComponentResult> InvokeAsync()
    {
        IReadOnlyCollection<FooterLinkViewModel> categoryLinks = await LoadCategoryLinksAsync();

        StoreFooterViewModel model = new()
        {
            BrandName = _clientExperienceOptions.StorefrontName,
            BrandTagline = _clientExperienceOptions.StorefrontTagline,
            CurrentYear = DateTime.UtcNow.Year,
            SupportEmail = _clientExperienceOptions.SupportEmail,
            SupportPhone = _clientExperienceOptions.SupportPhone,
            SupportHours = _clientExperienceOptions.SupportHours,
            ExploreLinks = BuildExploreLinks(),
            CategoryLinks = categoryLinks,
            AccessLinks = BuildAccessLinks()
        };

        return View(model);
    }

    private async Task<IReadOnlyCollection<FooterLinkViewModel>> LoadCategoryLinksAsync()
    {
        var result = await _categoryApplicationService.GetCategoriesAsync(
            new GetCategoriesQuery
            {
                OnlyActive = true,
                RootOnly = true
            },
            HttpContext.RequestAborted);

        return result.IsFailure
            ? Array.Empty<FooterLinkViewModel>()
            : result.Value
                .Where(category => category.IsRootCategory && category.IsActive)
                .OrderBy(category => category.Name)
                .Take(MaxFooterCategories)
                .Select(MapCategory)
                .ToArray();
    }

    private IReadOnlyCollection<FooterLinkViewModel> BuildExploreLinks()
    {
        return
        [
            new FooterLinkViewModel { Text = "Inicio", Page = "/Index" },
            new FooterLinkViewModel { Text = "Catálogo", Page = "/Catalog/Index" },
            new FooterLinkViewModel { Text = "Destacados", Page = "/Catalog/Index", RouteValues = new Dictionary<string, string> { ["IsFeatured"] = "true" } },
            new FooterLinkViewModel { Text = "Novedades", Page = "/Catalog/Index", RouteValues = new Dictionary<string, string> { ["SortBy"] = "createdAt", ["SortDescending"] = "true" } }
        ];
    }

    private IReadOnlyCollection<FooterLinkViewModel> BuildAccessLinks()
    {
        bool isAuthenticated = User.Identity?.IsAuthenticated == true;
        bool isAdminAuthenticated = isAuthenticated && (User.IsInRole("Administrador") || User.IsInRole("SuperUsuario"));
        bool isCustomerAuthenticated = isAuthenticated && User.IsInRole("Cliente");

        List<FooterLinkViewModel> links = [];

        if (isAdminAuthenticated)
        {
            links.Add(new FooterLinkViewModel { Text = "Panel administrativo", Page = "/Admin/Index" });
        }
        else if (isCustomerAuthenticated)
        {
            links.Add(new FooterLinkViewModel { Text = "Mi cuenta", Page = "/Account/Index" });
        }
        else
        {
            links.Add(new FooterLinkViewModel { Text = "Ingresar", Page = "/Auth/Login" });
            links.Add(new FooterLinkViewModel { Text = "Crear cuenta", Page = "/Auth/Register" });
        }

        links.Add(new FooterLinkViewModel { Text = "Carrito", Page = "/Cart/Index" });

        string? email = HttpContext.User.FindFirstValue(ClaimTypes.Email);
        if (!string.IsNullOrWhiteSpace(email))
        {
            links.Add(new FooterLinkViewModel { Text = email.Trim(), Page = isAdminAuthenticated ? "/Admin/Index" : "/Account/Index", IsMuted = true });
        }

        return links;
    }

    private static FooterLinkViewModel MapCategory(CategoryDto category)
    {
        return new FooterLinkViewModel
        {
            Text = category.Name,
            Page = "/Catalog/Index",
            RouteValues = new Dictionary<string, string>
            {
                ["CategoryId"] = category.Id.ToString()
            }
        };
    }

    /// <summary>
    /// Representa el modelo visual del footer comercial.
    /// </summary>
    public sealed class StoreFooterViewModel
    {
        /// <summary>
        /// Nombre visible de la marca configurada para la tienda.
        /// </summary>
        public string BrandName { get; init; } = string.Empty;

        /// <summary>
        /// Propuesta de valor corta visible en el footer comercial.
        /// </summary>
        public string BrandTagline { get; init; } = string.Empty;

        /// <summary>
        /// Año visible del footer.
        /// </summary>
        public int CurrentYear { get; init; }

        /// <summary>
        /// Correo principal de soporte.
        /// </summary>
        public string SupportEmail { get; init; } = string.Empty;

        /// <summary>
        /// Teléfono principal de soporte.
        /// </summary>
        public string SupportPhone { get; init; } = string.Empty;

        /// <summary>
        /// Horario operativo de soporte.
        /// </summary>
        public string SupportHours { get; init; } = string.Empty;

        /// <summary>
        /// Enlaces de navegación pública principal.
        /// </summary>
        public IReadOnlyCollection<FooterLinkViewModel> ExploreLinks { get; init; } = Array.Empty<FooterLinkViewModel>();

        /// <summary>
        /// Enlaces a categorías raíz activas del catálogo.
        /// </summary>
        public IReadOnlyCollection<FooterLinkViewModel> CategoryLinks { get; init; } = Array.Empty<FooterLinkViewModel>();

        /// <summary>
        /// Enlaces contextuales de acceso según la sesión actual.
        /// </summary>
        public IReadOnlyCollection<FooterLinkViewModel> AccessLinks { get; init; } = Array.Empty<FooterLinkViewModel>();
    }

    /// <summary>
    /// Representa un enlace navegable del footer comercial.
    /// </summary>
    public sealed class FooterLinkViewModel
    {
        /// <summary>
        /// Texto visible del enlace.
        /// </summary>
        public string Text { get; init; } = string.Empty;

        /// <summary>
        /// Página Razor de destino.
        /// </summary>
        public string Page { get; init; } = string.Empty;

        /// <summary>
        /// Valores de ruta adicionales asociados al enlace.
        /// </summary>
        public IDictionary<string, string>? RouteValues { get; init; }

        /// <summary>
        /// Indica si el enlace debe mostrarse con énfasis secundario.
        /// </summary>
        public bool IsMuted { get; init; }
    }
}
