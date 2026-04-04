using System.Reflection;
using Microsoft.EntityFrameworkCore;
using PlataformaECommerce.Application.Interfaces.Repositories.Orders;
using PlataformaECommerce.Domain.Common;
using PlataformaECommerce.Domain.Entities.Orders;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.ValueObjects;
using PlataformaECommerce.Infrastructure.Persistence.Context;
using PlataformaECommerce.Infrastructure.Persistence.Entities;

namespace PlataformaECommerce.Infrastructure.Repositories.Orders;

/// <summary>
/// Implementa el repositorio de pedidos sobre Entity Framework Core.
/// </summary>
/// <remarks>
/// Esta implementación traduce entre el agregado <see cref="Pedido"/> y sus proyecciones
/// persistentes, preservando el encabezado del pedido, su dirección de envío y la
/// instantánea comercial de cada detalle sin acoplar el dominio a detalles del ORM.
/// </remarks>
public sealed class OrderRepository : IOrderRepository
{
    private const BindingFlags ReflectionFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private readonly ECommerceDbContext _context;

    /// <summary>
    /// Inicializa una nueva instancia del repositorio de pedidos.
    /// </summary>
    /// <param name="context">Contexto EF Core asociado.</param>
    public OrderRepository(ECommerceDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Pedido>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<OrderEntity> entities = await _context.Orders
            .AsNoTracking()
            .Include(order => order.Detalles)
            .OrderByDescending(order => order.FechaCreacionUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDomain).ToArray();
    }

    /// <inheritdoc />
    public async Task<Pedido?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        OrderEntity? entity = await _context.Orders
            .AsNoTracking()
            .Include(order => order.Detalles)
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);

        return entity is null ? null : MapToDomain(entity);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Pedido>> GetByCustomerIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
    {
        if (clienteId == Guid.Empty)
        {
            return Array.Empty<Pedido>();
        }

        List<OrderEntity> entities = await _context.Orders
            .AsNoTracking()
            .Include(order => order.Detalles)
            .Where(order => order.ClienteId == clienteId)
            .OrderByDescending(order => order.FechaCreacionUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDomain).ToArray();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Pedido>> GetByStatusAsync(EstadoPedido estado, CancellationToken cancellationToken = default)
    {
        string status = estado.ToString();

        List<OrderEntity> entities = await _context.Orders
            .AsNoTracking()
            .Include(order => order.Detalles)
            .Where(order => order.Estado == status)
            .OrderByDescending(order => order.FechaCreacionUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDomain).ToArray();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Pedido>> GetByCustomerIdAndStatusAsync(Guid clienteId, EstadoPedido estado, CancellationToken cancellationToken = default)
    {
        if (clienteId == Guid.Empty)
        {
            return Array.Empty<Pedido>();
        }

        string status = estado.ToString();

        List<OrderEntity> entities = await _context.Orders
            .AsNoTracking()
            .Include(order => order.Detalles)
            .Where(order => order.ClienteId == clienteId && order.Estado == status)
            .OrderByDescending(order => order.FechaCreacionUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDomain).ToArray();
    }

    /// <inheritdoc />
    public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return id == Guid.Empty
            ? Task.FromResult(false)
            : _context.Orders.AnyAsync(order => order.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> ExistsByCustomerIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
    {
        return clienteId == Guid.Empty
            ? Task.FromResult(false)
            : _context.Orders.AnyAsync(order => order.ClienteId == clienteId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Pedido pedido, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pedido);

        OrderEntity entity = MapToEntity(pedido);
        await _context.Orders.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Pedido pedido, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pedido);

        OrderEntity? entity = await _context.Orders
            .Include(order => order.Detalles)
            .FirstOrDefaultAsync(current => current.Id == pedido.Id, cancellationToken);

        if (entity is null)
        {
            throw new InvalidOperationException($"No se encontró el pedido con identificador '{pedido.Id}' para actualizar.");
        }

        UpdateEntityFromDomain(entity, pedido);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return;
        }

        OrderEntity? entity = await _context.Orders
            .Include(order => order.Detalles)
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);

        if (entity is null)
        {
            return;
        }

        _context.Orders.Remove(entity);
    }

    private static Pedido MapToDomain(OrderEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (!Enum.TryParse(entity.Estado, ignoreCase: true, out EstadoPedido orderStatus))
        {
            throw new InvalidOperationException($"El estado persistido '{entity.Estado}' no es válido para un pedido.");
        }

        Pedido order = Activator.CreateInstance(typeof(Pedido), nonPublic: true) as Pedido
            ?? throw new InvalidOperationException("No fue posible crear una instancia del pedido para su rehidratación.");

        ApplyOrderPersistenceState(order, entity, orderStatus);

        List<DetallePedido> details = GetFieldValue<List<DetallePedido>>(order, "_detalles");
        details.Clear();
        details.AddRange(entity.Detalles.Select(MapToDomain));

        return order;
    }

    private static DetallePedido MapToDomain(OrderItemEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (!Enum.TryParse(entity.TipoProducto, ignoreCase: true, out TipoProducto productType))
        {
            throw new InvalidOperationException($"El tipo de producto persistido '{entity.TipoProducto}' no es válido para un detalle de pedido.");
        }

        DetallePedido detail = Activator.CreateInstance(typeof(DetallePedido), nonPublic: true) as DetallePedido
            ?? throw new InvalidOperationException("No fue posible crear una instancia del detalle del pedido para su rehidratación.");

        SetPropertyValue(typeof(DetallePedido), detail, nameof(DetallePedido.Id), entity.Id);
        SetPropertyValue(typeof(DetallePedido), detail, nameof(DetallePedido.PedidoId), entity.PedidoId);
        SetPropertyValue(typeof(DetallePedido), detail, nameof(DetallePedido.ProductoId), entity.ProductoId);
        SetPropertyValue(typeof(DetallePedido), detail, nameof(DetallePedido.NombreProducto), entity.NombreProducto);
        SetPropertyValue(typeof(DetallePedido), detail, nameof(DetallePedido.SkuProducto), new Sku(entity.SkuProducto));
        SetPropertyValue(typeof(DetallePedido), detail, nameof(DetallePedido.TipoProducto), productType);
        SetPropertyValue(typeof(DetallePedido), detail, nameof(DetallePedido.ImagenPrincipalUrl), entity.ImagenPrincipalUrl);
        SetPropertyValue(typeof(DetallePedido), detail, nameof(DetallePedido.PrecioUnitario), new Money(entity.PrecioUnitario, entity.Moneda));
        SetPropertyValue(typeof(DetallePedido), detail, nameof(DetallePedido.Cantidad), entity.Cantidad);
        SetPropertyValue(typeof(DetallePedido), detail, nameof(DetallePedido.FechaCreacionUtc), entity.FechaCreacionUtc);

        return detail;
    }

    private static OrderEntity MapToEntity(Pedido pedido)
    {
        OrderEntity entity = new();
        UpdateEntityFromDomain(entity, pedido);
        return entity;
    }

    private static OrderItemEntity MapToEntity(DetallePedido detail)
    {
        return new OrderItemEntity
        {
            Id = detail.Id,
            PedidoId = detail.PedidoId,
            ProductoId = detail.ProductoId,
            NombreProducto = detail.NombreProducto,
            SkuProducto = detail.SkuProducto.Value,
            TipoProducto = detail.TipoProducto.ToString(),
            ImagenPrincipalUrl = detail.ImagenPrincipalUrl,
            PrecioUnitario = detail.PrecioUnitario.Amount,
            Moneda = detail.PrecioUnitario.Currency,
            Cantidad = detail.Cantidad,
            FechaCreacionUtc = detail.FechaCreacionUtc
        };
    }

    private static void UpdateEntityFromDomain(OrderEntity entity, Pedido pedido)
    {
        entity.Id = pedido.Id;
        entity.ClienteId = pedido.ClienteId;
        entity.Estado = pedido.Estado.ToString();
        entity.FechaCreacionUtc = pedido.FechaCreacionUtc;
        entity.FechaActualizacionUtc = pedido.FechaActualizacionUtc;
        entity.FechaConfirmacionUtc = pedido.FechaConfirmacionUtc;
        entity.FechaPagoUtc = pedido.FechaPagoUtc;
        entity.FechaEnvioUtc = pedido.FechaEnvioUtc;
        entity.FechaEntregaUtc = pedido.FechaEntregaUtc;
        entity.FechaCancelacionUtc = pedido.FechaCancelacionUtc;
        entity.ObservacionCancelacion = pedido.ObservacionCancelacion;
        entity.MetodoPagoSeleccionado = pedido.MetodoPagoSeleccionado?.ToString();

        if (pedido.DireccionEnvio is null)
        {
            entity.DireccionCalle = null;
            entity.DireccionCiudad = null;
            entity.DireccionDepartamento = null;
            entity.DireccionPais = null;
            entity.DireccionCodigoPostal = null;
        }
        else
        {
            entity.DireccionCalle = pedido.DireccionEnvio.Calle;
            entity.DireccionCiudad = pedido.DireccionEnvio.Ciudad;
            entity.DireccionDepartamento = pedido.DireccionEnvio.Departamento;
            entity.DireccionPais = pedido.DireccionEnvio.Pais;
            entity.DireccionCodigoPostal = pedido.DireccionEnvio.CodigoPostal;
        }

        Dictionary<Guid, OrderItemEntity> currentDetails = entity.Detalles.ToDictionary(detail => detail.Id);
        HashSet<Guid> incomingIds = pedido.Detalles.Select(detail => detail.Id).ToHashSet();

        foreach (OrderItemEntity detailToRemove in entity.Detalles.Where(detail => !incomingIds.Contains(detail.Id)).ToArray())
        {
            entity.Detalles.Remove(detailToRemove);
        }

        foreach (DetallePedido detail in pedido.Detalles)
        {
            if (currentDetails.TryGetValue(detail.Id, out OrderItemEntity? currentDetail))
            {
                currentDetail.PedidoId = detail.PedidoId;
                currentDetail.ProductoId = detail.ProductoId;
                currentDetail.NombreProducto = detail.NombreProducto;
                currentDetail.SkuProducto = detail.SkuProducto.Value;
                currentDetail.TipoProducto = detail.TipoProducto.ToString();
                currentDetail.ImagenPrincipalUrl = detail.ImagenPrincipalUrl;
                currentDetail.PrecioUnitario = detail.PrecioUnitario.Amount;
                currentDetail.Moneda = detail.PrecioUnitario.Currency;
                currentDetail.Cantidad = detail.Cantidad;
                currentDetail.FechaCreacionUtc = detail.FechaCreacionUtc;
                continue;
            }

            entity.Detalles.Add(MapToEntity(detail));
        }
    }

    private static void ApplyOrderPersistenceState(Pedido order, OrderEntity entity, EstadoPedido orderStatus)
    {
        SetPropertyValue(typeof(AggregateRoot), order, nameof(AggregateRoot.Id), entity.Id);
        SetPropertyValue(typeof(AggregateRoot), order, nameof(AggregateRoot.FechaCreacionUtc), entity.FechaCreacionUtc);
        SetPropertyValue(typeof(AggregateRoot), order, nameof(AggregateRoot.FechaActualizacionUtc), entity.FechaActualizacionUtc);
        SetPropertyValue(typeof(Pedido), order, nameof(Pedido.ClienteId), entity.ClienteId);
        SetPropertyValue(typeof(Pedido), order, nameof(Pedido.Estado), orderStatus);
        SetPropertyValue(typeof(Pedido), order, nameof(Pedido.FechaConfirmacionUtc), entity.FechaConfirmacionUtc);
        SetPropertyValue(typeof(Pedido), order, nameof(Pedido.FechaPagoUtc), entity.FechaPagoUtc);
        SetPropertyValue(typeof(Pedido), order, nameof(Pedido.FechaEnvioUtc), entity.FechaEnvioUtc);
        SetPropertyValue(typeof(Pedido), order, nameof(Pedido.FechaEntregaUtc), entity.FechaEntregaUtc);
        SetPropertyValue(typeof(Pedido), order, nameof(Pedido.FechaCancelacionUtc), entity.FechaCancelacionUtc);
        SetPropertyValue(typeof(Pedido), order, nameof(Pedido.ObservacionCancelacion), entity.ObservacionCancelacion);
        SetPropertyValue(typeof(Pedido), order, nameof(Pedido.DireccionEnvio), TryBuildShippingAddress(entity));
        SetPropertyValue(typeof(Pedido), order, nameof(Pedido.MetodoPagoSeleccionado), TryBuildPaymentMethod(entity.MetodoPagoSeleccionado));
    }

    private static MetodoPagoPedido? TryBuildPaymentMethod(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Enum.TryParse(value, ignoreCase: true, out MetodoPagoPedido paymentMethod)
            ? paymentMethod
            : throw new InvalidOperationException($"El método de pago persistido '{value}' no es válido para un pedido.");
    }

    private static DireccionEnvio? TryBuildShippingAddress(OrderEntity entity)
    {
        string?[] values =
        {
            entity.DireccionCalle,
            entity.DireccionCiudad,
            entity.DireccionDepartamento,
            entity.DireccionPais,
            entity.DireccionCodigoPostal
        };

        if (values.All(string.IsNullOrWhiteSpace))
        {
            return null;
        }

        if (values.Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidOperationException("La dirección de envío persistida para el pedido se encuentra incompleta.");
        }

        return new DireccionEnvio(
            entity.DireccionCalle!,
            entity.DireccionCiudad!,
            entity.DireccionDepartamento!,
            entity.DireccionPais!,
            entity.DireccionCodigoPostal!);
    }

    private static TField GetFieldValue<TField>(object instance, string fieldName)
        where TField : class
    {
        FieldInfo? field = instance.GetType().GetField(fieldName, ReflectionFlags);
        object? value = field?.GetValue(instance);

        return value as TField
            ?? throw new InvalidOperationException($"No se pudo acceder al campo '{fieldName}' durante la rehidratación del agregado de pedidos.");
    }

    private static void SetPropertyValue(Type declaringType, object instance, string propertyName, object? value)
    {
        PropertyInfo? property = declaringType.GetProperty(propertyName, ReflectionFlags);
        if (property is null)
        {
            throw new InvalidOperationException($"No se pudo acceder a la propiedad '{propertyName}' durante la rehidratación del agregado de pedidos.");
        }

        property.SetValue(instance, value);
    }
}
