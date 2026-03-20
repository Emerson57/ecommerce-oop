using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
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
                entity.RequiereEnvio ?? true)
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
                entity.RequiereLicencia ?? false);

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