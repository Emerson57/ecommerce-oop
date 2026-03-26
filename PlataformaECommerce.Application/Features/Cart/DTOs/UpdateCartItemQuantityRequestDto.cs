namespace PlataformaECommerce.Application.Features.Cart.DTOs;

/// <summary>
/// Representa la solicitud para actualizar la cantidad de un ítem
/// dentro de un carrito de compras.
/// </summary>
/// <remarks>
/// Este DTO se utiliza como contrato de entrada para operaciones
/// que modifican la cantidad de un producto ya existente dentro
/// de un carrito.
///
/// Su propósito es transportar datos desde capas externas
/// (API, UI, clientes móviles o integraciones) hacia la capa
/// Application sin exponer directamente las entidades del dominio.
///
/// La lógica de negocio relacionada con la actualización
/// de cantidades (por ejemplo validación de stock, límites de
/// compra o reglas comerciales) debe implementarse en:
///
/// - Validadores de Application
/// - Servicios de aplicación
/// - Reglas del dominio
///
/// Este DTO solamente representa datos de transporte.
/// </remarks>
public sealed class UpdateCartItemQuantityRequestDto
{
    #region Identificación

    /// <summary>
    /// Identificador único del carrito de compras.
    /// </summary>
    public Guid CartId { get; init; }

    /// <summary>
    /// Identificador único del ítem del carrito cuya cantidad será modificada.
    /// </summary>
    public Guid CartItemId { get; init; }

    /// <summary>
    /// Identificador del producto asociado al ítem del carrito.
    /// </summary>
    /// <remarks>
    /// Este valor puede utilizarse para validaciones adicionales
    /// o verificación de integridad durante el procesamiento
    /// de la solicitud.
    /// </remarks>
    public Guid ProductId { get; init; }

    #endregion

    #region Información de actualización

    /// <summary>
    /// Nueva cantidad del producto dentro del carrito.
    /// </summary>
    /// <remarks>
    /// Este valor representa la cantidad final deseada.
    ///
    /// Ejemplo:
    /// Si el carrito tenía 2 unidades y se envía 5,
    /// la línea deberá actualizarse a 5 unidades.
    /// </remarks>
    public int NewQuantity { get; init; }

    #endregion

    #region Contexto de operación

    /// <summary>
    /// Identificador del usuario que ejecuta la operación.
    /// </summary>
    /// <remarks>
    /// Este valor puede ser utilizado para:
    /// - auditoría
    /// - trazabilidad
    /// - seguridad
    /// - control de sesión
    /// </remarks>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>
    /// Dirección IP desde la cual se originó la solicitud.
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// Canal desde el cual se originó la solicitud.
    /// </summary>
    /// <remarks>
    /// Ejemplos posibles:
    /// - Web
    /// - Mobile
    /// - AdminPortal
    /// - ApiClient
    /// </remarks>
    public string? Source { get; init; }

    /// <summary>
    /// Identificador externo opcional de la operación.
    /// </summary>
    /// <remarks>
    /// Puede representar:
    /// - un identificador de correlación
    /// - un ID de sesión
    /// - un identificador de tracking
    /// - un ticket de operación
    /// </remarks>
    public string? ExternalReference { get; init; }

    #endregion

    #region Propiedades derivadas

    /// <summary>
    /// Indica si la operación solicita eliminar el ítem del carrito.
    /// </summary>
    /// <remarks>
    /// En muchos sistemas de e-commerce,
    /// establecer la cantidad en cero implica eliminar la línea.
    /// </remarks>
    public bool IsRemovalRequest => NewQuantity == 0;

    /// <summary>
    /// Indica si la cantidad solicitada es válida
    /// desde una perspectiva estructural.
    /// </summary>
    public bool HasValidQuantity => NewQuantity >= 0;

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida de la solicitud.
    /// </summary>
    /// <returns>Cadena representativa de la solicitud.</returns>
    public override string ToString()
    {
        return $"UpdateCartItemQuantityRequestDto | CartId: {CartId} | CartItemId: {CartItemId} | ProductId: {ProductId} | NewQuantity: {NewQuantity} | RequestedByUserId: {RequestedByUserId}";
    }

    #endregion
}