using Microsoft.AspNetCore.Mvc;
using PlataformaECommerce.Application.Features.Categories.DTOs;
using PlataformaECommerce.Application.Features.Categories.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Categories;

namespace PlataformaECommerce.Web.ViewComponents;

/// <summary>
/// Renderiza la navegación superior dinámica del storefront a partir de las categorías activas.
/// </summary>
public sealed class StoreCategoryNavViewComponent : ViewComponent
{
    private const int MaxNavigationCategories = 6;
    private readonly ICategoryApplicationService _categoryApplicationService;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="StoreCategoryNavViewComponent"/>.
    /// </summary>
    /// <param name="categoryApplicationService">Servicio de aplicación de categorías.</param>
    public StoreCategoryNavViewComponent(ICategoryApplicationService categoryApplicationService)
    {
        _categoryApplicationService = categoryApplicationService ?? throw new ArgumentNullException(nameof(categoryApplicationService));
    }

    /// <summary>
    /// Construye el menú dinámico del storefront usando categorías raíz activas.
    /// </summary>
    /// <returns>Vista del componente con la colección de enlaces a categorías.</returns>
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var result = await _categoryApplicationService.GetCategoriesAsync(
            new GetCategoriesQuery
            {
                OnlyActive = true,
                RootOnly = true
            },
            HttpContext.RequestAborted);

        IReadOnlyCollection<StoreCategoryNavItemViewModel> model = result.IsFailure
            ? Array.Empty<StoreCategoryNavItemViewModel>()
            : result.Value
                .Where(category => category.IsRootCategory && category.IsActive)
                .OrderBy(category => category.Name)
                .Take(MaxNavigationCategories)
                .Select(Map)
                .ToArray();

        return View(model);
    }

    private static StoreCategoryNavItemViewModel Map(CategoryDto category)
    {
        return new StoreCategoryNavItemViewModel
        {
            Id = category.Id,
            Name = category.Name
        };
    }

    /// <summary>
    /// Representa un elemento navegable del menú superior del storefront.
    /// </summary>
    public sealed class StoreCategoryNavItemViewModel
    {
        /// <summary>
        /// Identificador de la categoría asociada al enlace.
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Nombre visible de la categoría.
        /// </summary>
        public string Name { get; init; } = string.Empty;
    }
}
