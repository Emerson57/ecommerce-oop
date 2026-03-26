using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Catalog.DTOs;
using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Application.Features.Catalog.Queries;

/// <summary>
/// Representa la consulta de aplicación para obtener productos destacados
/// del catálogo en vitrinas, carruseles, campañas o secciones promocionales.
/// </summary>
/// <remarks>
/// Esta query modela una intención explícita de lectura dentro del módulo
/// de catálogo, correspondiente al caso de uso de consultar productos
/// priorizados para exposición comercial de alto impacto.
///
/// Su propósito es transportar los criterios funcionales necesarios para que
/// la capa Application recupere una colección de <see cref="FeaturedProductDto"/>
/// desacoplada del dominio y preparada para consumo por interfaces como:
/// - página principal,
/// - sliders promocionales,
/// - vitrinas destacadas,
/// - campañas temporales,
/// - secciones de recomendados,
/// - y espacios publicitarios internos.
///
/// Esta clase no debe contener lógica de negocio, reglas de ranking,
/// selección promocional ni acceso a infraestructura. Dichas responsabilidades
/// corresponden a servicios de aplicación y motores especializados.
///
/// El resultado esperado es un <see cref="Result{TValue}"/> que contiene una
/// colección de <see cref="FeaturedProductDto"/> cuando la ejecución es exitosa.
/// </remarks>
public sealed class GetFeaturedProductsQuery
{
    #region Constantes

    /// <summary>
    /// Cantidad máxima de productos destacados recomendada por defecto.
    /// </summary>
    private const int DefaultTake = 12;

    /// <summary>
    /// Cantidad máxima absoluta permitida para esta consulta.
    /// </summary>
    private const int MaxTake = 50;

    #endregion

    #region Constructores

    /// <summary>
    /// Inicializa una nueva instancia vacía de la consulta.
    /// </summary>
    public GetFeaturedProductsQuery()
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la consulta con una cantidad objetivo.
    /// </summary>
    /// <param name="take">Cantidad de productos destacados a recuperar.</param>
    public GetFeaturedProductsQuery(int take)
    {
        Take = take;
    }

    #endregion

    #region Alcance de la consulta

    /// <summary>
    /// Cantidad de productos destacados solicitados.
    /// </summary>
    public int Take { get; init; } = DefaultTake;

    /// <summary>
    /// Identificador opcional de la categoría principal a priorizar.
    /// </summary>
    public Guid? CategoryId { get; init; }

    /// <summary>
    /// Nombre de la categoría principal a priorizar.
    /// </summary>
    public string? CategoryName { get; init; }

    /// <summary>
    /// Marca comercial a priorizar en la selección.
    /// </summary>
    public string? Brand { get; init; }

    /// <summary>
    /// Tipo de producto dentro del dominio a priorizar.
    /// </summary>
    public TipoProducto? ProductType { get; init; }

    #endregion

    #region Segmentación comercial

    /// <summary>
    /// Indica si la consulta debe limitarse a productos en promoción.
    /// </summary>
    public bool? OnlyOnSale { get; init; }

    /// <summary>
    /// Indica si la consulta debe limitarse a productos nuevos.
    /// </summary>
    public bool? OnlyNew { get; init; }

    /// <summary>
    /// Indica si la consulta debe limitarse a productos recomendados.
    /// </summary>
    public bool? OnlyRecommended { get; init; }

    /// <summary>
    /// Indica si la consulta debe limitarse a productos más vendidos.
    /// </summary>
    public bool? OnlyBestSellers { get; init; }

    /// <summary>
    /// Indica si la consulta debe limitarse a productos disponibles para venta.
    /// </summary>
    public bool? OnlyAvailable { get; init; }

    /// <summary>
    /// Indica si la consulta debe limitarse a productos con stock.
    /// </summary>
    public bool? OnlyWithStock { get; init; }

    /// <summary>
    /// Etiqueta promocional o de campaña utilizada como filtro.
    /// </summary>
    public string? BadgeText { get; init; }

    /// <summary>
    /// Clave lógica de campaña, colección o vitrina a consultar.
    /// </summary>
    /// <remarks>
    /// Ejemplos comunes:
    /// - home-main
    /// - home-secondary
    /// - black-friday
    /// - cyber-week
    /// - christmas
    /// - recommended
    /// </remarks>
    public string? CampaignKey { get; init; }

    /// <summary>
    /// Zona o slot visual donde se desea consumir los productos destacados.
    /// </summary>
    /// <remarks>
    /// Ejemplos comunes:
    /// - hero
    /// - slider
    /// - home-grid
    /// - sidebar
    /// - popup
    /// </remarks>
    public string? Placement { get; init; }

    #endregion

    #region Ordenamiento y comportamiento

    /// <summary>
    /// Campo lógico de ordenamiento deseado.
    /// </summary>
    /// <remarks>
    /// Ejemplos comunes:
    /// - displayOrder
    /// - relevance
    /// - sales
    /// - rating
    /// - createdAt
    /// - price
    /// </remarks>
    public string? SortBy { get; init; } = "displayOrder";

    /// <summary>
    /// Indica si el ordenamiento debe ser descendente.
    /// </summary>
    public bool SortDescending { get; init; }

    /// <summary>
    /// Indica si la consulta debe considerar vigencia temporal
    /// de campañas destacadas cuando la implementación lo soporte.
    /// </summary>
    public bool RespectFeaturedWindow { get; init; } = true;

    /// <summary>
    /// Indica si la consulta debe incluir recursos visuales ampliados
    /// cuando la implementación lo soporte.
    /// </summary>
    public bool IncludeVisualAssets { get; init; } = true;

    /// <summary>
    /// Indica si la consulta debe incluir métricas comerciales
    /// cuando la implementación lo soporte.
    /// </summary>
    public bool IncludeCommercialMetrics { get; init; }

    #endregion

    #region Contexto de personalización y trazabilidad

    /// <summary>
    /// Identificador opcional del usuario para personalización contextual.
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

    /// <summary>
    /// Fecha y hora UTC de referencia para campañas o vigencia temporal,
    /// cuando la capa superior desee controlarla explícitamente.
    /// </summary>
    public DateTime? ReferenceDateUtc { get; init; }

    #endregion

    #region Propiedades calculadas

    /// <summary>
    /// Obtiene la cantidad normalizada de resultados solicitados.
    /// </summary>
    public int NormalizedTake
    {
        get
        {
            if (Take < 1)
            {
                return DefaultTake;
            }

            return Take > MaxTake
                ? MaxTake
                : Take;
        }
    }

    /// <summary>
    /// Indica si la consulta contiene filtros adicionales
    /// aparte de la cantidad a recuperar.
    /// </summary>
    public bool HasAdditionalFilters =>
        CategoryId.HasValue ||
        !string.IsNullOrWhiteSpace(CategoryName) ||
        !string.IsNullOrWhiteSpace(Brand) ||
        ProductType.HasValue ||
        OnlyOnSale.HasValue ||
        OnlyNew.HasValue ||
        OnlyRecommended.HasValue ||
        OnlyBestSellers.HasValue ||
        OnlyAvailable.HasValue ||
        OnlyWithStock.HasValue ||
        !string.IsNullOrWhiteSpace(BadgeText) ||
        !string.IsNullOrWhiteSpace(CampaignKey) ||
        !string.IsNullOrWhiteSpace(Placement);

    /// <summary>
    /// Obtiene la fecha UTC efectiva de referencia para evaluación temporal.
    /// </summary>
    public DateTime EffectiveReferenceDateUtc => ReferenceDateUtc ?? DateTime.UtcNow;

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida de la consulta de productos destacados.
    /// </summary>
    /// <returns>Cadena representativa de la query.</returns>
    public override string ToString()
    {
        return $"GetFeaturedProductsQuery | Take: {NormalizedTake} | CategoryName: {CategoryName} | Brand: {Brand} | ProductType: {ProductType} | CampaignKey: {CampaignKey} | Placement: {Placement} | SortBy: {SortBy} | SortDescending: {SortDescending}";
    }

    #endregion
}