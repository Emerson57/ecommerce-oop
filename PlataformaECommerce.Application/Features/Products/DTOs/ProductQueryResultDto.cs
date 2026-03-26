namespace PlataformaECommerce.Application.Features.Products.DTOs;

/// <summary>
/// Representa el resultado paginado de una consulta administrativa o pública de productos.
/// </summary>
/// <remarks>
/// Este DTO encapsula la colección proyectada de productos junto con metadatos de
/// paginación para soportar navegación profesional, resúmenes de resultados y
/// preservación consistente del estado de la consulta en capas superiores.
/// </remarks>
public sealed record ProductQueryResultDto
{
    /// <summary>
    /// Obtiene o establece la colección de productos devueltos por la consulta.
    /// </summary>
    public IReadOnlyCollection<ProductDto> Items { get; init; } = Array.Empty<ProductDto>();

    /// <summary>
    /// Obtiene o establece la cantidad total de productos que cumplen los filtros aplicados.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Obtiene o establece la cantidad de productos incluidos efectivamente en la respuesta.
    /// </summary>
    public int ReturnedCount { get; init; }

    /// <summary>
    /// Obtiene o establece el número de página actual del resultado.
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Obtiene o establece el tamaño de página aplicado a la consulta.
    /// </summary>
    public int PageSize { get; init; } = 20;

    /// <summary>
    /// Obtiene o establece la cantidad total de páginas calculadas para la consulta.
    /// </summary>
    public int TotalPages { get; init; }

    /// <summary>
    /// Obtiene o establece un valor que indica si existe una página anterior disponible.
    /// </summary>
    public bool HasPreviousPage { get; init; }

    /// <summary>
    /// Obtiene o establece un valor que indica si existe una página siguiente disponible.
    /// </summary>
    public bool HasNextPage { get; init; }
}
