using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Products.Commands;
using PlataformaECommerce.Application.Features.Products.DTOs;

namespace PlataformaECommerce.Application.Interfaces.Services.Products;

/// <summary>
/// Define la frontera de escritura principal del módulo de productos.
/// </summary>
public interface IProductCommandService
{
    /// <summary>
    /// Crea un nuevo producto físico dentro del sistema.
    /// </summary>
    Task<Result<Guid>> CreatePhysicalProductAsync(
        CreatePhysicalProductCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Crea un nuevo producto digital dentro del sistema.
    /// </summary>
    Task<Result<Guid>> CreateDigitalProductAsync(
        CreateDigitalProductCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Importa productos desde una plantilla tabular validada.
    /// </summary>
    Task<Result<ProductImportResultDto>> ImportProductsAsync(
        ImportProductsCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza la información integral de un producto existente.
    /// </summary>
    Task<Result<ProductResponseDto>> UpdateProductAsync(
        UpdateProductCommand command,
        CancellationToken cancellationToken = default);
}
