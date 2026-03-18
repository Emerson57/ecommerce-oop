using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Application.Features.Orders.DTOs;

/// <summary>
/// Representa el objeto de transferencia de datos de una línea o ítem
/// de un pedido dentro de la capa de aplicación.
/// </summary>
/// <remarks>
/// Este DTO se utiliza para transportar la información comercial e histórica
/// de cada línea del pedido desde la capa Application hacia capas superiores como:
/// - Web API,
/// - paneles administrativos,
/// - módulos de atención al cliente,
/// - procesos de seguimiento,
/// - integraciones externas,
/// - y consultas internas.
///
/// Su propósito es desacoplar la representación expuesta de la línea del pedido
/// respecto de la entidad de dominio <c>DetallePedido</c>, evitando exponer
/// directamente detalles internos del modelo.
///
/// Este DTO conserva la instantánea comercial del producto al momento
/// de la compra, permitiendo mantener consistencia histórica incluso si
/// posteriormente el producto cambia dentro del catálogo.
/// </remarks>
public sealed class OrderItemDto
{
    #region Identificación

    /// <summary>
    /// Identificador único de la línea del pedido.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Identificador único del pedido al que pertenece la línea.
    /// </summary>
    public Guid OrderId { get; init; }

    /// <summary>
    /// Identificador único del producto asociado a la línea.
    /// </summary>
    public Guid ProductId { get; init; }

    #endregion

    #region Información histórica del producto

    /// <summary>
    /// Nombre comercial del producto al momento de la compra.
    /// </summary>
    public string ProductName { get; init; } = string.Empty;

    /// <summary>
    /// SKU del producto al momento de la compra.
    /// </summary>
    public string ProductSku { get; init; } = string.Empty;

    /// <summary>
    /// Tipo funcional del producto asociado a la línea del pedido.
    /// </summary>
    public TipoProducto ProductType { get; init; }

    /// <summary>
    /// URL o ruta de la imagen principal del producto al momento de la compra.
    /// </summary>
    public string? MainImageUrl { get; init; }

    #endregion

    #region Información comercial

    /// <summary>
    /// Cantidad adquirida del producto dentro del pedido.
    /// </summary>
    public int Quantity { get; init; }

    /// <summary>
    /// Precio unitario aplicado al producto dentro del pedido.
    /// </summary>
    public decimal UnitPrice { get; init; }

    /// <summary>
    /// Código de moneda asociado al precio unitario.
    /// </summary>
    public string Currency { get; init; } = string.Empty;

    /// <summary>
    /// Subtotal consolidado de la línea del pedido.
    /// </summary>
    public decimal Subtotal { get; init; }

    #endregion

    #region Información temporal

    /// <summary>
    /// Fecha y hora UTC en que fue creada la línea del pedido.
    /// </summary>
    public DateTime CreatedAtUtc { get; init; }

    #endregion

    #region Propiedades calculadas

    /// <summary>
    /// Indica si la línea del pedido tiene una cantidad válida mayor que cero.
    /// </summary>
    public bool HasValidQuantity => Quantity > 0;

    /// <summary>
    /// Indica si la línea corresponde a un producto físico.
    /// </summary>
    public bool IsPhysicalProduct => ProductType == TipoProducto.Fisico;

    /// <summary>
    /// Indica si la línea corresponde a un producto digital.
    /// </summary>
    public bool IsDigitalProduct => ProductType == TipoProducto.Digital;

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del DTO de la línea del pedido.
    /// </summary>
    /// <returns>Cadena representativa de la línea del pedido.</returns>
    public override string ToString()
    {
        return $"OrderItemDto | Id: {Id} | OrderId: {OrderId} | ProductId: {ProductId} | ProductName: {ProductName} | ProductSku: {ProductSku} | Quantity: {Quantity} | UnitPrice: {Currency} {UnitPrice:N2} | Subtotal: {Currency} {Subtotal:N2}";
    }

    #endregion
}