using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Products.Commands;
using PlataformaECommerce.Application.Features.Products.DTOs;
using PlataformaECommerce.Application.Features.Products.Queries;
using FluentValidation;
using PlataformaECommerce.Application.Interfaces.Persistence;
using PlataformaECommerce.Application.Interfaces.Repositories.Categories;
using PlataformaECommerce.Application.Interfaces.Repositories.Products;
using PlataformaECommerce.Application.Interfaces.Services.Audit;
using PlataformaECommerce.Application.Interfaces.Services.Products;
using Microsoft.Extensions.DependencyInjection;

namespace PlataformaECommerce.Application.Features.Products.Services;

/// <summary>
/// Mantiene la frontera pública heredada del módulo de productos delegando en servicios especializados.
/// </summary>
public sealed class ProductApplicationService : IProductApplicationService
{
    private readonly IProductCommandService _productCommandService;
    private readonly IProductQueryService _productQueryService;
    private readonly IProductStockService _productStockService;
    private readonly IProductPromotionService _productPromotionService;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="ProductApplicationService"/>.
    /// </summary>
    [ActivatorUtilitiesConstructor]
    public ProductApplicationService(
        IProductCommandService productCommandService,
        IProductQueryService productQueryService,
        IProductStockService productStockService,
        IProductPromotionService productPromotionService)
    {
        _productCommandService = productCommandService ?? throw new ArgumentNullException(nameof(productCommandService));
        _productQueryService = productQueryService ?? throw new ArgumentNullException(nameof(productQueryService));
        _productStockService = productStockService ?? throw new ArgumentNullException(nameof(productStockService));
        _productPromotionService = productPromotionService ?? throw new ArgumentNullException(nameof(productPromotionService));
    }

    /// <summary>
    /// Inicializa una nueva instancia de compatibilidad para pruebas existentes del módulo de productos.
    /// </summary>
    public ProductApplicationService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IAuditTrailService auditTrailService,
        IUnitOfWork unitOfWork,
        IValidator<CreatePhysicalProductCommand> createPhysicalProductCommandValidator,
        IValidator<CreateDigitalProductCommand> createDigitalProductCommandValidator,
        IValidator<ImportProductsCommand> importProductsCommandValidator,
        IValidator<UpdateProductCommand> updateProductCommandValidator,
        IValidator<UpdateProductStockCommand> updateProductStockCommandValidator)
        : this(
            new ProductCommandService(
                productRepository,
                categoryRepository,
                auditTrailService,
                unitOfWork,
                createPhysicalProductCommandValidator,
                createDigitalProductCommandValidator,
                importProductsCommandValidator,
                updateProductCommandValidator),
            new ProductQueryService(productRepository),
            new ProductStockService(productRepository, auditTrailService, unitOfWork, updateProductStockCommandValidator),
            new ProductPromotionService(productRepository, auditTrailService, unitOfWork))
    {
    }

    /// <inheritdoc />
    public Task<Result<Guid>> CreatePhysicalProductAsync(
        CreatePhysicalProductCommand command,
        CancellationToken cancellationToken = default)
    {
        return _productCommandService.CreatePhysicalProductAsync(command, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<Guid>> CreateDigitalProductAsync(
        CreateDigitalProductCommand command,
        CancellationToken cancellationToken = default)
    {
        return _productCommandService.CreateDigitalProductAsync(command, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<ProductImportResultDto>> ImportProductsAsync(
        ImportProductsCommand command,
        CancellationToken cancellationToken = default)
    {
        return _productCommandService.ImportProductsAsync(command, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<ProductResponseDto>> UpdateProductAsync(
        UpdateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        return _productCommandService.UpdateProductAsync(command, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<ProductResponseDto>> UpdateProductStockAsync(
        UpdateProductStockCommand command,
        CancellationToken cancellationToken = default)
    {
        return _productStockService.UpdateProductStockAsync(command, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<ProductResponseDto>> ActivateProductAsync(
        ActivateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        return _productStockService.ActivateProductAsync(command, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<ProductResponseDto>> DeactivateProductAsync(
        DeactivateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        return _productStockService.DeactivateProductAsync(command, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<ProductResponseDto>> ApplyProductPromotionAsync(
        ApplyProductPromotionCommand command,
        CancellationToken cancellationToken = default)
    {
        return _productPromotionService.ApplyProductPromotionAsync(command, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<ProductResponseDto>> RemoveProductPromotionAsync(
        RemoveProductPromotionCommand command,
        CancellationToken cancellationToken = default)
    {
        return _productPromotionService.RemoveProductPromotionAsync(command, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<ProductResponseDto>> FeatureProductAsync(
        FeatureProductCommand command,
        CancellationToken cancellationToken = default)
    {
        return _productPromotionService.FeatureProductAsync(command, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<ProductResponseDto>> UnfeatureProductAsync(
        UnfeatureProductCommand command,
        CancellationToken cancellationToken = default)
    {
        return _productPromotionService.UnfeatureProductAsync(command, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<ProductDetailDto>> GetProductByIdAsync(
        GetProductByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        return _productQueryService.GetProductByIdAsync(query, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<ProductQueryResultDto>> GetProductsAsync(
        GetProductsQuery query,
        CancellationToken cancellationToken = default)
    {
        return _productQueryService.GetProductsAsync(query, cancellationToken);
    }
}