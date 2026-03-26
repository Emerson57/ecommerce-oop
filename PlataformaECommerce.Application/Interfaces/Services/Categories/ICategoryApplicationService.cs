using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Categories.Commands;
using PlataformaECommerce.Application.Features.Categories.DTOs;
using PlataformaECommerce.Application.Features.Categories.Queries;

namespace PlataformaECommerce.Application.Interfaces.Services.Categories;

/// <summary>
/// Define la frontera pública de los casos de uso del módulo de categorías.
/// </summary>
public interface ICategoryApplicationService
{
    /// <summary>
    /// Obtiene categorías aplicando filtros administrativos o de catálogo.
    /// </summary>
    Task<Result<IReadOnlyCollection<CategoryDto>>> GetCategoriesAsync(GetCategoriesQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene una categoría por su identificador.
    /// </summary>
    Task<Result<CategoryDto>> GetCategoryByIdAsync(GetCategoryByIdQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra una nueva categoría dentro del catálogo.
    /// </summary>
    Task<Result<Guid>> CreateCategoryAsync(CreateCategoryCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Importa categorías y subcategorías desde un archivo XML validado.
    /// </summary>
    Task<Result<CategoryImportResultDto>> ImportCategoriesFromXmlAsync(ImportCategoriesFromXmlCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza una categoría existente.
    /// </summary>
    Task<Result<CategoryDto>> UpdateCategoryAsync(UpdateCategoryCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cambia el estado operativo de una categoría.
    /// </summary>
    Task<Result<CategoryDto>> ChangeCategoryStatusAsync(ChangeCategoryStatusCommand command, CancellationToken cancellationToken = default);
}
