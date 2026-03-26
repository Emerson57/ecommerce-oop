using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlataformaECommerce.Application.Features.Categories.Commands;
using PlataformaECommerce.Application.Features.Categories.DTOs;
using PlataformaECommerce.Application.Features.Categories.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Categories;
using PlataformaECommerce.Web.Services.Categories;

namespace PlataformaECommerce.Web.Pages.Admin.Categories;

/// <summary>
/// Proporciona el listado administrativo de categorías del catálogo.
/// </summary>
public sealed class IndexModel : PageModel
{
    private const long MaxImportFileSizeInBytes = 1024 * 1024;
    private readonly ICategoryApplicationService _categoryApplicationService;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="IndexModel"/>.
    /// </summary>
    public IndexModel(ICategoryApplicationService categoryApplicationService)
    {
        _categoryApplicationService = categoryApplicationService ?? throw new ArgumentNullException(nameof(categoryApplicationService));
    }

    /// <summary>
    /// Categorías visibles en la consulta actual.
    /// </summary>
    public IReadOnlyCollection<CategoryDto> Categories { get; private set; } = Array.Empty<CategoryDto>();

    /// <summary>
    /// Modelo de entrada para la importación XML de categorías.
    /// </summary>
    [BindProperty]
    public ImportInputModel ImportInput { get; set; } = new();

    /// <summary>
    /// Mensaje de error funcional del listado.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Mensaje de éxito posterior a una acción administrativa.
    /// </summary>
    [TempData]
    public string? SuccessMessage { get; set; }

    /// <summary>
    /// Mensaje de error posterior a una acción administrativa.
    /// </summary>
    [TempData]
    public string? StatusErrorMessage { get; set; }

    /// <summary>
    /// Carga el listado completo de categorías para administración.
    /// </summary>
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadCategoriesAsync(cancellationToken);
    }

    /// <summary>
    /// Descarga la plantilla XML de categorías y subcategorías.
    /// </summary>
    public FileContentResult OnGetDownloadTemplate()
        => File(CategoryXmlTemplateProvider.BuildTemplateBytes(), CategoryXmlTemplateProvider.ContentType, CategoryXmlTemplateProvider.FileName);

    /// <summary>
    /// Descarga la plantilla CSV de categorías y subcategorías.
    /// </summary>
    public FileContentResult OnGetDownloadCsvTemplate()
        => File(CategoryCsvTemplateProvider.BuildTemplateBytes(), CategoryCsvTemplateProvider.ContentType, CategoryCsvTemplateProvider.FileName);

    /// <summary>
    /// Descarga la plantilla Excel de categorías y subcategorías.
    /// </summary>
    public FileContentResult OnGetDownloadExcelTemplate()
        => File(CategoryExcelTemplateProvider.BuildTemplateBytes(), CategoryExcelTemplateProvider.ContentType, CategoryExcelTemplateProvider.FileName);

    /// <summary>
    /// Procesa la importación XML de categorías desde el backoffice.
    /// </summary>
    public async Task<IActionResult> OnPostImportAsync(CancellationToken cancellationToken)
    {
        if (ImportInput.ImportFile is null || ImportInput.ImportFile.Length == 0)
        {
            StatusErrorMessage = "Seleccione un archivo válido para importar categorías.";
            return RedirectToPage("./Index");
        }

        if (ImportInput.ImportFile.Length > MaxImportFileSizeInBytes)
        {
            StatusErrorMessage = "El archivo de categorías supera el tamaño máximo permitido de 1 MB.";
            return RedirectToPage("./Index");
        }

        var conversionResult = await CategoryImportFileConverter.ConvertToXmlAsync(ImportInput.ImportFile, cancellationToken);
        if (conversionResult.IsFailure)
        {
            StatusErrorMessage = conversionResult.Error.Message;
            return RedirectToPage("./Index");
        }

        var result = await _categoryApplicationService.ImportCategoriesFromXmlAsync(
            new ImportCategoriesFromXmlCommand
            {
                XmlContent = conversionResult.Value
            },
            cancellationToken);

        if (result.IsFailure)
        {
            StatusErrorMessage = result.Error.Message;
            return RedirectToPage("./Index");
        }

        SuccessMessage = $"Importación completada correctamente. Categorías principales creadas: {result.Value.RootCategoriesCreated}. Subcategorías creadas: {result.Value.SubcategoriesCreated}.";
        return RedirectToPage("./Index");
    }

    /// <summary>
    /// Activa una categoría desde el listado administrativo.
    /// </summary>
    public Task<IActionResult> OnPostActivateAsync(Guid categoryId, CancellationToken cancellationToken)
        => ChangeStatusAsync(categoryId, true, "Categoría activada correctamente.", cancellationToken);

    /// <summary>
    /// Desactiva una categoría desde el listado administrativo.
    /// </summary>
    public Task<IActionResult> OnPostDeactivateAsync(Guid categoryId, CancellationToken cancellationToken)
        => ChangeStatusAsync(categoryId, false, "Categoría desactivada correctamente.", cancellationToken);

    /// <summary>
    /// Resuelve el nombre de la categoría padre mostrado en la vista.
    /// </summary>
    public string GetParentName(CategoryDto category)
    {
        ArgumentNullException.ThrowIfNull(category);

        if (!category.ParentCategoryId.HasValue)
        {
            return "Categoría principal";
        }

        return Categories.FirstOrDefault(current => current.Id == category.ParentCategoryId.Value)?.Name ?? "Padre no disponible";
    }

    private async Task<IActionResult> ChangeStatusAsync(Guid categoryId, bool isActive, string successMessage, CancellationToken cancellationToken)
    {
        var result = await _categoryApplicationService.ChangeCategoryStatusAsync(
            new ChangeCategoryStatusCommand
            {
                CategoryId = categoryId,
                IsActive = isActive
            },
            cancellationToken);

        if (result.IsFailure)
        {
            StatusErrorMessage = result.Error.Message;
        }
        else
        {
            SuccessMessage = successMessage;
        }

        return RedirectToPage("./Index");
    }

    private async Task LoadCategoriesAsync(CancellationToken cancellationToken)
    {
        var result = await _categoryApplicationService.GetCategoriesAsync(new GetCategoriesQuery(), cancellationToken);
        if (result.IsFailure)
        {
            ErrorMessage = result.Error.Message;
            Categories = Array.Empty<CategoryDto>();
            return;
        }

        Categories = result.Value
            .OrderBy(category => category.ParentCategoryId.HasValue)
            .ThenBy(category => category.Name)
            .ToArray();
    }

    /// <summary>
    /// Representa el formulario de importación de categorías.
    /// </summary>
    public sealed class ImportInputModel
    {
        /// <summary>
        /// Archivo de categorías suministrado por el administrador.
        /// </summary>
        [Display(Name = "Archivo de categorías")]
        public IFormFile? ImportFile { get; set; }
    }
}
