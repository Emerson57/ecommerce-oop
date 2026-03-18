using PlataformaECommerce.Application.Abstractions;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Orders.DTOs;
using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Application.Features.Orders.Queries;

/// <summary>
/// Representa la consulta de aplicación para obtener un listado de pedidos
/// asociados a un cliente específico, permitiendo aplicar criterios
/// de filtrado, ordenamiento y paginación.
/// </summary>
/// <remarks>
/// Esta query modela una intención explícita de lectura dentro del sistema,
/// correspondiente al caso de uso de consultar el historial de pedidos
/// de un cliente o recuperar pedidos relacionados con un usuario determinado.
///
/// Su responsabilidad es transportar los criterios necesarios para que
/// el handler correspondiente recupere, filtre y proyecte la información
/// hacia una colección desacoplada del dominio.
///
/// El resultado esperado de la operación es un <see cref="Result{TValue}"/>
/// que contiene una colección de <see cref="OrderDto"/> cuando la ejecución es exitosa.
///
/// Esta consulta no debe contener lógica de negocio ni acceso a infraestructura;
/// dichas responsabilidades pertenecen al handler y a los componentes
/// especializados de la capa Application e Infrastructure.
///
/// Esta query está preparada para soportar escenarios como:
/// - historial de compras del cliente,
/// - consulta administrativa de pedidos por cliente,
/// - filtros por estado,
/// - segmentación temporal,
/// - paginación del historial,
/// - y trazabilidad contextual de lectura.
/// </remarks>
public sealed class GetOrdersByCustomerIdQuery : IQuery<Result<IReadOnlyCollection<OrderDto>>>
{
    #region Constantes

    /// <summary>
    /// Tamaño de página por defecto para la consulta de pedidos por cliente.
    /// </summary>
    private const int DefaultPageSize = 20;

    /// <summary>
    /// Número de página por defecto para la consulta de pedidos por cliente.
    /// </summary>
    private const int DefaultPageNumber = 1;

    /// <summary>
    /// Tamaño máximo de página permitido para la consulta.
    /// </summary>
    private const int MaxPageSize = 200;

    #endregion

    #region Constructores

    /// <summary>
    /// Inicializa una nueva instancia vacía de la consulta.
    /// </summary>
    public GetOrdersByCustomerIdQuery()
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la consulta con el identificador del cliente.
    /// </summary>
    /// <param name="customerId">Identificador único del cliente cuyos pedidos se desean consultar.</param>
    public GetOrdersByCustomerIdQuery(Guid customerId)
    {
        CustomerId = customerId;
    }

    #endregion

    #region Filtro principal

    /// <summary>
    /// Identificador único del cliente cuyos pedidos serán consultados.
    /// </summary>
    public Guid CustomerId { get; init; }

    #endregion

    #region Filtros de búsqueda

    /// <summary>
    /// Filtra los pedidos por estado funcional dentro del ciclo de vida.
    /// </summary>
    public EstadoPedido? Status { get; init; }

    /// <summary>
    /// Filtra los pedidos creados desde una fecha UTC determinada.
    /// </summary>
    public DateTime? CreatedFromUtc { get; init; }

    /// <summary>
    /// Filtra los pedidos creados hasta una fecha UTC determinada.
    /// </summary>
    public DateTime? CreatedToUtc { get; init; }

    /// <summary>
    /// Filtra pedidos cuyo total sea mayor o igual al valor indicado.
    /// </summary>
    public decimal? MinTotalAmount { get; init; }

    /// <summary>
    /// Filtra pedidos cuyo total sea menor o igual al valor indicado.
    /// </summary>
    public decimal? MaxTotalAmount { get; init; }

    /// <summary>
    /// Filtra pedidos por código de moneda.
    /// </summary>
    public string? Currency { get; init; }

    /// <summary>
    /// Indica si la consulta debe limitarse únicamente a pedidos finalizados.
    /// </summary>
    /// <remarks>
    /// Se consideran finalizados, de forma general, los pedidos entregados
    /// o cancelados, conforme a la semántica del dominio.
    /// </remarks>
    public bool? OnlyFinalized { get; init; }

    /// <summary>
    /// Indica si la consulta debe limitarse únicamente a pedidos activos.
    /// </summary>
    /// <remarks>
    /// Un pedido activo corresponde, en términos generales, a un pedido
    /// que aún no ha concluido satisfactoriamente ni ha sido cancelado.
    /// </remarks>
    public bool? OnlyActive { get; init; }

    #endregion

    #region Paginación

    /// <summary>
    /// Número de página solicitado.
    /// </summary>
    /// <remarks>
    /// Si no se especifica, la consulta utiliza el valor por defecto.
    /// </remarks>
    public int PageNumber { get; init; } = DefaultPageNumber;

    /// <summary>
    /// Tamaño de página solicitado.
    /// </summary>
    /// <remarks>
    /// Si no se especifica, la consulta utiliza el valor por defecto.
    /// </remarks>
    public int PageSize { get; init; } = DefaultPageSize;

    #endregion

    #region Ordenamiento

    /// <summary>
    /// Campo lógico por el cual se desea ordenar la consulta.
    /// </summary>
    /// <remarks>
    /// Ejemplos comunes:
    /// - createdAt
    /// - totalAmount
    /// - status
    /// - updatedAt
    /// </remarks>
    public string? SortBy { get; init; } = "createdAt";

    /// <summary>
    /// Indica si el ordenamiento debe ser descendente.
    /// </summary>
    public bool SortDescending { get; init; } = true;

    #endregion

    #region Contexto opcional

    /// <summary>
    /// Indica si el handler debe incluir información extendida cuando la implementación lo soporte.
    /// </summary>
    /// <remarks>
    /// Esta propiedad permite evolucionar la consulta sin romper su contrato,
    /// por ejemplo para controlar inclusión de trazabilidad adicional,
    /// información logística o enriquecimiento contextual.
    /// </remarks>
    public bool IncludeExtendedData { get; init; }

    /// <summary>
    /// Indica si el handler debe incluir el detalle de líneas o ítems del pedido
    /// dentro de la proyección, cuando la implementación lo soporte.
    /// </summary>
    public bool IncludeItems { get; init; }

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

    #region Propiedades calculadas

    /// <summary>
    /// Obtiene el número de página normalizado.
    /// </summary>
    public int NormalizedPageNumber => PageNumber < 1 ? DefaultPageNumber : PageNumber;

    /// <summary>
    /// Obtiene el tamaño de página normalizado.
    /// </summary>
    public int NormalizedPageSize
    {
        get
        {
            if (PageSize < 1)
            {
                return DefaultPageSize;
            }

            return PageSize > MaxPageSize
                ? MaxPageSize
                : PageSize;
        }
    }

    /// <summary>
    /// Obtiene el desplazamiento calculado para paginación.
    /// </summary>
    public int Offset => (NormalizedPageNumber - 1) * NormalizedPageSize;

    /// <summary>
    /// Indica si la consulta contiene al menos un filtro adicional
    /// distinto al identificador del cliente.
    /// </summary>
    public bool HasAdditionalFilters =>
        Status.HasValue ||
        CreatedFromUtc.HasValue ||
        CreatedToUtc.HasValue ||
        MinTotalAmount.HasValue ||
        MaxTotalAmount.HasValue ||
        !string.IsNullOrWhiteSpace(Currency) ||
        OnlyFinalized.HasValue ||
        OnlyActive.HasValue;

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida de la consulta.
    /// </summary>
    /// <returns>Cadena representativa de la query.</returns>
    public override string ToString()
    {
        return $"GetOrdersByCustomerIdQuery | CustomerId: {CustomerId} | Status: {Status} | PageNumber: {NormalizedPageNumber} | PageSize: {NormalizedPageSize} | SortBy: {SortBy} | SortDescending: {SortDescending} | RequestedByUserId: {RequestedByUserId}";
    }

    #endregion
}