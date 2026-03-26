using System.Reflection;
using Microsoft.EntityFrameworkCore;
using PlataformaECommerce.Application.Interfaces.Repositories.Categories;
using PlataformaECommerce.Domain.Entities.Categories;
using PlataformaECommerce.Infrastructure.Persistence.Context;
using PlataformaECommerce.Infrastructure.Persistence.Entities;

namespace PlataformaECommerce.Infrastructure.Repositories.Categories;

/// <summary>
/// Implementa el repositorio de categorías sobre Entity Framework Core.
/// </summary>
public sealed class CategoryRepository : ICategoryRepository
{
    private readonly ECommerceDbContext _context;

    /// <summary>
    /// Inicializa una nueva instancia del repositorio.
    /// </summary>
    public CategoryRepository(ECommerceDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<CategoriaProducto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<CategoryEntity> entities = await _context.Categories
            .AsNoTracking()
            .OrderBy(category => category.Nombre)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDomain).ToArray();
    }

    /// <inheritdoc />
    public async Task<CategoriaProducto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        CategoryEntity? entity = await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(category => category.Id == id, cancellationToken);

        return entity is null ? null : MapToDomain(entity);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<CategoriaProducto>> GetByParentCategoryIdAsync(Guid? parentCategoryId, CancellationToken cancellationToken = default)
    {
        List<CategoryEntity> entities = await _context.Categories
            .AsNoTracking()
            .Where(category => category.ParentCategoryId == parentCategoryId)
            .OrderBy(category => category.Nombre)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDomain).ToArray();
    }

    /// <inheritdoc />
    public Task<bool> ExistsBySlugAsync(string slug, Guid? excludedCategoryId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return Task.FromResult(false);
        }

        string normalizedSlug = slug.Trim().ToLowerInvariant();

        return _context.Categories.AnyAsync(
            category => category.Slug == normalizedSlug && (!excludedCategoryId.HasValue || category.Id != excludedCategoryId.Value),
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(CategoriaProducto categoria, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(categoria);

        await _context.Categories.AddAsync(MapToEntity(categoria), cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(CategoriaProducto categoria, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(categoria);

        CategoryEntity? entity = await _context.Categories.FirstOrDefaultAsync(category => category.Id == categoria.Id, cancellationToken);
        if (entity is null)
        {
            throw new InvalidOperationException($"No se encontró la categoría con identificador '{categoria.Id}' para actualizar.");
        }

        UpdateEntity(entity, categoria);
    }

    private static CategoryEntity MapToEntity(CategoriaProducto categoria)
    {
        CategoryEntity entity = new();
        UpdateEntity(entity, categoria);
        return entity;
    }

    private static void UpdateEntity(CategoryEntity entity, CategoriaProducto categoria)
    {
        entity.Id = categoria.Id;
        entity.Nombre = categoria.Nombre;
        entity.Slug = categoria.Slug;
        entity.Descripcion = categoria.Descripcion;
        entity.Activa = categoria.Activa;
        entity.ParentCategoryId = categoria.ParentCategoryId;
        entity.FechaCreacionUtc = categoria.FechaCreacionUtc;
        entity.FechaActualizacionUtc = categoria.FechaActualizacionUtc;
    }

    private static CategoriaProducto MapToDomain(CategoryEntity entity)
    {
        CategoriaProducto category = new(entity.Nombre, entity.Slug, entity.Descripcion, entity.ParentCategoryId);
        ApplyPersistenceState(category, entity);
        return category;
    }

    private static void ApplyPersistenceState(CategoriaProducto category, CategoryEntity entity)
    {
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        typeof(CategoriaProducto).GetProperty(nameof(CategoriaProducto.Id), Flags)?.SetValue(category, entity.Id);
        typeof(CategoriaProducto).GetProperty(nameof(CategoriaProducto.Activa), Flags)?.SetValue(category, entity.Activa);
        typeof(CategoriaProducto).GetProperty(nameof(CategoriaProducto.FechaCreacionUtc), Flags)?.SetValue(category, entity.FechaCreacionUtc);
        typeof(CategoriaProducto).GetProperty(nameof(CategoriaProducto.FechaActualizacionUtc), Flags)?.SetValue(category, entity.FechaActualizacionUtc);
    }
}
