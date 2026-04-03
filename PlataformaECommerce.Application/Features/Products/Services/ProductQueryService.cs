using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Products.DTOs;
using PlataformaECommerce.Application.Features.Products.Mappings;
using PlataformaECommerce.Application.Features.Products.Queries;
using PlataformaECommerce.Application.Interfaces.Repositories.Products;
using PlataformaECommerce.Application.Interfaces.Services.Products;
using PlataformaECommerce.Domain.Entities.Products;

namespace PlataformaECommerce.Application.Features.Products.Services;

/// <summary>
/// Orquesta las operaciones de lectura del módulo de productos.
/// </summary>
public sealed class ProductQueryService : IProductQueryService
{
    private readonly IProductRepository _productRepository;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="ProductQueryService"/>.
    /// </summary>
    public ProductQueryService(IProductRepository productRepository)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
    }

    /// <inheritdoc />
    public async Task<Result<ProductDetailDto>> GetProductByIdAsync(
        GetProductByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.ProductId == Guid.Empty)
        {
            return Result.Failure<ProductDetailDto>(
                Error.Validation("Products.InvalidId", "El identificador del producto es obligatorio."));
        }

        Producto? product = await _productRepository.GetByIdAsync(query.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure<ProductDetailDto>(
                Error.NotFound("Products.NotFound", $"No se encontró un producto con identificador '{query.ProductId}'."));
        }

        return Result.Success(product.ToProductDetailDto());
    }

    /// <inheritdoc />
    public async Task<Result<ProductQueryResultDto>> GetProductsAsync(
        GetProductsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        IEnumerable<Producto> products = await _productRepository.GetAllAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            string term = query.SearchTerm.Trim();
            products = products.Where(product =>
                product.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                product.Descripcion.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                product.Sku.Value.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                product.Slug.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        if (query.ProductType.HasValue)
        {
            products = products.Where(product => product.TipoProducto == query.ProductType.Value);
        }

        if (query.CategoryId.HasValue)
        {
            products = products.Where(product => product.CategoriaId == query.CategoryId.Value || product.SubcategoriaId == query.CategoryId.Value);
        }

        if (query.IsActive.HasValue)
        {
            products = products.Where(product => product.Activo == query.IsActive.Value);
        }

        if (query.IsFeatured.HasValue)
        {
            products = products.Where(product => product.Destacado == query.IsFeatured.Value);
        }

        if (query.HasStock.HasValue)
        {
            products = query.HasStock.Value
                ? products.Where(product => product.Stock > 0)
                : products.Where(product => product.Stock <= 0);
        }

        if (query.MinPrice.HasValue)
        {
            products = products.Where(product => product.Precio.Amount >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            products = products.Where(product => product.Precio.Amount <= query.MaxPrice.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Currency))
        {
            string currency = query.Currency.Trim().ToUpperInvariant();
            products = products.Where(product => product.Precio.Currency.Equals(currency, StringComparison.OrdinalIgnoreCase));
        }

        products = ProductServiceSupport.ApplySorting(products, query);

        int totalCount = products.Count();
        IReadOnlyCollection<ProductDto> items = products
            .Skip(query.Offset)
            .Take(query.NormalizedPageSize)
            .ToProductDtos();

        int totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)query.NormalizedPageSize);

        return Result.Success(new ProductQueryResultDto
        {
            Items = items,
            TotalCount = totalCount,
            ReturnedCount = items.Count,
            PageNumber = query.NormalizedPageNumber,
            PageSize = query.NormalizedPageSize,
            TotalPages = totalPages,
            HasPreviousPage = query.NormalizedPageNumber > 1 && totalPages > 0,
            HasNextPage = totalPages > 0 && query.NormalizedPageNumber < totalPages
        });
    }
}
