using System.Reflection;
using Microsoft.EntityFrameworkCore;
using PlataformaECommerce.Application.Interfaces.Repositories.Cart;
using PlataformaECommerce.Domain.Common;
using PlataformaECommerce.Domain.Entities.Cart;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.ValueObjects;
using PlataformaECommerce.Infrastructure.Persistence.Context;
using PlataformaECommerce.Infrastructure.Persistence.Entities;

namespace PlataformaECommerce.Infrastructure.Repositories.Cart;

/// <summary>
/// Implementa el repositorio de carritos sobre Entity Framework Core.
/// </summary>
/// <remarks>
/// Esta implementación traduce entre el agregado <see cref="CarritoCompra"/> y sus
/// proyecciones persistentes, preservando el encabezado del carrito y la instantánea
/// comercial de cada línea sin acoplar el dominio a detalles del ORM.
/// </remarks>
public sealed class CartRepository : ICartRepository
{
    private const BindingFlags ReflectionFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private readonly ECommerceDbContext _context;

    /// <summary>
    /// Inicializa una nueva instancia del repositorio de carritos.
    /// </summary>
    /// <param name="context">Contexto EF Core asociado.</param>
    public CartRepository(ECommerceDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<CarritoCompra>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<CartEntity> entities = await _context.Carts
            .AsNoTracking()
            .Include(cart => cart.Items)
            .OrderByDescending(cart => cart.FechaCreacionUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDomain).ToArray();
    }

    /// <inheritdoc />
    public async Task<CarritoCompra?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        CartEntity? entity = await _context.Carts
            .AsNoTracking()
            .Include(cart => cart.Items)
            .FirstOrDefaultAsync(cart => cart.Id == id, cancellationToken);

        return entity is null ? null : MapToDomain(entity);
    }

    /// <inheritdoc />
    public async Task<CarritoCompra?> GetByCustomerIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
    {
        if (clienteId == Guid.Empty)
        {
            return null;
        }

        CartEntity? entity = await _context.Carts
            .AsNoTracking()
            .Include(cart => cart.Items)
            .Where(cart => cart.ClienteId == clienteId)
            .OrderByDescending(cart => cart.Activo)
            .ThenByDescending(cart => cart.FechaCreacionUtc)
            .FirstOrDefaultAsync(cancellationToken);

        return entity is null ? null : MapToDomain(entity);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<CarritoCompra>> GetAllByCustomerIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
    {
        if (clienteId == Guid.Empty)
        {
            return Array.Empty<CarritoCompra>();
        }

        List<CartEntity> entities = await _context.Carts
            .AsNoTracking()
            .Include(cart => cart.Items)
            .Where(cart => cart.ClienteId == clienteId)
            .OrderByDescending(cart => cart.FechaCreacionUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDomain).ToArray();
    }

    /// <inheritdoc />
    public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return id == Guid.Empty
            ? Task.FromResult(false)
            : _context.Carts.AnyAsync(cart => cart.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> ExistsByCustomerIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
    {
        return clienteId == Guid.Empty
            ? Task.FromResult(false)
            : _context.Carts.AnyAsync(cart => cart.ClienteId == clienteId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(CarritoCompra carrito, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(carrito);

        CartEntity entity = MapToEntity(carrito);
        await _context.Carts.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(CarritoCompra carrito, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(carrito);

        CartEntity? entity = await _context.Carts
            .Include(cart => cart.Items)
            .FirstOrDefaultAsync(current => current.Id == carrito.Id, cancellationToken);

        if (entity is null)
        {
            throw new InvalidOperationException($"No se encontró el carrito con identificador '{carrito.Id}' para actualizar.");
        }

        UpdateEntityFromDomain(entity, carrito);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return;
        }

        CartEntity? entity = await _context.Carts
            .Include(cart => cart.Items)
            .FirstOrDefaultAsync(cart => cart.Id == id, cancellationToken);

        if (entity is null)
        {
            return;
        }

        _context.Carts.Remove(entity);
    }

    private static CarritoCompra MapToDomain(CartEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        CarritoCompra cart = new(entity.ClienteId);
        ApplyCartPersistenceState(cart, entity);

        List<ItemCarrito> items = GetFieldValue<List<ItemCarrito>>(cart, "_items");
        items.Clear();
        items.AddRange(entity.Items.Select(MapToDomain));

        return cart;
    }

    private static ItemCarrito MapToDomain(CartItemEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (!Enum.TryParse(entity.TipoProducto, ignoreCase: true, out TipoProducto productType))
        {
            throw new InvalidOperationException($"El tipo de producto persistido '{entity.TipoProducto}' no es válido para un ítem de carrito.");
        }

        ItemCarrito item = Activator.CreateInstance(typeof(ItemCarrito), nonPublic: true) as ItemCarrito
            ?? throw new InvalidOperationException("No fue posible crear una instancia del ítem de carrito para su rehidratación.");

        SetPropertyValue(typeof(ItemCarrito), item, nameof(ItemCarrito.Id), entity.Id);
        SetPropertyValue(typeof(ItemCarrito), item, nameof(ItemCarrito.ProductoId), entity.ProductoId);
        SetPropertyValue(typeof(ItemCarrito), item, nameof(ItemCarrito.NombreProducto), entity.NombreProducto);
        SetPropertyValue(typeof(ItemCarrito), item, nameof(ItemCarrito.SkuProducto), new Sku(entity.SkuProducto));
        SetPropertyValue(typeof(ItemCarrito), item, nameof(ItemCarrito.TipoProducto), productType);
        SetPropertyValue(typeof(ItemCarrito), item, nameof(ItemCarrito.ImagenPrincipalUrl), entity.ImagenPrincipalUrl);
        SetPropertyValue(typeof(ItemCarrito), item, nameof(ItemCarrito.PrecioUnitario), new Money(entity.PrecioUnitario, entity.Moneda));
        SetPropertyValue(typeof(ItemCarrito), item, nameof(ItemCarrito.Cantidad), entity.Cantidad);
        SetPropertyValue(typeof(ItemCarrito), item, nameof(ItemCarrito.FechaCreacionUtc), entity.FechaCreacionUtc);
        SetPropertyValue(typeof(ItemCarrito), item, nameof(ItemCarrito.FechaActualizacionUtc), entity.FechaActualizacionUtc);

        return item;
    }

    private static CartEntity MapToEntity(CarritoCompra carrito)
    {
        CartEntity entity = new();
        UpdateEntityFromDomain(entity, carrito);
        return entity;
    }

    private static CartItemEntity MapToEntity(ItemCarrito item, Guid cartId)
    {
        return new CartItemEntity
        {
            Id = item.Id,
            TenantId = string.Empty,
            CartId = cartId,
            ProductoId = item.ProductoId,
            NombreProducto = item.NombreProducto,
            SkuProducto = item.SkuProducto.Value,
            TipoProducto = item.TipoProducto.ToString(),
            ImagenPrincipalUrl = item.ImagenPrincipalUrl,
            PrecioUnitario = item.PrecioUnitario.Amount,
            Moneda = item.PrecioUnitario.Currency,
            Cantidad = item.Cantidad,
            FechaCreacionUtc = item.FechaCreacionUtc,
            FechaActualizacionUtc = item.FechaActualizacionUtc
        };
    }

    private static void UpdateEntityFromDomain(CartEntity entity, CarritoCompra carrito)
    {
        entity.Id = carrito.Id;
        entity.ClienteId = carrito.ClienteId;
        entity.Activo = carrito.Activo;
        entity.FechaCreacionUtc = carrito.FechaCreacionUtc;
        entity.FechaActualizacionUtc = carrito.FechaActualizacionUtc;

        Dictionary<Guid, CartItemEntity> currentItems = entity.Items.ToDictionary(item => item.Id);
        HashSet<Guid> incomingIds = carrito.Items.Select(item => item.Id).ToHashSet();

        foreach (CartItemEntity itemToRemove in entity.Items.Where(item => !incomingIds.Contains(item.Id)).ToArray())
        {
            entity.Items.Remove(itemToRemove);
        }

        foreach (ItemCarrito item in carrito.Items)
        {
            if (currentItems.TryGetValue(item.Id, out CartItemEntity? currentItem))
            {
                currentItem.TenantId = entity.TenantId;
                currentItem.ProductoId = item.ProductoId;
                currentItem.NombreProducto = item.NombreProducto;
                currentItem.SkuProducto = item.SkuProducto.Value;
                currentItem.TipoProducto = item.TipoProducto.ToString();
                currentItem.ImagenPrincipalUrl = item.ImagenPrincipalUrl;
                currentItem.PrecioUnitario = item.PrecioUnitario.Amount;
                currentItem.Moneda = item.PrecioUnitario.Currency;
                currentItem.Cantidad = item.Cantidad;
                currentItem.FechaCreacionUtc = item.FechaCreacionUtc;
                currentItem.FechaActualizacionUtc = item.FechaActualizacionUtc;
                continue;
            }

            CartItemEntity itemEntity = MapToEntity(item, carrito.Id);
            itemEntity.TenantId = entity.TenantId;
            entity.Items.Add(itemEntity);
        }
    }

    private static void ApplyCartPersistenceState(CarritoCompra cart, CartEntity entity)
    {
        SetPropertyValue(typeof(AggregateRoot), cart, nameof(AggregateRoot.Id), entity.Id);
        SetPropertyValue(typeof(AggregateRoot), cart, nameof(AggregateRoot.FechaCreacionUtc), entity.FechaCreacionUtc);
        SetPropertyValue(typeof(AggregateRoot), cart, nameof(AggregateRoot.FechaActualizacionUtc), entity.FechaActualizacionUtc);
        SetPropertyValue(typeof(CarritoCompra), cart, nameof(CarritoCompra.ClienteId), entity.ClienteId);
        SetPropertyValue(typeof(CarritoCompra), cart, nameof(CarritoCompra.Activo), entity.Activo);
    }

    private static TField GetFieldValue<TField>(object instance, string fieldName)
        where TField : class
    {
        FieldInfo? field = instance.GetType().GetField(fieldName, ReflectionFlags);
        object? value = field?.GetValue(instance);

        return value as TField
            ?? throw new InvalidOperationException($"No se pudo acceder al campo '{fieldName}' durante la rehidratación del agregado de carrito.");
    }

    private static void SetPropertyValue(Type declaringType, object instance, string propertyName, object? value)
    {
        PropertyInfo? property = declaringType.GetProperty(propertyName, ReflectionFlags);
        if (property is null)
        {
            throw new InvalidOperationException($"No se pudo acceder a la propiedad '{propertyName}' durante la rehidratación del agregado de carrito.");
        }

        property.SetValue(instance, value);
    }
}
