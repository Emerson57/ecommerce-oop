using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlataformaECommerce.Application.Features.Categories.Commands;
using PlataformaECommerce.Application.Features.Categories.DTOs;
using PlataformaECommerce.Application.Features.Categories.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Categories;

namespace PlataformaECommerce.Web.Pages.Admin.Categories;

/// <summary>
/// Proporciona el alta administrativa de categorías.
/// </summary>
public sealed class CreateModel : PageModel
{
    private readonly ICategoryApplicationService _categoryApplicationService;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="CreateModel"/>.
    /// </summary>
    public CreateModel(ICategoryApplicationService categoryApplicationService)
    {
        _categoryApplicationService = categoryApplicationService ?? throw new ArgumentNullException(nameof(categoryApplicationService));
    }

    /// <summary>
    /// Modelo de entrada del formulario.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// Categorías principales disponibles para actuar como padre.
    /// </summary>
    public IReadOnlyCollection<CategoryDto> ParentCategories { get; private set; } = Array.Empty<CategoryDto>();

    /// <summary>
    /// Indica si el flujo actual corresponde a la creación de una subcategoría.
    /// </summary>
    public bool IsCreatingSubcategory => Input.ParentCategoryId.HasValue;

    /// <summary>
    /// Nombre de la categoría principal seleccionada como padre cuando aplica.
    /// </summary>
    public string? SelectedParentCategoryName { get; private set; }

    /// <summary>
    /// Mensaje funcional de error.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Mensaje de éxito mostrado tras el alta.
    /// </summary>
    [TempData]
    public string? SuccessMessage { get; set; }

    /// <summary>
    /// Inicializa la página de alta.
    /// </summary>
    public async Task OnGetAsync(Guid? parentCategoryId, CancellationToken cancellationToken)
    {
        Input.IsActive = true;
        await LoadParentCategoriesAsync(cancellationToken);

        if (parentCategoryId.HasValue)
        {
            ApplyParentCategorySelection(parentCategoryId.Value);
        }
    }

    /// <summary>
    /// Procesa la creación de una categoría.
    /// </summary>
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadParentCategoriesAsync(cancellationToken);
            return Page();
        }

        var result = await _categoryApplicationService.CreateCategoryAsync(
            new CreateCategoryCommand
            {
                Name = Input.Name,
                Slug = Input.Slug,
                Description = Normalize(Input.Description),
                ParentCategoryId = Input.ParentCategoryId,
                IsActive = Input.IsActive
            },
            cancellationToken);

        if (result.IsFailure)
        {
            ErrorMessage = result.Error.Message;
            await LoadParentCategoriesAsync(cancellationToken);
            return Page();
        }

        SuccessMessage = "Categoría registrada correctamente.";
        return RedirectToPage("./Index");
    }

    private async Task LoadParentCategoriesAsync(CancellationToken cancellationToken)
    {
        var result = await _categoryApplicationService.GetCategoriesAsync(
            new GetCategoriesQuery { RootOnly = true },
            cancellationToken);

        if (result.IsFailure)
        {
            ParentCategories = Array.Empty<CategoryDto>();
            ErrorMessage ??= result.Error.Message;
            return;
        }

        ParentCategories = result.Value;
        SelectedParentCategoryName = ParentCategories
            .FirstOrDefault(category => category.Id == Input.ParentCategoryId)?.Name;
    }

    private void ApplyParentCategorySelection(Guid parentCategoryId)
    {
        CategoryDto? parentCategory = ParentCategories.FirstOrDefault(category => category.Id == parentCategoryId);
        if (parentCategory is null)
        {
            ErrorMessage ??= "La categoría principal seleccionada no está disponible para registrar una subcategoría.";
            return;
        }

        Input.ParentCategoryId = parentCategory.Id;
        SelectedParentCategoryName = parentCategory.Name;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <summary>
    /// Representa el formulario de entrada para crear categorías.
    /// </summary>
    public sealed class InputModel
    {
        /// <summary>
        /// Nombre visible de la categoría.
        /// </summary>
        [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Slug único de la categoría.
        /// </summary>
        [Required(ErrorMessage = "El slug de la categoría es obligatorio.")]
        public string Slug { get; set; } = string.Empty;

        /// <summary>
        /// Descripción opcional de la categoría.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Categoría padre opcional.
        /// </summary>
        public Guid? ParentCategoryId { get; set; }

        /// <summary>
        /// Estado inicial de la categoría.
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
