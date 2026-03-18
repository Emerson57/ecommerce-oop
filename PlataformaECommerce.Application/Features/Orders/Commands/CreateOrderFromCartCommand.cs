using PlataformaECommerce.Application.Abstractions;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Orders.DTOs;

namespace PlataformaECommerce.Application.Features.Orders.Commands;

/// <summary>
/// Representa el comando de aplicación para crear un pedido
/// a partir de un carrito de compras existente dentro del sistema.
/// </summary>
/// <remarks>
/// Este comando modela una intención explícita de escritura sobre el sistema,
/// correspondiente al caso de uso de conversión de carrito a pedido.
///
/// Su responsabilidad es transportar los datos necesarios desde la capa superior
/// hacia el handler correspondiente, sin contener lógica de negocio ni reglas
/// de validación complejas, las cuales deben resolverse en:
/// - validadores de Application,
/// - handlers,
/// - servicios transversales,
/// - y entidades del dominio.
///
/// Este comando está orientado al proceso de checkout inicial y permite
/// encapsular tanto la identificación del carrito como el contexto operativo
/// desde el cual se solicita la creación del pedido.
///
/// El resultado esperado de la operación es un <see cref="Result{TValue}"/>
/// que contiene un <see cref="OrderDetailDto"/> cuando la ejecución es exitosa,
/// permitiendo devolver a la capa superior una representación consolidada
/// y detallada del pedido recién generado.
/// </remarks>
public sealed class CreateOrderFromCartCommand : ICommand<Result<OrderDetailDto>>
{
    #region Identificación principal

    /// <summary>
    /// Identificador único del carrito de compras que servirá
    /// como origen para la creación del pedido.
    /// </summary>
    public Guid CartId { get; init; }

    /// <summary>
    /// Identificador único del cliente propietario del carrito
    /// y del pedido que será generado.
    /// </summary>
    /// <remarks>
    /// Aunque el carrito ya está asociado a un cliente en el dominio,
    /// este dato se conserva en el comando para reforzar consistencia,
    /// trazabilidad y validaciones cruzadas en Application.
    /// </remarks>
    public Guid CustomerId { get; init; }

    #endregion

    #region Información funcional del proceso

    /// <summary>
    /// Observación funcional o comentario asociado al proceso
    /// de creación del pedido.
    /// </summary>
    /// <remarks>
    /// Este campo puede utilizarse para auditoría, soporte,
    /// trazabilidad operacional o registro de contexto adicional
    /// durante el checkout.
    /// </remarks>
    public string? Notes { get; init; }

    /// <summary>
    /// Referencia funcional externa asociada al proceso de creación del pedido.
    /// </summary>
    /// <remarks>
    /// Puede representar un identificador de correlación, número de sesión,
    /// ticket de soporte, referencia de integración o cualquier otro dato
    /// útil para observabilidad entre sistemas.
    /// </remarks>
    public string? ExternalReference { get; init; }

    #endregion

    #region Contexto y trazabilidad

    /// <summary>
    /// Identificador del usuario que solicita o ejecuta la creación del pedido,
    /// cuando dicho dato esté disponible.
    /// </summary>
    /// <remarks>
    /// Este valor puede utilizarse para trazabilidad, auditoría
    /// y control de seguridad en escenarios administrativos,
    /// automatizados o de autoservicio.
    /// </remarks>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>
    /// Dirección IP desde la cual se originó la solicitud,
    /// cuando esté disponible.
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// Canal de origen desde el cual se genera la solicitud.
    /// </summary>
    /// <remarks>
    /// Ejemplos comunes:
    /// - Web
    /// - Mobile
    /// - AdminPortal
    /// - ApiClient
    /// - CustomerService
    /// </remarks>
    public string? Source { get; init; }

    /// <summary>
    /// Fecha y hora UTC en la que la capa superior registró
    /// la solicitud de creación del pedido.
    /// </summary>
    /// <remarks>
    /// Este dato es útil para escenarios de observabilidad,
    /// correlación de eventos y trazabilidad distribuida.
    /// Cuando no se informe, el handler puede utilizar su propia
    /// fuente de tiempo controlada.
    /// </remarks>
    public DateTime? RequestedAtUtc { get; init; }

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del comando
    /// de creación de pedido a partir de carrito.
    /// </summary>
    /// <returns>Cadena representativa del comando.</returns>
    public override string ToString()
    {
        return $"CreateOrderFromCartCommand | CartId: {CartId} | CustomerId: {CustomerId} | RequestedByUserId: {RequestedByUserId} | Source: {Source}";
    }

    #endregion
}