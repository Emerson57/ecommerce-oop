using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Products.Commands;
using PlataformaECommerce.Application.Features.Products.DTOs;

namespace PlataformaECommerce.Application.Interfaces.Services.Products;

/// <summary>
/// Define la frontera de operaciones de inventario y disponibilidad del módulo de productos.
/// </summary>
public interface IProductStockService
{
    /// <summary>
    /// Actualiza el inventario de un producto existente.
    /// </summary>
    Task<Result<ProductResponseDto>> UpdateProductStockAsync(
        UpdateProductStockCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Activa un producto existente dentro del sistema.
    /// </summary>
    Task<Result<ProductResponseDto>> ActivateProductAsync(
        ActivateProductCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Desactiva un producto existente dentro del sistema.
    /// </summary>
    Task<Result<ProductResponseDto>> DeactivateProductAsync(
        DeactivateProductCommand command,
        CancellationToken cancellationToken = default);
}
