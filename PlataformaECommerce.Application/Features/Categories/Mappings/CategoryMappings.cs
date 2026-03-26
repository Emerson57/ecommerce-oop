using PlataformaECommerce.Application.Features.Categories.DTOs;
using PlataformaECommerce.Domain.Entities.Categories;

namespace PlataformaECommerce.Application.Features.Categories.Mappings;

/// <summary>
/// Proporciona las proyecciones entre la entidad de dominio de categorías y sus DTOs de lectura.
/// </summary>
public static class CategoryMappings
{
    /// <summary>
    /// Proyecta una categoría del dominio hacia un DTO de lectura.
    /// </summary>
    public static CategoryDto ToCategoryDto(this CategoriaProducto category)
    {
        ArgumentNullException.ThrowIfNull(category);

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Nombre,
            Slug = category.Slug,
            Description = category.Descripcion,
            ParentCategoryId = category.ParentCategoryId,
            IsActive = category.Activa,
            IsRootCategory = category.EsCategoriaRaiz
        };
    }
}
