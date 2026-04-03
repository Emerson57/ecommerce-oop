using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Products.Commands;
using PlataformaECommerce.Application.Features.Products.DTOs;

namespace PlataformaECommerce.Application.Interfaces.Services.Products;

/// <summary>
/// Define la frontera de operaciones promocionales y de merchandising del módulo de productos.
/// </summary>
public interface IProductPromotionService
{
    /// <summary>
    /// Aplica una promoción porcentual a un producto disponible.
    /// </summary>
    Task<Result<ProductResponseDto>> ApplyProductPromotionAsync(
        ApplyProductPromotionCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retira una promoción activa y restaura el precio base del producto.
    /// </summary>
    Task<Result<ProductResponseDto>> RemoveProductPromotionAsync(
        RemoveProductPromotionCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca un producto como destacado dentro del catálogo.
    /// </summary>
    Task<Result<ProductResponseDto>> FeatureProductAsync(
        FeatureProductCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retira la marca de destacado de un producto existente.
    /// </summary>
    Task<Result<ProductResponseDto>> UnfeatureProductAsync(
        UnfeatureProductCommand command,
        CancellationToken cancellationToken = default);
}
