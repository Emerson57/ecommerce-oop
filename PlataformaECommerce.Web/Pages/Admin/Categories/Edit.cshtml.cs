using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlataformaECommerce.Application.Features.Categories.Commands;
using PlataformaECommerce.Application.Features.Categories.DTOs;
using PlataformaECommerce.Application.Features.Categories.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Categories;

namespace PlataformaECommerce.Web.Pages.Admin.Categories;

/// <summary>
/// Proporciona la edición administrativa de categorías del catálogo.
/// </summary>
public sealed class EditModel : PageModel
{
    private readonly ICategoryApplicationService _categoryApplicationService;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="EditModel"/>.
    /// </summary>
    public EditModel(ICategoryApplicationService categoryApplicationService)
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
    /// Nombre de la categoría principal actualmente asociada como padre cuando aplica.
    /// </summary>
    public string? ParentCategoryName { get; private set; }

    /// <summary>
    /// Indica si la categoría actual corresponde a una categoría principal.
    /// </summary>
    public bool IsRootCategory => !Input.ParentCategoryId.HasValue;

    /// <summary>
    /// Indica si desde la categoría actual puede iniciarse el alta de una subcategoría hija.
    /// </summary>
    public bool CanCreateSubcategory => Input.Id != Guid.Empty && IsRootCategory;

    /// <summary>
    /// Mensaje funcional de error.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Carga la categoría solicitada para edición.
    /// </summary>
    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _categoryApplicationService.GetCategoryByIdAsync(new GetCategoryByIdQuery(id), cancellationToken);
        if (result.IsFailure)
        {
            TempData["StatusErrorMessage"] = result.Error.Message;
            return RedirectToPage("./Index");
        }

        MapToInput(result.Value);
        await LoadParentCategoriesAsync(cancellationToken);
        return Page();
    }

    /// <summary>
    /// Procesa la actualización de la categoría actual.
    /// </summary>
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadParentCategoriesAsync(cancellationToken);
            return Page();
        }

        var result = await _categoryApplicationService.UpdateCategoryAsync(
            new UpdateCategoryCommand
            {
                Id = Input.Id,
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

        TempData["SuccessMessage"] = "Categoría actualizada correctamente.";
        return RedirectToPage("./Index");
    }

    private void MapToInput(CategoryDto category)
    {
        Input = new InputModel
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            Description = category.Description,
            ParentCategoryId = category.ParentCategoryId,
            IsActive = category.IsActive
        };
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

        ParentCategories = result.Value
            .Where(category => category.Id != Input.Id)
            .OrderBy(category => category.Name)
            .ToArray();

        ParentCategoryName = ParentCategories
            .FirstOrDefault(category => category.Id == Input.ParentCategoryId)?.Name;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <summary>
    /// Representa el formulario administrativo de edición.
    /// </summary>
    public sealed class InputModel
    {
        /// <summary>
        /// Identificador de la categoría.
        /// </summary>
        public Guid Id { get; set; }

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
        /// Estado operativo de la categoría.
        /// </summary>
        public bool IsActive { get; set; }
    }
}
