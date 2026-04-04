using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Catalog.DTOs;
using PlataformaECommerce.Application.Features.Catalog.Queries;

namespace PlataformaECommerce.Application.Interfaces.Services.Catalog;

/// <summary>
/// Define el contrato del servicio de aplicación encargado de coordinar
/// las consultas del catálogo comercial.
/// </summary>
/// <remarks>
/// Este contrato constituye la frontera pública del módulo de catálogo dentro de
/// <c>Application</c>. Sus consultas expresan criterios de lectura del caso de uso
/// y son procesadas por un servicio de aplicación especializado en proyección comercial.
/// </remarks>
public interface ICatalogApplicationService
{
    /// <summary>
    /// Obtiene la colección de productos del catálogo aplicando filtros y paginación.
    /// </summary>
    Task<Result<CatalogQueryResultDto>> GetCatalogProductsAsync(
        GetCatalogProductsQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la colección de productos destacados del catálogo.
    /// </summary>
    Task<Result<IReadOnlyCollection<FeaturedProductDto>>> GetFeaturedProductsAsync(
        GetFeaturedProductsQuery query,
        CancellationToken cancellationToken = default);
}
