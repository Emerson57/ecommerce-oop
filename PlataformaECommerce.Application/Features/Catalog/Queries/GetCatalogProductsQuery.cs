using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Catalog.DTOs;
using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Application.Features.Catalog.Queries;

/// <summary>
/// Representa la consulta de aplicación para obtener productos del catálogo
/// aplicando criterios de búsqueda, filtrado, ordenamiento y paginación.
/// </summary>
/// <remarks>
/// Esta query modela una intención explícita de lectura dentro del módulo
/// de catálogo del e-Commerce, correspondiente al caso de uso de explorar,
/// buscar y filtrar productos disponibles para su visualización comercial.
///
/// Su responsabilidad es transportar, de forma desacoplada y consistente,
/// los criterios necesarios para que la capa Application recupere y proyecte
/// la información del catálogo hacia una colección de <see cref="CatalogProductDto"/>.
///
/// Esta clase no debe contener lógica de negocio, acceso a infraestructura
/// ni comportamiento de dominio. Dichas responsabilidades corresponden
/// a servicios de aplicación y repositorios especializados.
///
/// El resultado esperado de la operación es un <see cref="Result{TValue}"/>
/// que contiene una colección de <see cref="CatalogProductDto"/> cuando
/// la ejecución es exitosa.
///
/// La consulta está preparada para soportar escenarios profesionales como:
/// - navegación del catálogo público,
/// - filtros por categoría, marca o tipo de producto,
/// - búsqueda por texto libre,
/// - segmentación por disponibilidad o promoción,
/// - ordenamiento comercial,
/// - y paginación de resultados.
/// </remarks>
public sealed class GetCatalogProductsQuery
{
    #region Constantes

    /// <summary>
    /// Número de página por defecto.
    /// </summary>
    private const int DefaultPageNumber = 1;

    /// <summary>
    /// Tamaño de página por defecto.
    /// </summary>
    private const int DefaultPageSize = 20;

    /// <summary>
    /// Tamaño máximo de página permitido.
    /// </summary>
    private const int MaxPageSize = 100;

    #endregion

    #region Constructores

    /// <summary>
    /// Inicializa una nueva instancia vacía de la consulta.
    /// </summary>
    public GetCatalogProductsQuery()
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la consulta con un término de búsqueda.
    /// </summary>
    /// <param name="searchTerm">Término principal de búsqueda.</param>
    public GetCatalogProductsQuery(string? searchTerm)
    {
        SearchTerm = searchTerm;
    }

    #endregion

    #region Búsqueda principal

    /// <summary>
    /// Término libre de búsqueda aplicado sobre nombre, descripción,
    /// SKU, marca, etiquetas u otros campos indexables del catálogo.
    /// </summary>
    public string? SearchTerm { get; init; }

    /// <summary>
    /// Slug específico del producto, categoría o segmento cuando la consulta
    /// se origine desde navegación amigable basada en URL.
    /// </summary>
    public string? Slug { get; init; }

    #endregion

    #region Filtros comerciales y funcionales

    /// <summary>
    /// Identificador opcional de la categoría principal.
    /// </summary>
    public Guid? CategoryId { get; init; }

    /// <summary>
    /// Nombre de la categoría principal a filtrar.
    /// </summary>
    public string? CategoryName { get; init; }

    /// <summary>
    /// Identificador opcional de la subcategoría.
    /// </summary>
    public Guid? SubcategoryId { get; init; }

    /// <summary>
    /// Nombre de la subcategoría a filtrar.
    /// </summary>
    public string? SubcategoryName { get; init; }

    /// <summary>
    /// Marca comercial a filtrar.
    /// </summary>
    public string? Brand { get; init; }

    /// <summary>
    /// Tipo de producto dentro del dominio.
    /// </summary>
    public TipoProducto? ProductType { get; init; }

    /// <summary>
    /// Código SKU a filtrar de forma exacta o controlada.
    /// </summary>
    public string? Sku { get; init; }

    /// <summary>
    /// Filtra productos activos.
    /// </summary>
    public bool? IsActive { get; init; }

    /// <summary>
    /// Filtra productos disponibles para venta.
    /// </summary>
    public bool? IsAvailable { get; init; }

    /// <summary>
    /// Filtra productos con existencia disponible.
    /// </summary>
    public bool? HasStock { get; init; }

    /// <summary>
    /// Filtra productos destacados.
    /// </summary>
    public bool? IsFeatured { get; init; }

    /// <summary>
    /// Filtra productos nuevos.
    /// </summary>
    public bool? IsNew { get; init; }

    /// <summary>
    /// Filtra productos recomendados.
    /// </summary>
    public bool? IsRecommended { get; init; }

    /// <summary>
    /// Filtra productos en promoción.
    /// </summary>
    public bool? IsOnSale { get; init; }

    /// <summary>
    /// Filtra productos asociados a una etiqueta específica.
    /// </summary>
    public string? Tag { get; init; }

    #endregion

    #region Filtros monetarios

    /// <summary>
    /// Precio mínimo permitido dentro del resultado.
    /// </summary>
    public decimal? MinPrice { get; init; }

    /// <summary>
    /// Precio máximo permitido dentro del resultado.
    /// </summary>
    public decimal? MaxPrice { get; init; }

    /// <summary>
    /// Código de moneda asociado al filtro monetario.
    /// </summary>
    public string? Currency { get; init; }

    #endregion

    #region Paginación

    /// <summary>
    /// Número de página solicitado.
    /// </summary>
    public int PageNumber { get; init; } = DefaultPageNumber;

    /// <summary>
    /// Tamaño de página solicitado.
    /// </summary>
    public int PageSize { get; init; } = DefaultPageSize;

    #endregion

    #region Ordenamiento

    /// <summary>
    /// Campo lógico por el cual se desea ordenar la consulta.
    /// </summary>
    /// <remarks>
    /// Ejemplos comunes:
    /// - relevance
    /// - name
    /// - price
    /// - createdAt
    /// - rating
    /// - sales
    /// - featured
    /// </remarks>
    public string? SortBy { get; init; } = "relevance";

    /// <summary>
    /// Indica si el ordenamiento debe ser descendente.
    /// </summary>
    public bool SortDescending { get; init; } = true;

    #endregion

    #region Proyección y comportamiento de consulta

    /// <summary>
    /// Indica si se deben incluir productos inactivos cuando la implementación lo soporte.
    /// </summary>
    /// <remarks>
    /// Esta propiedad está orientada principalmente a escenarios administrativos
    /// o internos donde se requiera ampliar el espectro de consulta.
    /// </remarks>
    public bool IncludeInactive { get; init; }

    /// <summary>
    /// Indica si la consulta debe incluir métricas comerciales extendidas
    /// cuando la implementación lo soporte.
    /// </summary>
    public bool IncludeCommercialMetrics { get; init; }

    /// <summary>
    /// Indica si la consulta debe incluir información ampliada de imágenes
    /// o recursos visuales cuando la implementación lo soporte.
    /// </summary>
    public bool IncludeImageGallery { get; init; } = true;

    #endregion

    #region Contexto y trazabilidad

    /// <summary>
    /// Identificador opcional del usuario que origina la consulta.
    /// </summary>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>
    /// Canal de origen desde el cual se ejecuta la consulta.
    /// </summary>
    public string? Source { get; init; }

    /// <summary>
    /// Referencia externa opcional asociada a la consulta.
    /// </summary>
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
    /// Indica si la consulta contiene un término principal de búsqueda informado.
    /// </summary>
    public bool HasSearchTerm => !string.IsNullOrWhiteSpace(SearchTerm);

    /// <summary>
    /// Indica si la consulta contiene filtros adicionales
    /// distintos a la paginación y al ordenamiento.
    /// </summary>
    public bool HasAdditionalFilters =>
        !string.IsNullOrWhiteSpace(Slug) ||
        CategoryId.HasValue ||
        !string.IsNullOrWhiteSpace(CategoryName) ||
        SubcategoryId.HasValue ||
        !string.IsNullOrWhiteSpace(SubcategoryName) ||
        !string.IsNullOrWhiteSpace(Brand) ||
        ProductType.HasValue ||
        !string.IsNullOrWhiteSpace(Sku) ||
        IsActive.HasValue ||
        IsAvailable.HasValue ||
        HasStock.HasValue ||
        IsFeatured.HasValue ||
        IsNew.HasValue ||
        IsRecommended.HasValue ||
        IsOnSale.HasValue ||
        !string.IsNullOrWhiteSpace(Tag) ||
        MinPrice.HasValue ||
        MaxPrice.HasValue ||
        !string.IsNullOrWhiteSpace(Currency);

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida de la consulta de catálogo.
    /// </summary>
    /// <returns>Cadena representativa de la query.</returns>
    public override string ToString()
    {
        return $"GetCatalogProductsQuery | SearchTerm: {SearchTerm} | CategoryName: {CategoryName} | Brand: {Brand} | ProductType: {ProductType} | PageNumber: {NormalizedPageNumber} | PageSize: {NormalizedPageSize} | SortBy: {SortBy} | SortDescending: {SortDescending}";
    }

    #endregion
}