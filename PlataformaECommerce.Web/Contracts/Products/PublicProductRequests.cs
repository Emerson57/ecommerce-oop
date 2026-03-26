using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Web.Contracts.Products;

/// <summary>
/// Representa la solicitud HTTP de consulta pública del catálogo de productos.
/// </summary>
/// <remarks>
/// Este contrato congela la superficie HTTP de filtros, ordenamiento y paginación
/// expuesta por la API pública sin acoplarla directamente a la query de Application.
/// </remarks>
public sealed record GetProductsRequest
{
    /// <summary>
    /// Texto libre de búsqueda aplicado sobre nombre, descripción o SKU.
    /// </summary>
    public string? SearchTerm { get; init; }

    /// <summary>
    /// Tipo funcional del producto.
    /// </summary>
    public TipoProducto? ProductType { get; init; }

    /// <summary>
    /// Categoría principal del producto.
    /// </summary>
    public Guid? CategoryId { get; init; }

    /// <summary>
    /// Estado activo o inactivo del producto.
    /// </summary>
    public bool? IsActive { get; init; }

    /// <summary>
    /// Estado de destacado del producto.
    /// </summary>
    public bool? IsFeatured { get; init; }

    /// <summary>
    /// Indica si se requiere disponibilidad de stock.
    /// </summary>
    public bool? HasStock { get; init; }

    /// <summary>
    /// Precio mínimo del filtro.
    /// </summary>
    public decimal? MinPrice { get; init; }

    /// <summary>
    /// Precio máximo del filtro.
    /// </summary>
    public decimal? MaxPrice { get; init; }

    /// <summary>
    /// Código de moneda del filtro.
    /// </summary>
    public string? Currency { get; init; }

    /// <summary>
    /// Número de página solicitado.
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Tamaño de página solicitado.
    /// </summary>
    public int PageSize { get; init; } = 20;

    /// <summary>
    /// Campo lógico por el cual se desea ordenar.
    /// </summary>
    public string? SortBy { get; init; }

    /// <summary>
    /// Indica si el ordenamiento debe ser descendente.
    /// </summary>
    public bool SortDescending { get; init; }

    /// <summary>
    /// Identificador opcional del usuario que solicita la consulta.
    /// </summary>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>
    /// Referencia externa opcional de la consulta.
    /// </summary>
    public string? ExternalReference { get; init; }
}
