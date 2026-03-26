using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Cart.DTOs;

namespace PlataformaECommerce.Application.Features.Cart.Commands;

/// <summary>
/// Representa el comando de aplicación para crear un nuevo carrito de compras
/// dentro del sistema.
/// </summary>
/// <remarks>
/// Este comando modela una intención explícita de escritura sobre el sistema,
/// correspondiente al caso de uso de creación de un carrito para un cliente.
///
/// Su propósito es desacoplar esta operación respecto de otras acciones
/// del ciclo de vida del carrito, tales como:
/// - agregar productos,
/// - actualizar cantidades,
/// - vaciar el carrito,
/// - desactivar el carrito,
/// - convertir el carrito en pedido.
///
/// La lógica de validación estructural y consistencia de entrada debe resolverse
/// en validadores de Application, mientras que las reglas de negocio definitivas
/// deben reforzarse en el dominio.
///
/// El resultado esperado de la operación es un <see cref="Result{TValue}"/>
/// que contiene un <see cref="CartDto"/> cuando la ejecución es exitosa.
/// </remarks>
public sealed class CreateCartCommand
{
    #region Identificación

    /// <summary>
    /// Identificador único del cliente propietario del carrito.
    /// </summary>
    public Guid CustomerId { get; init; }

    #endregion

    #region Estado inicial

    /// <summary>
    /// Indica si el carrito debe crearse inicialmente activo.
    /// </summary>
    /// <remarks>
    /// En la mayoría de los escenarios este valor será <see langword="true"/>,
    /// pero se conserva como parte del comando para permitir flexibilidad
    /// en procesos administrativos o integraciones.
    /// </remarks>
    public bool IsActive { get; init; } = true;

    #endregion

    #region Contexto y trazabilidad

    /// <summary>
    /// Identificador del usuario que solicita o ejecuta la creación del carrito.
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
    /// Referencia externa opcional asociada al proceso de creación del carrito.
    /// </summary>
    /// <remarks>
    /// Puede representar un identificador de correlación, ticket,
    /// sesión o cualquier referencia funcional útil para observabilidad.
    /// </remarks>
    public string? ExternalReference { get; init; }

    /// <summary>
    /// Motivo funcional o comentario asociado a la creación del carrito.
    /// </summary>
    /// <remarks>
    /// Este campo puede ser útil para auditoría, soporte u observabilidad
    /// en escenarios administrativos o automatizados.
    /// </remarks>
    public string? Reason { get; init; }

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del comando de creación de carrito.
    /// </summary>
    /// <returns>Cadena representativa del comando.</returns>
    public override string ToString()
    {
        return $"CreateCartCommand | CustomerId: {CustomerId} | IsActive: {IsActive} | RequestedByUserId: {RequestedByUserId}";
    }

    #endregion
}