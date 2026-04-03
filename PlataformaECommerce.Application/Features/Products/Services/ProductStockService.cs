using System.Globalization;
using FluentValidation;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Products.Commands;
using PlataformaECommerce.Application.Features.Products.DTOs;
using PlataformaECommerce.Application.Features.Products.Mappings;
using PlataformaECommerce.Application.Interfaces.Persistence;
using PlataformaECommerce.Application.Interfaces.Repositories.Products;
using PlataformaECommerce.Application.Interfaces.Services.Audit;
using PlataformaECommerce.Application.Interfaces.Services.Products;
using PlataformaECommerce.Domain.Entities.Products;

namespace PlataformaECommerce.Application.Features.Products.Services;

/// <summary>
/// Orquesta operaciones de inventario y disponibilidad del módulo de productos.
/// </summary>
public sealed class ProductStockService : IProductStockService
{
    private readonly IProductRepository _productRepository;
    private readonly IAuditTrailService _auditTrailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UpdateProductStockCommand> _updateProductStockCommandValidator;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="ProductStockService"/>.
    /// </summary>
    public ProductStockService(
        IProductRepository productRepository,
        IAuditTrailService auditTrailService,
        IUnitOfWork unitOfWork,
        IValidator<UpdateProductStockCommand> updateProductStockCommandValidator)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _auditTrailService = auditTrailService ?? throw new ArgumentNullException(nameof(auditTrailService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _updateProductStockCommandValidator = updateProductStockCommandValidator ?? throw new ArgumentNullException(nameof(updateProductStockCommandValidator));
    }

    /// <inheritdoc />
    public async Task<Result<ProductResponseDto>> UpdateProductStockAsync(
        UpdateProductStockCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = await ProductServiceSupport.ValidateAsync(command, _updateProductStockCommandValidator, cancellationToken);
        if (validationError is not null)
        {
            return Result.Failure<ProductResponseDto>(validationError);
        }

        return await ProductServiceSupport.ExecuteAsync(async () =>
        {
            Producto? product = await _productRepository.GetByIdAsync(command.ProductId, cancellationToken);
            if (product is null)
            {
                return Result.Failure<ProductResponseDto>(
                    Error.NotFound("Products.NotFound", $"No se encontró un producto con identificador '{command.ProductId}'."));
            }

            switch (command.UpdateType)
            {
                case StockUpdateType.Set:
                    product.ActualizarStock(command.Quantity);
                    break;
                case StockUpdateType.Increase:
                    product.IncrementarStock(command.Quantity);
                    break;
                case StockUpdateType.Decrease:
                    product.DisminuirStock(command.Quantity);
                    break;
                default:
                    return Result.Failure<ProductResponseDto>(
                        Error.Validation("Products.InvalidStockUpdateType", "El tipo de actualización de inventario no es válido."));
            }

            await _productRepository.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await ProductServiceSupport.AuditProductEventAsync(
                _auditTrailService,
                product,
                "product.stock.updated",
                $"Se actualizó el inventario del producto con SKU '{product.Sku.Value}'.",
                new Dictionary<string, string>
                {
                    ["updateType"] = command.UpdateType.ToString(),
                    ["quantity"] = command.Quantity.ToString(CultureInfo.InvariantCulture),
                    ["resultingStock"] = product.Stock.ToString(CultureInfo.InvariantCulture),
                    ["currency"] = product.Precio.Currency
                },
                cancellationToken);

            return Result.Success(product.ToProductResponseDto());
        }, "Products.Domain");
    }

    /// <inheritdoc />
    public async Task<Result<ProductResponseDto>> ActivateProductAsync(
        ActivateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = ProductServiceSupport.ValidateProductId(command.ProductId);
        if (validationError is not null)
        {
            return Result.Failure<ProductResponseDto>(validationError);
        }

        return await ExecuteStateChangeAsync(
            command.ProductId,
            product => product.Activar(),
            "product.activated",
            product => $"Se activó el producto con SKU '{product.Sku.Value}'.",
            product => new Dictionary<string, string>
            {
                ["isActive"] = product.Activo.ToString(),
                ["sku"] = product.Sku.Value
            },
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<ProductResponseDto>> DeactivateProductAsync(
        DeactivateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = ProductServiceSupport.ValidateProductId(command.ProductId);
        if (validationError is not null)
        {
            return Result.Failure<ProductResponseDto>(validationError);
        }

        return await ExecuteStateChangeAsync(
            command.ProductId,
            product => product.Desactivar(),
            "product.deactivated",
            product => $"Se desactivó el producto con SKU '{product.Sku.Value}'.",
            product => new Dictionary<string, string>
            {
                ["isActive"] = product.Activo.ToString(),
                ["sku"] = product.Sku.Value
            },
            cancellationToken);
    }

    private Task<Result<ProductResponseDto>> ExecuteStateChangeAsync(
        Guid productId,
        Action<Producto> stateChange,
        string action,
        Func<Producto, string> detailFactory,
        Func<Producto, IReadOnlyDictionary<string, string>> metadataFactory,
        CancellationToken cancellationToken)
    {
        return ProductServiceSupport.ExecuteAsync(async () =>
        {
            Producto? product = await _productRepository.GetByIdAsync(productId, cancellationToken);
            if (product is null)
            {
                return Result.Failure<ProductResponseDto>(
                    Error.NotFound("Products.NotFound", $"No se encontró un producto con identificador '{productId}'."));
            }

            stateChange(product);

            await _productRepository.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await ProductServiceSupport.AuditProductEventAsync(
                _auditTrailService,
                product,
                action,
                detailFactory(product),
                metadataFactory(product),
                cancellationToken);

            return Result.Success(product.ToProductResponseDto());
        }, "Products.Domain");
    }
}
