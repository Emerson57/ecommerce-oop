using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Cart.DTOs;

namespace PlataformaECommerce.Application.Features.Cart.Commands;

/// <summary>
/// Representa el comando de aplicación para remover un producto
/// de un carrito de compras existente dentro del sistema.
/// </summary>
/// <remarks>
/// Este comando modela una intención explícita de escritura sobre el sistema,
/// correspondiente al caso de uso de eliminación de una línea de producto
/// previamente registrada dentro de un carrito.
///
/// Su propósito es desacoplar esta operación respecto de otras acciones
/// del ciclo de vida del carrito, tales como:
/// - creación del carrito,
/// - agregado de productos,
/// - actualización de cantidades,
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
public sealed class RemoveProductFromCartCommand
{
    #region Identificación

    /// <summary>
    /// Identificador único del carrito del cual se removerá el producto.
    /// </summary>
    public Guid CartId { get; init; }

    /// <summary>
    /// Identificador único del producto que será removido del carrito.
    /// </summary>
    /// <remarks>
    /// Este identificador representa el producto asociado a la línea comercial
    /// que debe ser eliminada del carrito.
    /// </remarks>
    public Guid ProductId { get; init; }

    /// <summary>
    /// Identificador opcional del ítem específico del carrito que será removido.
    /// </summary>
    /// <remarks>
    /// Esta propiedad permite trabajar con escenarios donde la capa superior
    /// conoce el identificador exacto del ítem del carrito.
    /// Si no se informa, el caso de uso puede operar únicamente con
    /// <see cref="ProductId"/> según la estrategia adoptada.
    /// </remarks>
    public Guid? CartItemId { get; init; }

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
    /// Indica si la operación incluye un identificador explícito de ítem de carrito.
    /// </summary>
    public bool HasCartItemId => CartItemId.HasValue && CartItemId.Value != Guid.Empty;

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del comando de remoción
    /// de producto del carrito.
    /// </summary>
    /// <returns>Cadena representativa del comando.</returns>
    public override string ToString()
    {
        return $"RemoveProductFromCartCommand | CartId: {CartId} | ProductId: {ProductId} | CartItemId: {CartItemId} | RequestedByUserId: {RequestedByUserId}";
    }

    #endregion
}