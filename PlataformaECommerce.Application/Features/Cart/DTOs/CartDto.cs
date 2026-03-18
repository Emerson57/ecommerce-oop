using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Application.Features.Cart.DTOs;

/// <summary>
/// Representa el objeto de transferencia de datos de un carrito de compras
/// dentro de la capa de aplicación.
/// </summary>
/// <remarks>
/// Este DTO se utiliza para transportar la información consolidada de un carrito
/// desde la capa Application hacia capas superiores como:
/// - Web API,
/// - frontends de e-commerce,
/// - paneles administrativos,
/// - procesos de checkout,
/// - consultas internas.
///
/// Su propósito es desacoplar la representación expuesta del carrito
/// respecto de la entidad de dominio <c>CarritoCompra</c>, evitando filtrar
/// directamente detalles internos del modelo.
///
/// Este DTO contiene:
/// - información básica del carrito,
/// - identificación del cliente propietario,
/// - estado operativo,
/// - resumen económico,
/// - y el detalle de sus ítems.
/// </remarks>
public sealed class CartDto
{
    #region Identificación básica

    /// <summary>
    /// Identificador único del carrito.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Identificador del cliente propietario del carrito.
    /// </summary>
    public Guid CustomerId { get; init; }

    #endregion

    #region Estado operativo

    /// <summary>
    /// Indica si el carrito se encuentra activo.
    /// </summary>
    public bool IsActive { get; init; }

    #endregion

    #region Información de contenido

    /// <summary>
    /// Colección de ítems contenidos en el carrito.
    /// </summary>
    public IReadOnlyCollection<CartItemDto> Items { get; init; } = Array.Empty<CartItemDto>();

    /// <summary>
    /// Cantidad total de líneas distintas registradas en el carrito.
    /// </summary>
    public int ItemsCount { get; init; }

    /// <summary>
    /// Cantidad total de unidades acumuladas entre todas las líneas del carrito.
    /// </summary>
    public int TotalUnits { get; init; }

    /// <summary>
    /// Total monetario consolidado del carrito.
    /// </summary>
    public decimal TotalAmount { get; init; }

    /// <summary>
    /// Código de moneda asociado al total del carrito.
    /// </summary>
    public string Currency { get; init; } = string.Empty;

    #endregion

    #region Información temporal

    /// <summary>
    /// Fecha y hora UTC en que fue creado el carrito.
    /// </summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>
    /// Fecha y hora UTC de la última actualización relevante del carrito.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; init; }

    #endregion

    #region Propiedades calculadas

    /// <summary>
    /// Indica si el carrito contiene al menos un ítem.
    /// </summary>
    public bool HasItems => ItemsCount > 0;

    /// <summary>
    /// Indica si el carrito se encuentra listo para iniciar un flujo de checkout.
    /// </summary>
    public bool IsReadyForCheckout => IsActive && HasItems;

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del DTO del carrito.
    /// </summary>
    /// <returns>Cadena representativa del carrito.</returns>
    public override string ToString()
    {
        return $"CartDto | Id: {Id} | CustomerId: {CustomerId} | ItemsCount: {ItemsCount} | TotalUnits: {TotalUnits} | TotalAmount: {Currency} {TotalAmount:N2} | IsActive: {IsActive}";
    }

    #endregion
}