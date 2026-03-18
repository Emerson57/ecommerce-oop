using PlataformaECommerce.Application.Abstractions;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Cart.DTOs;

namespace PlataformaECommerce.Application.Features.Cart.Commands;

/// <summary>
/// Representa el comando de aplicación para actualizar la cantidad
/// de un ítem existente dentro de un carrito de compras.
/// </summary>
/// <remarks>
/// Este comando modela una intención explícita de escritura sobre el sistema,
/// correspondiente al caso de uso de modificación de la cantidad de una línea
/// de carrito previamente registrada.
///
/// Su propósito es desacoplar esta operación respecto de otras acciones
/// del ciclo de vida del carrito, tales como:
/// - creación del carrito,
/// - agregado de productos,
/// - eliminación de ítems,
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
public sealed class UpdateCartItemQuantityCommand : ICommand<Result<CartDto>>
{
    #region Identificación

    /// <summary>
    /// Identificador único del carrito de compras.
    /// </summary>
    public Guid CartId { get; init; }

    /// <summary>
    /// Identificador único del ítem del carrito cuya cantidad será actualizada.
    /// </summary>
    public Guid CartItemId { get; init; }

    /// <summary>
    /// Identificador del producto asociado al ítem del carrito.
    /// </summary>
    /// <remarks>
    /// Este valor puede utilizarse para verificaciones adicionales de integridad
    /// durante el procesamiento del caso de uso.
    /// </remarks>
    public Guid ProductId { get; init; }

    #endregion

    #region Información de actualización

    /// <summary>
    /// Nueva cantidad final deseada para el ítem dentro del carrito.
    /// </summary>
    /// <remarks>
    /// Este valor representa la cantidad absoluta final y no un incremento
    /// o decremento relativo.
    ///
    /// En muchos escenarios de e-commerce:
    /// - valores mayores que cero actualizan la línea,
    /// - el valor cero implica eliminar el ítem del carrito.
    /// </remarks>
    public int NewQuantity { get; init; }

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
    /// Indica si la cantidad solicitada es válida desde una perspectiva estructural.
    /// </summary>
    public bool HasValidQuantity => NewQuantity >= 0;

    /// <summary>
    /// Indica si la operación solicita eliminar el ítem del carrito.
    /// </summary>
    public bool IsRemovalRequest => NewQuantity == 0;

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del comando de actualización
    /// de cantidad de ítem de carrito.
    /// </summary>
    /// <returns>Cadena representativa del comando.</returns>
    public override string ToString()
    {
        return $"UpdateCartItemQuantityCommand | CartId: {CartId} | CartItemId: {CartItemId} | ProductId: {ProductId} | NewQuantity: {NewQuantity} | RequestedByUserId: {RequestedByUserId}";
    }

    #endregion
}