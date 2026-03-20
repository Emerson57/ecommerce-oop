using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Products.DTOs;
using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Application.Features.Products.Queries;

/// <summary>
/// Representa la consulta de aplicación para obtener un listado de productos
/// a partir de criterios de búsqueda, filtrado, ordenamiento y paginación.
/// </summary>
/// <remarks>
/// Esta query modela una intención explícita de lectura dentro del sistema,
/// correspondiente al caso de uso de consultar productos del catálogo
/// o del backoffice administrativo.
///
/// Su responsabilidad es transportar los criterios necesarios para que
/// la capa Application recupere, filtre y proyecte la información
/// de productos hacia una colección desacoplada del dominio.
///
/// El resultado esperado de la operación es un <see cref="Result{TValue}"/>
/// que contiene una colección de <see cref="ProductDto"/> cuando la ejecución es exitosa.
///
/// Esta consulta no debe contener lógica de negocio ni acceso a infraestructura;
/// dichas responsabilidades pertenecen al servicio de aplicación y a los componentes
/// especializados de la capa Application e Infrastructure.
/// </remarks>
public sealed class GetProductsQuery
{
    #region Constantes

    /// <summary>
    /// Tamaño de página por defecto para la consulta de productos.
    /// </summary>
    private const int DefaultPageSize = 20;

    /// <summary>
    /// Número de página por defecto para la consulta de productos.
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
    public GetProductsQuery()
    {
    }

    #endregion

    #region Filtros de búsqueda

    /// <summary>
    /// Texto libre de búsqueda aplicado sobre nombre, descripción, SKU
    /// u otros campos que la implementación soporte.
    /// </summary>
    public string? SearchTerm { get; init; }

    /// <summary>
    /// Filtra los productos por tipo funcional.
    /// </summary>
    public TipoProducto? ProductType { get; init; }

    /// <summary>
    /// Filtra los productos por identificador de categoría.
    /// </summary>
    public Guid? CategoryId { get; init; }

    /// <summary>
    /// Filtra productos activos o inactivos.
    /// </summary>
    public bool? IsActive { get; init; }

    /// <summary>
    /// Filtra productos destacados o no destacados.
    /// </summary>
    public bool? IsFeatured { get; init; }

    /// <summary>
    /// Filtra productos que tengan o no inventario disponible.
    /// </summary>
    public bool? HasStock { get; init; }

    /// <summary>
    /// Filtra productos cuyo precio sea mayor o igual al valor indicado.
    /// </summary>
    public decimal? MinPrice { get; init; }

    /// <summary>
    /// Filtra productos cuyo precio sea menor o igual al valor indicado.
    /// </summary>
    public decimal? MaxPrice { get; init; }

    /// <summary>
    /// Filtra productos por código de moneda.
    /// </summary>
    public string? Currency { get; init; }

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
    /// - name
    /// - price
    /// - stock
    /// - createdAt
    /// - updatedAt
    /// </remarks>
    public string? SortBy { get; init; }

    /// <summary>
    /// Indica si el ordenamiento debe ser descendente.
    /// </summary>
    public bool SortDescending { get; init; }

    #endregion

    #region Contexto opcional

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

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida de la consulta.
    /// </summary>
    /// <returns>Cadena representativa de la query.</returns>
    public override string ToString()
    {
        return $"GetProductsQuery | SearchTerm: {SearchTerm} | ProductType: {ProductType} | IsActive: {IsActive} | IsFeatured: {IsFeatured} | PageNumber: {NormalizedPageNumber} | PageSize: {NormalizedPageSize} | SortBy: {SortBy} | SortDescending: {SortDescending}";
    }

    #endregion
}