using PlataformaECommerce.Application.Abstractions;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Cart.DTOs;

namespace PlataformaECommerce.Application.Features.Cart.Commands;

/// <summary>
/// Representa el comando de aplicación para agregar un producto
/// a un carrito de compras existente dentro del sistema.
/// </summary>
/// <remarks>
/// Este comando modela una intención explícita de escritura sobre el sistema,
/// correspondiente al caso de uso de incorporación de un producto
/// al carrito de compras de un cliente.
///
/// Su propósito es desacoplar esta operación respecto de otras acciones
/// del ciclo de vida del carrito, tales como:
/// - creación del carrito,
/// - actualización de cantidades,
/// - eliminación de productos,
/// - vaciado del carrito,
/// - y conversión del carrito en pedido.
///
/// La lógica de validación estructural y consistencia de entrada debe resolverse
/// en validadores de Application, mientras que las reglas de negocio definitivas
/// deben reforzarse en el dominio.
///
/// El resultado esperado de la operación es un <see cref="Result{TValue}"/>
/// que contiene un <see cref="CartDto"/> cuando la ejecución es exitosa.
/// </remarks>
public sealed class AddProductToCartCommand : ICommand<Result<CartDto>>
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
    /// <remarks>
    /// Este valor representa la cantidad solicitada para incorporación.
    /// Si el producto ya existe en el carrito, el caso de uso podrá
    /// interpretar esta cantidad como un incremento sobre la línea existente,
    /// según las reglas definidas en la capa de dominio y aplicación.
    /// </remarks>
    public int Quantity { get; init; }

    #endregion

    #region Contexto y trazabilidad

    /// <summary>
    /// Identificador del usuario que solicita o ejecuta la operación.
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

    /// <summary>
    /// Motivo funcional o comentario asociado a la operación.
    /// </summary>
    /// <remarks>
    /// Este campo puede ser útil para auditoría, soporte u observabilidad
    /// en escenarios administrativos o automatizados.
    /// </remarks>
    public string? Reason { get; init; }

    #endregion

    #region Propiedades calculadas

    /// <summary>
    /// Indica si la cantidad solicitada es estructuralmente válida.
    /// </summary>
    public bool HasValidQuantity => Quantity > 0;

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del comando de agregado al carrito.
    /// </summary>
    /// <returns>Cadena representativa del comando.</returns>
    public override string ToString()
    {
        return $"AddProductToCartCommand | CartId: {CartId} | ProductId: {ProductId} | Quantity: {Quantity} | RequestedByUserId: {RequestedByUserId}";
    }

    #endregion
}