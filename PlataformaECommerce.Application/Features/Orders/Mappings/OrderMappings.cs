using PlataformaECommerce.Application.Features.Orders.DTOs;
using PlataformaECommerce.Domain.Entities.Orders;

namespace PlataformaECommerce.Application.Features.Orders.Mappings;

/// <summary>
/// Proporciona métodos de extensión para mapear entidades del dominio de pedidos
/// hacia objetos de transferencia de datos de la capa Application.
/// </summary>
/// <remarks>
/// Esta clase centraliza la lógica de proyección de las entidades
/// <see cref="Pedido"/> y <see cref="DetallePedido"/> hacia DTOs de lectura,
/// evitando duplicación de código en:
/// 
/// - servicios de aplicación,
/// - páginas y controladores consumidores,
/// - y otros componentes de orquestación.
///
/// Su propósito es:
/// 
/// - mantener consistencia en las proyecciones,
/// - desacoplar la capa Application del detalle interno del dominio,
/// - facilitar mantenimiento y evolución del modelo,
/// - y mejorar la legibilidad de los casos de uso.
///
/// La clase funciona como un mapper manual explícito, manteniendo control
/// total sobre el proceso de transformación y evitando dependencias externas
/// innecesarias para este tipo de proyecciones.
/// </remarks>
public static class OrderMappings
{
    #region Mapeos individuales

    /// <summary>
    /// Proyecta una entidad <see cref="Pedido"/> hacia un <see cref="OrderDto"/>
    /// incluyendo el detalle de sus ítems.
    /// </summary>
    /// <param name="order">Pedido a proyectar.</param>
    /// <returns>DTO resumido del pedido.</returns>
    /// <exception cref="ArgumentNullException">
    /// Se produce cuando el pedido es nulo.
    /// </exception>
    public static OrderDto ToOrderDto(this Pedido order)
    {
        ArgumentNullException.ThrowIfNull(order);

        return new OrderDto
        {
            Id = order.Id,
            CustomerId = order.ClienteId,
            Status = order.Estado,
            Items = order.Detalles.ToOrderItemDtos(),
            ItemsCount = order.CantidadDetalles,
            TotalUnits = order.CantidadTotalUnidades,
            TotalAmount = order.Total.Amount,
            Currency = order.Total.Currency,
            CreatedAtUtc = order.FechaCreacionUtc,
            UpdatedAtUtc = order.FechaActualizacionUtc,
            ConfirmedAtUtc = order.FechaConfirmacionUtc,
            PaidAtUtc = order.FechaPagoUtc,
            ShippedAtUtc = order.FechaEnvioUtc,
            DeliveredAtUtc = order.FechaEntregaUtc,
            CancelledAtUtc = order.FechaCancelacionUtc,
            CancellationReason = order.ObservacionCancelacion,
            PaymentMethod = order.MetodoPagoSeleccionado
        };
    }

    /// <summary>
    /// Proyecta una entidad <see cref="Pedido"/> hacia un <see cref="OrderDto"/>
    /// omitiendo el detalle de los ítems del pedido.
    /// </summary>
    /// <param name="order">Pedido a proyectar.</param>
    /// <returns>DTO resumido del pedido sin ítems.</returns>
    /// <exception cref="ArgumentNullException">
    /// Se produce cuando el pedido es nulo.
    /// </exception>
    public static OrderDto ToOrderDtoWithoutItems(this Pedido order)
    {
        ArgumentNullException.ThrowIfNull(order);

        return new OrderDto
        {
            Id = order.Id,
            CustomerId = order.ClienteId,
            Status = order.Estado,
            Items = Array.Empty<OrderItemDto>(),
            ItemsCount = order.CantidadDetalles,
            TotalUnits = order.CantidadTotalUnidades,
            TotalAmount = order.Total.Amount,
            Currency = order.Total.Currency,
            CreatedAtUtc = order.FechaCreacionUtc,
            UpdatedAtUtc = order.FechaActualizacionUtc,
            ConfirmedAtUtc = order.FechaConfirmacionUtc,
            PaidAtUtc = order.FechaPagoUtc,
            ShippedAtUtc = order.FechaEnvioUtc,
            DeliveredAtUtc = order.FechaEntregaUtc,
            CancelledAtUtc = order.FechaCancelacionUtc,
            CancellationReason = order.ObservacionCancelacion,
            PaymentMethod = order.MetodoPagoSeleccionado
        };
    }

    /// <summary>
    /// Proyecta una entidad <see cref="Pedido"/> hacia un <see cref="OrderDetailDto"/>
    /// incluyendo el detalle completo de sus ítems.
    /// </summary>
    /// <param name="order">Pedido a proyectar.</param>
    /// <returns>DTO detallado del pedido.</returns>
    /// <exception cref="ArgumentNullException">
    /// Se produce cuando el pedido es nulo.
    /// </exception>
    public static OrderDetailDto ToOrderDetailDto(this Pedido order)
    {
        ArgumentNullException.ThrowIfNull(order);

        IReadOnlyCollection<OrderItemDto> items = order.Detalles.ToOrderItemDtos();

        return new OrderDetailDto
        {
            Id = order.Id,
            CustomerId = order.ClienteId,
            Status = order.Estado,
            Items = items,
            ItemsCount = order.CantidadDetalles,
            TotalUnits = order.CantidadTotalUnidades,
            TotalAmount = order.Total.Amount,
            Currency = order.Total.Currency,
            CreatedAtUtc = order.FechaCreacionUtc,
            UpdatedAtUtc = order.FechaActualizacionUtc,
            ConfirmedAtUtc = order.FechaConfirmacionUtc,
            PaidAtUtc = order.FechaPagoUtc,
            ShippedAtUtc = order.FechaEnvioUtc,
            DeliveredAtUtc = order.FechaEntregaUtc,
            CancelledAtUtc = order.FechaCancelacionUtc,
            CancellationReason = order.ObservacionCancelacion,
            PaymentMethod = order.MetodoPagoSeleccionado,
            ShippingStreet = order.DireccionEnvio?.Calle,
            ShippingCity = order.DireccionEnvio?.Ciudad,
            ShippingRegion = order.DireccionEnvio?.Departamento,
            ShippingCountry = order.DireccionEnvio?.Pais,
            ShippingPostalCode = order.DireccionEnvio?.CodigoPostal,
            ContainsPhysicalProducts = order.ContieneProductosFisicos(),
            ContainsDigitalProducts = order.ContieneProductosDigitales()
        };
    }

    /// <summary>
    /// Proyecta una entidad <see cref="Pedido"/> hacia un <see cref="OrderDetailDto"/>
    /// omitiendo el detalle de los ítems para escenarios donde se requiere
    /// una respuesta más liviana.
    /// </summary>
    /// <param name="order">Pedido a proyectar.</param>
    /// <returns>DTO detallado del pedido sin ítems cargados.</returns>
    /// <exception cref="ArgumentNullException">
    /// Se produce cuando el pedido es nulo.
    /// </exception>
    public static OrderDetailDto ToOrderDetailDtoWithoutItems(this Pedido order)
    {
        ArgumentNullException.ThrowIfNull(order);

        return new OrderDetailDto
        {
            Id = order.Id,
            CustomerId = order.ClienteId,
            Status = order.Estado,
            Items = Array.Empty<OrderItemDto>(),
            ItemsCount = order.CantidadDetalles,
            TotalUnits = order.CantidadTotalUnidades,
            TotalAmount = order.Total.Amount,
            Currency = order.Total.Currency,
            CreatedAtUtc = order.FechaCreacionUtc,
            UpdatedAtUtc = order.FechaActualizacionUtc,
            ConfirmedAtUtc = order.FechaConfirmacionUtc,
            PaidAtUtc = order.FechaPagoUtc,
            ShippedAtUtc = order.FechaEnvioUtc,
            DeliveredAtUtc = order.FechaEntregaUtc,
            CancelledAtUtc = order.FechaCancelacionUtc,
            CancellationReason = order.ObservacionCancelacion,
            PaymentMethod = order.MetodoPagoSeleccionado,
            ShippingStreet = order.DireccionEnvio?.Calle,
            ShippingCity = order.DireccionEnvio?.Ciudad,
            ShippingRegion = order.DireccionEnvio?.Departamento,
            ShippingCountry = order.DireccionEnvio?.Pais,
            ShippingPostalCode = order.DireccionEnvio?.CodigoPostal,
            ContainsPhysicalProducts = order.ContieneProductosFisicos(),
            ContainsDigitalProducts = order.ContieneProductosDigitales()
        };
    }

    /// <summary>
    /// Proyecta una entidad <see cref="DetallePedido"/> hacia un <see cref="OrderItemDto"/>.
    /// </summary>
    /// <param name="item">Detalle del pedido a proyectar.</param>
    /// <returns>DTO de la línea del pedido.</returns>
    /// <exception cref="ArgumentNullException">
    /// Se produce cuando el detalle es nulo.
    /// </exception>
    public static OrderItemDto ToOrderItemDto(this DetallePedido item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return new OrderItemDto
        {
            Id = item.Id,
            OrderId = item.PedidoId,
            ProductId = item.ProductoId,
            ProductName = item.NombreProducto,
            ProductSku = item.SkuProducto.Value,
            ProductType = item.TipoProducto,
            MainImageUrl = item.ImagenPrincipalUrl,
            Quantity = item.Cantidad,
            UnitPrice = item.PrecioUnitario.Amount,
            Currency = item.PrecioUnitario.Currency,
            Subtotal = item.Subtotal.Amount,
            CreatedAtUtc = item.FechaCreacionUtc
        };
    }

    #endregion

    #region Mapeos de colecciones

    /// <summary>
    /// Proyecta una colección de entidades <see cref="Pedido"/>
    /// hacia una colección de <see cref="OrderDto"/> incluyendo sus ítems.
    /// </summary>
    /// <param name="orders">Colección de pedidos a proyectar.</param>
    /// <returns>Colección de DTOs de pedido.</returns>
    /// <exception cref="ArgumentNullException">
    /// Se produce cuando la colección es nula.
    /// </exception>
    public static IReadOnlyCollection<OrderDto> ToOrderDtos(this IEnumerable<Pedido> orders)
    {
        ArgumentNullException.ThrowIfNull(orders);

        return orders
            .Select(order => order.ToOrderDto())
            .ToArray();
    }

    /// <summary>
    /// Proyecta una colección de entidades <see cref="Pedido"/>
    /// hacia una colección de <see cref="OrderDto"/>, permitiendo decidir
    /// si deben incluirse o no los ítems del pedido.
    /// </summary>
    /// <param name="orders">Colección de pedidos a proyectar.</param>
    /// <param name="includeItems">
    /// Indica si la proyección debe incluir el detalle de ítems.
    /// </param>
    /// <returns>Colección de DTOs de pedido.</returns>
    /// <exception cref="ArgumentNullException">
    /// Se produce cuando la colección es nula.
    /// </exception>
    public static IReadOnlyCollection<OrderDto> ToOrderDtos(
        this IEnumerable<Pedido> orders,
        bool includeItems)
    {
        ArgumentNullException.ThrowIfNull(orders);

        return includeItems
            ? orders.Select(order => order.ToOrderDto()).ToArray()
            : orders.Select(order => order.ToOrderDtoWithoutItems()).ToArray();
    }

    /// <summary>
    /// Proyecta una colección de entidades <see cref="DetallePedido"/>
    /// hacia una colección de <see cref="OrderItemDto"/>.
    /// </summary>
    /// <param name="items">Colección de detalles a proyectar.</param>
    /// <returns>Colección de DTOs de líneas del pedido.</returns>
    /// <exception cref="ArgumentNullException">
    /// Se produce cuando la colección es nula.
    /// </exception>
    public static IReadOnlyCollection<OrderItemDto> ToOrderItemDtos(this IEnumerable<DetallePedido> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        return items
            .Select(item => item.ToOrderItemDto())
            .ToArray();
    }

    #endregion
}
