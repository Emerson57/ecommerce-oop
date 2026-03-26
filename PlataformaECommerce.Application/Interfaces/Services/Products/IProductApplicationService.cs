using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Products.Commands;
using PlataformaECommerce.Application.Features.Products.DTOs;
using PlataformaECommerce.Application.Features.Products.Queries;

namespace PlataformaECommerce.Application.Interfaces.Services.Products;

/// <summary>
/// Define el contrato del servicio de aplicación encargado de coordinar
/// los casos de uso del módulo de productos.
/// </summary>
/// <remarks>
/// Este contrato expone una frontera estable para capas consumidoras como Web,
/// API o procesos de integración, evitando dependencia directa de la implementación
/// concreta del servicio y favoreciendo una composición más profesional del módulo.
/// Los comandos y consultas se usan aquí como modelos de entrada del caso de uso,
/// manteniendo a <c>ApplicationService</c> como única frontera pública del módulo.
/// </remarks>
public interface IProductApplicationService
{
    /// <summary>
    /// Crea un nuevo producto físico dentro del sistema.
    /// </summary>
    /// <param name="command">Comando de creación del producto físico.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Resultado que contiene el identificador del producto creado.</returns>
    Task<Result<Guid>> CreatePhysicalProductAsync(
        CreatePhysicalProductCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Crea un nuevo producto digital dentro del sistema.
    /// </summary>
    /// <param name="command">Comando de creación del producto digital.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Resultado que contiene el identificador del producto creado.</returns>
    Task<Result<Guid>> CreateDigitalProductAsync(
        CreateDigitalProductCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Importa productos físicos y digitales desde una plantilla tabular validada.
    /// </summary>
    /// <param name="command">Comando de importación masiva de productos.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Resultado con el resumen de productos creados durante la importación.</returns>
    Task<Result<ProductImportResultDto>> ImportProductsAsync(
        ImportProductsCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza la información integral de un producto existente.
    /// </summary>
    /// <param name="command">Comando de actualización del producto.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Resultado que contiene la representación actualizada del producto.</returns>
    Task<Result<ProductResponseDto>> UpdateProductAsync(
        UpdateProductCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza el inventario de un producto existente.
    /// </summary>
    /// <param name="command">Comando de actualización de inventario.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Resultado que contiene la representación actualizada del producto.</returns>
    Task<Result<ProductResponseDto>> UpdateProductStockAsync(
        UpdateProductStockCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Activa un producto existente dentro del sistema.
    /// </summary>
    /// <param name="command">Comando de activación del producto.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Resultado que contiene la representación actualizada del producto.</returns>
    Task<Result<ProductResponseDto>> ActivateProductAsync(
        ActivateProductCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Desactiva un producto existente dentro del sistema.
    /// </summary>
    /// <param name="command">Comando de desactivación del producto.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Resultado que contiene la representación actualizada del producto.</returns>
    Task<Result<ProductResponseDto>> DeactivateProductAsync(
        DeactivateProductCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Aplica una promoción porcentual a un producto disponible.
    /// </summary>
    /// <param name="command">Comando de promoción del producto.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Resultado que contiene la representación actualizada del producto.</returns>
    Task<Result<ProductResponseDto>> ApplyProductPromotionAsync(
        ApplyProductPromotionCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retira una promoción activa y restaura el precio base del producto.
    /// </summary>
    /// <param name="command">Comando de retiro de promoción.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Resultado que contiene la representación actualizada del producto.</returns>
    Task<Result<ProductResponseDto>> RemoveProductPromotionAsync(
        RemoveProductPromotionCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca un producto como destacado dentro del catálogo.
    /// </summary>
    /// <param name="command">Comando para destacar el producto.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Resultado que contiene la representación actualizada del producto.</returns>
    Task<Result<ProductResponseDto>> FeatureProductAsync(
        FeatureProductCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retira la marca de destacado de un producto existente.
    /// </summary>
    /// <param name="command">Comando para retirar el destacado del producto.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Resultado que contiene la representación actualizada del producto.</returns>
    Task<Result<ProductResponseDto>> UnfeatureProductAsync(
        UnfeatureProductCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el detalle de un producto por su identificador.
    /// </summary>
    /// <param name="query">Consulta de detalle del producto.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Resultado que contiene el detalle del producto solicitado.</returns>
    Task<Result<ProductDetailDto>> GetProductByIdAsync(
        GetProductByIdQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un listado de productos aplicando filtros, ordenamiento y paginación.
    /// </summary>
    /// <param name="query">Consulta de listado de productos.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Resultado que contiene la colección proyectada de productos y sus metadatos de paginación.</returns>
    Task<Result<ProductQueryResultDto>> GetProductsAsync(
        GetProductsQuery query,
        CancellationToken cancellationToken = default);
}
