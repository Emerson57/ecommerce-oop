namespace PlataformaECommerce.Application.Features.Cart.DTOs;

/// <summary>
/// Representa la solicitud de adición de un producto al carrito de compras
/// dentro de la capa de aplicación.
/// </summary>
/// <remarks>
/// Este DTO se utiliza para transportar la información necesaria para agregar
/// un producto a un carrito existente, desacoplando la entrada externa
/// respecto de las entidades del dominio.
///
/// Su propósito es servir como contrato de entrada para:
/// - endpoints HTTP,
/// - comandos de aplicación,
/// - servicios de aplicación,
/// - flujos de carrito,
/// - procesos de compra.
///
/// La estructura contiene únicamente datos de transporte y no debe incluir
/// lógica de negocio ni reglas de validación complejas, las cuales deben
/// resolverse en la capa Application mediante validadores especializados
/// y, posteriormente, reforzarse en el dominio.
/// </remarks>
public sealed class AddCartItemRequestDto
{
    #region Identificación

    /// <summary>
    /// Identificador único del carrito al que se agregará el producto.
    /// </summary>
    public Guid CartId { get; init; }

    /// <summary>
    /// Identificador único del producto que será agregado al carrito.
    /// </summary>
    public Guid ProductId { get; init; }

    #endregion

    #region Información de la operación

    /// <summary>
    /// Cantidad del producto que se desea agregar al carrito.
    /// </summary>
    public int Quantity { get; init; }

    #endregion

    #region Contexto y trazabilidad

    /// <summary>
    /// Identificador del usuario que solicita la operación, cuando esté disponible.
    /// </summary>
    /// <remarks>
    /// Este valor puede utilizarse para trazabilidad, auditoría
    /// o control de seguridad cuando la capa superior desee enviarlo explícitamente.
    /// </remarks>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>
    /// Dirección IP desde la cual se originó la solicitud, cuando esté disponible.
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// Canal de origen de la solicitud, cuando la capa superior desee informarlo.
    /// </summary>
    /// <remarks>
    /// Ejemplos comunes:
    /// - Web
    /// - Mobile
    /// - AdminPortal
    /// - ApiClient
    /// </remarks>
    public string? Source { get; init; }

    /// <summary>
    /// Referencia externa opcional asociada a la operación.
    /// </summary>
    /// <remarks>
    /// Puede representar un identificador de correlación, ticket,
    /// sesión o cualquier referencia funcional útil para observabilidad.
    /// </remarks>
    public string? ExternalReference { get; init; }

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida de la solicitud de agregado al carrito.
    /// </summary>
    /// <returns>Cadena representativa de la solicitud.</returns>
    public override string ToString()
    {
        return $"AddCartItemRequestDto | CartId: {CartId} | ProductId: {ProductId} | Quantity: {Quantity} | RequestedByUserId: {RequestedByUserId}";
    }

    #endregion
}