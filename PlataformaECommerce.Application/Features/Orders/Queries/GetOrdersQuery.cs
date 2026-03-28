using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Application.Features.Orders.Queries;

/// <summary>
/// Representa la consulta de aplicación para obtener un listado administrativo de pedidos.
/// </summary>
/// <remarks>
/// Esta query permite recuperar pedidos del sistema aplicando filtros operativos básicos,
/// ordenamiento y metadatos de trazabilidad sin restringir la consulta a un cliente específico.
/// </remarks>
public sealed class GetOrdersQuery
{
    private const string DefaultSortByValue = "createdAt";

    /// <summary>
    /// Filtra los pedidos por estado funcional.
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
    public bool? OnlyFinalized { get; init; }

    /// <summary>
    /// Indica si la consulta debe limitarse únicamente a pedidos activos.
    /// </summary>
    public bool? OnlyActive { get; init; }

    /// <summary>
    /// Campo lógico por el cual se desea ordenar la consulta.
    /// </summary>
    public string? SortBy { get; init; } = DefaultSortByValue;

    /// <summary>
    /// Indica si el ordenamiento debe ser descendente.
    /// </summary>
    public bool SortDescending { get; init; } = true;

    /// <summary>
    /// Indica si el servicio debe incluir el detalle de líneas del pedido.
    /// </summary>
    public bool IncludeItems { get; init; }

    /// <summary>
    /// Identificador opcional del usuario que solicita la consulta.
    /// </summary>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>
    /// Referencia externa opcional asociada a la consulta.
    /// </summary>
    public string? ExternalReference { get; init; }

    /// <summary>
    /// Indica si la consulta contiene al menos un filtro adicional.
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

    /// <summary>
    /// Devuelve una representación resumida de la consulta.
    /// </summary>
    public override string ToString()
    {
        return $"GetOrdersQuery | Status: {Status} | SortBy: {SortBy ?? DefaultSortByValue} | SortDescending: {SortDescending} | RequestedByUserId: {RequestedByUserId}";
    }
}