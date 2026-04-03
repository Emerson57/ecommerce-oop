using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Products.DTOs;
using PlataformaECommerce.Application.Features.Products.Queries;

namespace PlataformaECommerce.Application.Interfaces.Services.Products;

/// <summary>
/// Define la frontera de lectura del módulo de productos.
/// </summary>
public interface IProductQueryService
{
    /// <summary>
    /// Obtiene el detalle de un producto por su identificador.
    /// </summary>
    Task<Result<ProductDetailDto>> GetProductByIdAsync(
        GetProductByIdQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un listado de productos aplicando filtros, ordenamiento y paginación.
    /// </summary>
    Task<Result<ProductQueryResultDto>> GetProductsAsync(
        GetProductsQuery query,
        CancellationToken cancellationToken = default);
}
