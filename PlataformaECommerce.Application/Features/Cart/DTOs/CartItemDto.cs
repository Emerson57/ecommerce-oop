using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Application.Features.Cart.DTOs;

/// <summary>
/// Representa el objeto de transferencia de datos de una línea o ítem
/// de carrito dentro de la capa de aplicación.
/// </summary>
/// <remarks>
/// Este DTO se utiliza para transportar la información resumida y comercial
/// de un ítem del carrito desde la capa Application hacia capas superiores como:
/// - Web API,
/// - frontends de e-commerce,
/// - procesos de checkout,
/// - paneles administrativos,
/// - consultas internas.
///
/// Su propósito es desacoplar la representación expuesta del ítem del carrito
/// respecto de la entidad de dominio <c>ItemCarrito</c>, evitando filtrar
/// directamente detalles internos del modelo.
///
/// Este DTO conserva la información relevante de la línea comercial:
/// - producto asociado,
/// - nombre,
/// - SKU,
/// - tipo,
/// - imagen,
/// - cantidad,
/// - precio unitario,
/// - subtotal,
/// - y metadatos temporales.
/// </remarks>
public sealed class CartItemDto
{
    #region Identificación

    /// <summary>
    /// Identificador único del ítem dentro del carrito.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Identificador único del producto asociado a la línea del carrito.
    /// </summary>
    public Guid ProductId { get; init; }

    #endregion

    #region Información del producto

    /// <summary>
    /// Nombre comercial del producto asociado al ítem.
    /// </summary>
    public string ProductName { get; init; } = string.Empty;

    /// <summary>
    /// SKU del producto asociado al ítem.
    /// </summary>
    public string ProductSku { get; init; } = string.Empty;

    /// <summary>
    /// Tipo funcional del producto asociado a la línea del carrito.
    /// </summary>
    public TipoProducto ProductType { get; init; }

    /// <summary>
    /// URL o ruta de la imagen principal del producto.
    /// </summary>
    public string? MainImageUrl { get; init; }

    #endregion

    #region Información comercial

    /// <summary>
    /// Cantidad seleccionada del producto dentro del carrito.
    /// </summary>
    public int Quantity { get; init; }

    /// <summary>
    /// Precio unitario del producto al momento de la proyección del ítem.
    /// </summary>
    public decimal UnitPrice { get; init; }

    /// <summary>
    /// Código de moneda asociado al precio unitario.
    /// </summary>
    public string Currency { get; init; } = string.Empty;

    /// <summary>
    /// Subtotal consolidado del ítem del carrito.
    /// </summary>
    public decimal Subtotal { get; init; }

    #endregion

    #region Información temporal

    /// <summary>
    /// Fecha y hora UTC en que fue creado el ítem del carrito.
    /// </summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>
    /// Fecha y hora UTC de la última actualización relevante del ítem del carrito.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; init; }

    #endregion

    #region Propiedades calculadas

    /// <summary>
    /// Indica si el ítem tiene una cantidad válida mayor que cero.
    /// </summary>
    public bool HasValidQuantity => Quantity > 0;

    /// <summary>
    /// Indica si el ítem corresponde a un producto físico.
    /// </summary>
    public bool IsPhysicalProduct => ProductType == TipoProducto.Fisico;

    /// <summary>
    /// Indica si el ítem corresponde a un producto digital.
    /// </summary>
    public bool IsDigitalProduct => ProductType == TipoProducto.Digital;

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del DTO del ítem del carrito.
    /// </summary>
    /// <returns>Cadena representativa del ítem del carrito.</returns>
    public override string ToString()
    {
        return $"CartItemDto | Id: {Id} | ProductId: {ProductId} | ProductName: {ProductName} | ProductSku: {ProductSku} | Quantity: {Quantity} | UnitPrice: {Currency} {UnitPrice:N2} | Subtotal: {Currency} {Subtotal:N2}";
    }

    #endregion
}