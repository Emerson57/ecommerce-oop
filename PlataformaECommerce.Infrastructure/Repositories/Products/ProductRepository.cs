using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using PlataformaECommerce.Application.Features.Products.DTOs;
using PlataformaECommerce.Application.Features.Products.Queries;
using PlataformaECommerce.Application.Interfaces.Repositories.Products;
using PlataformaECommerce.Domain.Entities.Products;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.ValueObjects;
using PlataformaECommerce.Infrastructure.Persistence.Context;
using PlataformaECommerce.Infrastructure.Persistence.Entities;

namespace PlataformaECommerce.Infrastructure.Repositories.Products;

/// <summary>
/// Implementa el repositorio de productos sobre Entity Framework Core.
/// </summary>
/// <remarks>
/// Esta implementación traduce entre el agregado <see cref="Producto"/> y su representación
/// persistente <see cref="ProductEntity"/>, manteniendo consultas orientadas al dominio.
/// </remarks>
public sealed class ProductRepository : IProductRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ECommerceDbContext _context;

    /// <summary>
    /// Inicializa una nueva instancia del repositorio de productos.
    /// </summary>
    /// <param name="context">Contexto EF Core asociado.</param>
    public ProductRepository(ECommerceDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyCollection<ProductDto> Items, int TotalCount)> QueryProductsAsync(
        GetProductsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        IQueryable<ProductEntity> q = _context.Products.AsNoTracking();

        // Filters
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            string term = query.SearchTerm.Trim();
            q = q.Where(p => p.Nombre.Contains(term) || p.Descripcion.Contains(term) || p.Sku.Contains(term) || p.Slug.Contains(term));
        }

        if (query.ProductType.HasValue)
        {
            string type = query.ProductType.Value.ToString();
            q = q.Where(p => p.TipoProducto == type);
        }

        if (query.CategoryId.HasValue)
        {
            Guid cat = query.CategoryId.Value;
            q = q.Where(p => p.CategoriaId == cat || p.SubcategoriaId == cat);
        }

        if (query.IsActive.HasValue)
        {
            q = q.Where(p => p.Activo == query.IsActive.Value);
        }

        if (query.IsFeatured.HasValue)
        {
            q = q.Where(p => p.Destacado == query.IsFeatured.Value);
        }

        if (query.HasStock.HasValue)
        {
            q = query.HasStock.Value ? q.Where(p => p.Stock > 0) : q.Where(p => p.Stock <= 0);
        }

        if (query.MinPrice.HasValue)
        {
            q = q.Where(p => p.Precio >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            q = q.Where(p => p.Precio <= query.MaxPrice.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Currency))
        {
            string currency = query.Currency.Trim().ToUpperInvariant();
            q = q.Where(p => p.Moneda.ToUpper() == currency);
        }

        // Total count before pagination
        int total = await q.CountAsync(cancellationToken);

        // Sorting
        q = query.SortBy?.ToLowerInvariant() switch
        {
            "price" => query.SortDescending ? q.OrderByDescending(p => p.Precio) : q.OrderBy(p => p.Precio),
            "createdat" => query.SortDescending ? q.OrderByDescending(p => p.FechaCreacionUtc) : q.OrderBy(p => p.FechaCreacionUtc),
            "updatedat" => query.SortDescending ? q.OrderByDescending(p => p.FechaActualizacionUtc) : q.OrderBy(p => p.FechaActualizacionUtc),
            "stock" => query.SortDescending ? q.OrderByDescending(p => p.Stock) : q.OrderBy(p => p.Stock),
            _ => query.SortDescending ? q.OrderByDescending(p => p.Nombre) : q.OrderBy(p => p.Nombre),
        };

        // Pagination
        int skip = query.Offset;
        int take = query.NormalizedPageSize;

        var items = await q
            .Skip(skip)
            .Take(take)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Nombre,
                Description = p.Descripcion,
                Sku = p.Sku,
                Price = p.Precio,
                BasePrice = p.PrecioBase,
                PromotionalPrice = p.PrecioPromocionalActual,
                Currency = p.Moneda,
                Stock = p.Stock,
                IsActive = p.Activo,
                IsFeatured = p.Destacado,
                HasPromotion = p.PrecioPromocionalActual.HasValue,
                CurrentDiscountPercentage = p.DescuentoPromocionalActual,
                Slug = p.Slug,
                MainImageUrl = p.ImagenPrincipalUrl,
                // Avoid JSON deserialization inside EF projection to keep translation to SQL.
                // For listings we return empty gallery and keep heavy fields for detail endpoints.
                ImageGallery = Array.Empty<string>(),
                ProductType = p.TipoProducto != null && p.TipoProducto.ToLower() == "digital" ? TipoProducto.Digital : TipoProducto.Fisico,
                CategoryId = p.CategoriaId,
                SubcategoryId = p.SubcategoriaId,
                CreatedAtUtc = p.FechaCreacionUtc,
                UpdatedAtUtc = p.FechaActualizacionUtc,
                WeightKg = p.PesoKg,
                HeightCm = p.AltoCm,
                WidthCm = p.AnchoCm,
                LengthCm = p.LargoCm,
                RequiresShipping = p.RequiereEnvio,
                FileFormat = p.FormatoArchivo,
                FileSizeMb = p.TamanoMB,
                RequiresLicense = p.RequiereLicencia
            })
            .ToArrayAsync(cancellationToken);

        return (items, total);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Producto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<ProductEntity> entities = await _context.Products
            .AsNoTracking()
            .OrderBy(product => product.Nombre)
            .ToListAsync(cancellationToken);

        return entities
            .Select(MapToDomain)
            .ToArray();
    }

    /// <inheritdoc />
    public async Task<Producto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        ProductEntity? entity = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(product => product.Id == id, cancellationToken);

        return entity is null ? null : MapToDomain(entity);
    }

    /// <inheritdoc />
    public async Task<Producto?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            return null;
        }

        string normalizedSku = sku.Trim().ToUpperInvariant();

        ProductEntity? entity = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(product => product.Sku == normalizedSku, cancellationToken);

        return entity is null ? null : MapToDomain(entity);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Producto>> GetActiveProductsAsync(CancellationToken cancellationToken = default)
    {
        List<ProductEntity> entities = await _context.Products
            .AsNoTracking()
            .Where(product => product.Activo)
            .OrderBy(product => product.Nombre)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDomain).ToArray();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Producto>> GetFeaturedProductsAsync(CancellationToken cancellationToken = default)
    {
        List<ProductEntity> entities = await _context.Products
            .AsNoTracking()
            .Where(product => product.Destacado)
            .OrderBy(product => product.Nombre)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDomain).ToArray();
    }

    /// <inheritdoc />
    public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return Task.FromResult(false);
        }

        return _context.Products.AnyAsync(product => product.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> ExistsBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            return Task.FromResult(false);
        }

        string normalizedSku = sku.Trim().ToUpperInvariant();
        return _context.Products.AnyAsync(product => product.Sku == normalizedSku, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Producto producto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(producto);

        ProductEntity entity = MapToEntity(producto);
        await _context.Products.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Producto producto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(producto);

        ProductEntity? entity = await _context.Products
            .FirstOrDefaultAsync(current => current.Id == producto.Id, cancellationToken);

        if (entity is null)
        {
            throw new InvalidOperationException($"No se encontró el producto con identificador '{producto.Id}' para actualizar.");
        }

        UpdateEntityFromDomain(entity, producto);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return;
        }

        ProductEntity? entity = await _context.Products
            .FirstOrDefaultAsync(product => product.Id == id, cancellationToken);

        if (entity is null)
        {
            return;
        }

        _context.Products.Remove(entity);
    }

    private static Producto MapToDomain(ProductEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        Producto product = entity.TipoProducto.Trim().Equals(TipoProducto.Fisico.ToString(), StringComparison.OrdinalIgnoreCase)
             ? new ProductoFisico(
                 entity.Nombre,
                 entity.Descripcion,
                 new Sku(entity.Sku),
                 new Money(entity.Precio, entity.Moneda),
                 entity.Stock,
                 entity.Slug,
                 entity.ImagenPrincipalUrl,
                 entity.CategoriaId,
                 entity.SubcategoriaId,
                 DeserializeTags(entity.EtiquetasSerializadas),
                 entity.PesoKg ?? 0m,
                 entity.AltoCm ?? 0m,
                 entity.AnchoCm ?? 0m,
                 entity.LargoCm ?? 0m,
                 entity.RequiereEnvio ?? true,
                 DeserializeImageGallery(entity.GaleriaImagenesSerializadas))
             : new ProductoDigital(
                 entity.Nombre,
                 entity.Descripcion,
                 new Sku(entity.Sku),
                 new Money(entity.Precio, entity.Moneda),
                 entity.Stock,
                 entity.Slug,
                 entity.ImagenPrincipalUrl,
                 entity.CategoriaId,
                 entity.SubcategoriaId,
                 DeserializeTags(entity.EtiquetasSerializadas),
                 entity.FormatoArchivo ?? string.Empty,
                 entity.TamanoMB,
                 entity.RequiereLicencia ?? false,
                 DeserializeImageGallery(entity.GaleriaImagenesSerializadas));

        ApplyPersistenceState(product, entity);
        return product;
    }

    private static ProductEntity MapToEntity(Producto producto)
    {
        ProductEntity entity = new();
        UpdateEntityFromDomain(entity, producto);
        return entity;
    }

    private static void UpdateEntityFromDomain(ProductEntity entity, Producto producto)
    {
        entity.Id = producto.Id;
        entity.Nombre = producto.Nombre;
        entity.Descripcion = producto.Descripcion;
        entity.Sku = producto.Sku.Value;
        entity.Precio = producto.Precio.Amount;
        entity.PrecioBase = producto.PrecioBase.Amount;
        entity.PrecioPromocionalActual = producto.PrecioPromocionalActual?.Amount;
        entity.DescuentoPromocionalActual = producto.DescuentoPromocionalActual;
        entity.Moneda = producto.Precio.Currency;
        entity.Stock = producto.Stock;
        entity.Activo = producto.Activo;
        entity.Destacado = producto.Destacado;
        entity.TipoProducto = producto.TipoProducto.ToString();
        entity.Slug = producto.Slug;
        entity.ImagenPrincipalUrl = producto.ImagenPrincipalUrl;
        entity.GaleriaImagenesSerializadas = SerializeImageGallery(producto.GaleriaImagenes);
        entity.CategoriaId = producto.CategoriaId;
        entity.SubcategoriaId = producto.SubcategoriaId;
        entity.EtiquetasSerializadas = SerializeTags(producto.Etiquetas);
        entity.FechaCreacionUtc = producto.FechaCreacionUtc;
        entity.FechaActualizacionUtc = producto.FechaActualizacionUtc;
        entity.FormatoArchivo = null;
        entity.TamanoMB = null;
        entity.RequiereLicencia = null;
        entity.PesoKg = null;
        entity.AltoCm = null;
        entity.AnchoCm = null;
        entity.LargoCm = null;
        entity.RequiereEnvio = null;

        if (producto is ProductoDigital digital)
        {
            entity.FormatoArchivo = digital.FormatoArchivo;
            entity.TamanoMB = digital.TamanoArchivoMb;
            entity.RequiereLicencia = digital.RequiereLicencia;
            return;
        }

        if (producto is ProductoFisico physical)
        {
            entity.PesoKg = physical.PesoKg;
            entity.AltoCm = physical.AltoCm;
            entity.AnchoCm = physical.AnchoCm;
            entity.LargoCm = physical.LargoCm;
            entity.RequiereEnvio = physical.RequiereEnvio;
        }
    }

    private static IReadOnlyCollection<EtiquetaProducto> DeserializeTags(string? serializedTags)
    {
        if (string.IsNullOrWhiteSpace(serializedTags))
        {
            return Array.Empty<EtiquetaProducto>();
        }

        string[]? values = JsonSerializer.Deserialize<string[]>(serializedTags, JsonOptions);
        if (values is null || values.Length == 0)
        {
            return Array.Empty<EtiquetaProducto>();
        }

        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => new EtiquetaProducto(value))
            .ToArray();
    }

    private static string? SerializeTags(IEnumerable<EtiquetaProducto> tags)
    {
        string[] values = tags
            .Select(tag => tag.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToArray();

        return values.Length == 0
            ? null
            : JsonSerializer.Serialize(values, JsonOptions);
    }

    private static IReadOnlyCollection<string> DeserializeImageGallery(string? serializedImageGallery)
    {
        if (string.IsNullOrWhiteSpace(serializedImageGallery))
        {
            return Array.Empty<string>();
        }

        string[]? values = JsonSerializer.Deserialize<string[]>(serializedImageGallery, JsonOptions);
        if (values is null || values.Length == 0)
        {
            return Array.Empty<string>();
        }

        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string? SerializeImageGallery(IEnumerable<string> imageGallery)
    {
        string[] values = imageGallery
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return values.Length == 0
            ? null
            : JsonSerializer.Serialize(values, JsonOptions);
    }

    private static void ApplyPersistenceState(Producto product, ProductEntity entity)
    {
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        typeof(Producto).GetProperty(nameof(Producto.Activo), Flags)?.SetValue(product, entity.Activo);
        typeof(Producto).GetProperty(nameof(Producto.Destacado), Flags)?.SetValue(product, entity.Destacado);
        typeof(Producto).GetProperty(nameof(Producto.Id), Flags)?.SetValue(product, entity.Id);
        typeof(Producto).GetProperty(nameof(Producto.FechaCreacionUtc), Flags)?.SetValue(product, entity.FechaCreacionUtc);
        typeof(Producto).GetProperty(nameof(Producto.FechaActualizacionUtc), Flags)?.SetValue(product, entity.FechaActualizacionUtc);
        decimal precioBase = entity.PrecioBase > 0m
            ? entity.PrecioBase
            : entity.Precio;

        typeof(Producto).GetProperty(nameof(Producto.PrecioBase), Flags)?.SetValue(product, new Money(precioBase, entity.Moneda));
        typeof(Producto).GetProperty(nameof(Producto.PrecioPromocionalActual), Flags)?.SetValue(
            product,
            entity.PrecioPromocionalActual.HasValue
                ? new Money(entity.PrecioPromocionalActual.Value, entity.Moneda)
                : null);
        typeof(Producto).GetProperty(nameof(Producto.DescuentoPromocionalActual), Flags)?.SetValue(product, entity.DescuentoPromocionalActual);
        typeof(Producto).GetProperty(nameof(Producto.Precio), Flags)?.SetValue(product, new Money(entity.Precio, entity.Moneda));
    }
}