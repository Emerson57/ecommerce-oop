using PlataformaECommerce.Application.Abstractions;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Orders.DTOs;

namespace PlataformaECommerce.Application.Features.Orders.Queries;

/// <summary>
/// Representa la consulta de aplicación para obtener el detalle completo
/// de un pedido a partir de su identificador único.
/// </summary>
/// <remarks>
/// Esta query modela una intención explícita de lectura dentro del sistema,
/// correspondiente al caso de uso de consultar la información detallada
/// de un pedido específico.
///
/// Su responsabilidad es transportar los datos mínimos necesarios para que
/// el handler correspondiente recupere la información desde la fuente de datos,
/// la proyecte adecuadamente y retorne una respuesta desacoplada del dominio.
///
/// El resultado esperado de la operación es un <see cref="Result{TValue}"/>
/// que contiene un <see cref="OrderDetailDto"/> cuando la ejecución es exitosa.
///
/// Esta consulta no debe contener lógica de negocio ni acceso a infraestructura;
/// dichas responsabilidades pertenecen al handler y a los componentes
/// especializados de la capa Application e Infrastructure.
///
/// Además, esta query está diseñada para soportar escenarios como:
/// - consulta desde panel administrativo,
/// - seguimiento del pedido por parte del cliente,
/// - validación de estado en procesos internos,
/// - integración con módulos logísticos,
/// - y trazabilidad funcional entre capas.
/// </remarks>
public sealed class GetOrderByIdQuery : IQuery<Result<OrderDetailDto>>
{
    #region Constructores

    /// <summary>
    /// Inicializa una nueva instancia vacía de la consulta.
    /// </summary>
    public GetOrderByIdQuery()
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la consulta con el identificador del pedido.
    /// </summary>
    /// <param name="orderId">Identificador único del pedido a consultar.</param>
    public GetOrderByIdQuery(Guid orderId)
    {
        OrderId = orderId;
    }

    #endregion

    #region Propiedades públicas

    /// <summary>
    /// Identificador único del pedido que será consultado.
    /// </summary>
    public Guid OrderId { get; init; }

    /// <summary>
    /// Indica si la consulta debe incluir información extendida o complementaria
    /// cuando la implementación del handler así lo soporte.
    /// </summary>
    /// <remarks>
    /// Esta propiedad permite evolucionar la consulta sin romper su contrato,
    /// por ejemplo para controlar inclusión de auditoría, historial operativo,
    /// datos logísticos, referencias externas o metadatos adicionales.
    /// </remarks>
    public bool IncludeExtendedData { get; init; } = true;

    /// <summary>
    /// Indica si la consulta debe incluir el detalle de líneas o ítems del pedido.
    /// </summary>
    /// <remarks>
    /// Esta propiedad permite optimizar escenarios donde únicamente se requiere
    /// validar la cabecera del pedido, su estado o su trazabilidad general,
    /// evitando cargar el detalle completo cuando no sea necesario.
    /// </remarks>
    public bool IncludeItems { get; init; } = true;

    /// <summary>
    /// Identificador opcional del cliente al que se espera que pertenezca el pedido.
    /// </summary>
    /// <remarks>
    /// Este valor puede utilizarse por el handler para reforzar controles
    /// de seguridad, validaciones de pertenencia o restricciones de acceso
    /// cuando la consulta se origine desde canales de autoservicio.
    /// </remarks>
    public Guid? ExpectedCustomerId { get; init; }

    /// <summary>
    /// Identificador opcional del usuario que solicita la consulta.
    /// </summary>
    /// <remarks>
    /// Puede utilizarse para trazabilidad, auditoría o adaptación contextual
    /// de la respuesta cuando la capa superior decida enviarlo explícitamente.
    /// </remarks>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>
    /// Referencia externa opcional asociada a la consulta.
    /// </summary>
    /// <remarks>
    /// Puede representar un identificador de correlación, un ticket
    /// o una referencia funcional útil para observabilidad.
    /// </remarks>
    public string? ExternalReference { get; init; }

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida de la consulta.
    /// </summary>
    /// <returns>Cadena representativa de la query.</returns>
    public override string ToString()
    {
        return $"GetOrderByIdQuery | OrderId: {OrderId} | IncludeExtendedData: {IncludeExtendedData} | IncludeItems: {IncludeItems} | ExpectedCustomerId: {ExpectedCustomerId} | RequestedByUserId: {RequestedByUserId}";
    }

    #endregion
}