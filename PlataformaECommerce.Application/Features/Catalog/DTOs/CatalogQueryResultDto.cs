namespace PlataformaECommerce.Application.Features.Catalog.DTOs;

/// <summary>
/// Representa el resultado paginado de una consulta del catálogo comercial.
/// </summary>
public sealed record CatalogQueryResultDto
{
    /// <summary>
    /// Colección de productos devueltos por la consulta.
    /// </summary>
    public IReadOnlyCollection<CatalogProductDto> Items { get; init; } = Array.Empty<CatalogProductDto>();

    /// <summary>
    /// Cantidad total de productos que cumplen los filtros aplicados.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Cantidad de productos incluidos en la respuesta actual.
    /// </summary>
    public int ReturnedCount { get; init; }

    /// <summary>
    /// Número de página actual.
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Tamaño de página aplicado.
    /// </summary>
    public int PageSize { get; init; } = 20;

    /// <summary>
    /// Cantidad total de páginas calculadas.
    /// </summary>
    public int TotalPages { get; init; }

    /// <summary>
    /// Indica si existe una página previa disponible.
    /// </summary>
    public bool HasPreviousPage { get; init; }

    /// <summary>
    /// Indica si existe una página siguiente disponible.
    /// </summary>
    public bool HasNextPage { get; init; }
}
