using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Cart.DTOs;

namespace PlataformaECommerce.Application.Features.Cart.Commands;

/// <summary>
/// Representa el comando de aplicación para vaciar completamente
/// un carrito de compras existente dentro del sistema.
/// </summary>
/// <remarks>
/// Este comando modela una intención explícita de escritura sobre el sistema,
/// correspondiente al caso de uso de eliminación total de los ítems
/// contenidos en un carrito.
///
/// Su propósito es desacoplar esta operación respecto de otras acciones
/// del ciclo de vida del carrito, tales como:
/// - creación del carrito,
/// - agregado de productos,
/// - actualización de cantidades,
/// - remoción de productos específicos,
/// - y conversión del carrito en pedido.
///
/// La lógica de validación estructural y consistencia de entrada debe resolverse
/// en validadores de Application, mientras que las reglas de negocio definitivas
/// deben reforzarse en el dominio.
///
/// El resultado esperado de la operación es un <see cref="Result{TValue}"/>
/// que contiene un <see cref="CartDto"/> cuando la ejecución es exitosa.
/// </remarks>
public sealed class ClearCartCommand
{
    #region Identificación

    /// <summary>
    /// Identificador único del carrito que será vaciado.
    /// </summary>
    public Guid CartId { get; init; }

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

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del comando de vaciado de carrito.
    /// </summary>
    /// <returns>Cadena representativa del comando.</returns>
    public override string ToString()
    {
        return $"ClearCartCommand | CartId: {CartId} | RequestedByUserId: {RequestedByUserId} | Reason: {Reason}";
    }

    #endregion
}