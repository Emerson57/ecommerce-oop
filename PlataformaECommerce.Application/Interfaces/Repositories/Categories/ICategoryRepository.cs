using PlataformaECommerce.Domain.Entities.Categories;

namespace PlataformaECommerce.Application.Interfaces.Repositories.Categories;

/// <summary>
/// Define el contrato del repositorio responsable de persistir y recuperar categorías de producto.
/// </summary>
public interface ICategoryRepository
{
    /// <summary>
    /// Obtiene todas las categorías registradas.
    /// </summary>
    Task<IReadOnlyCollection<CategoriaProducto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene una categoría por su identificador.
    /// </summary>
    Task<CategoriaProducto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene las categorías hijas de una categoría padre o las categorías raíz cuando el padre es nulo.
    /// </summary>
    Task<IReadOnlyCollection<CategoriaProducto>> GetByParentCategoryIdAsync(Guid? parentCategoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si ya existe una categoría con el slug indicado.
    /// </summary>
    Task<bool> ExistsBySlugAsync(string slug, Guid? excludedCategoryId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Agrega una nueva categoría al repositorio.
    /// </summary>
    Task AddAsync(CategoriaProducto categoria, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza una categoría existente en el repositorio.
    /// </summary>
    Task UpdateAsync(CategoriaProducto categoria, CancellationToken cancellationToken = default);
}
