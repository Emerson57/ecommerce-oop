using System.Globalization;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Products.Commands;
using PlataformaECommerce.Application.Features.Products.DTOs;
using PlataformaECommerce.Application.Features.Products.Mappings;
using PlataformaECommerce.Application.Interfaces.Persistence;
using PlataformaECommerce.Application.Interfaces.Repositories.Products;
using PlataformaECommerce.Application.Interfaces.Services.Audit;
using PlataformaECommerce.Application.Interfaces.Services.Products;
using PlataformaECommerce.Domain.Entities.Products;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Application.Features.Products.Services;

/// <summary>
/// Orquesta operaciones promocionales y de merchandising del módulo de productos.
/// </summary>
public sealed class ProductPromotionService : IProductPromotionService
{
    private readonly IProductRepository _productRepository;
    private readonly IAuditTrailService _auditTrailService;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="ProductPromotionService"/>.
    /// </summary>
    public ProductPromotionService(
        IProductRepository productRepository,
        IAuditTrailService auditTrailService,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _auditTrailService = auditTrailService ?? throw new ArgumentNullException(nameof(auditTrailService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result<ProductResponseDto>> ApplyProductPromotionAsync(
        ApplyProductPromotionCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = ProductServiceSupport.ValidateProductId(command.ProductId)
            ?? ProductServiceSupport.ValidatePromotionPercentage(command.DiscountPercentage);
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

            if (!product.EstaDisponible())
            {
                return Result.Failure<ProductResponseDto>(
                    Error.Failure("Products.NotAvailable", "No es posible aplicar una promoción a un producto no disponible."));
            }

            Money previousPrice = product.Precio;
            Money previousBasePrice = product.PrecioBase;
            product.AplicarPromocion(command.DiscountPercentage);

            await _productRepository.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await ProductServiceSupport.AuditProductEventAsync(
                _auditTrailService,
                product,
                "product.promotion.applied",
                $"Se aplicó una promoción sobre el producto con SKU '{product.Sku.Value}'.",
                new Dictionary<string, string>
                {
                    ["discountPercentage"] = command.DiscountPercentage.ToString(CultureInfo.InvariantCulture),
                    ["previousPrice"] = previousPrice.Amount.ToString(CultureInfo.InvariantCulture),
                    ["basePrice"] = previousBasePrice.Amount.ToString(CultureInfo.InvariantCulture),
                    ["newPrice"] = product.Precio.Amount.ToString(CultureInfo.InvariantCulture),
                    ["currency"] = product.Precio.Currency
                },
                cancellationToken);

            return Result.Success(product.ToProductResponseDto());
        }, "Products.Domain");
    }

    /// <inheritdoc />
    public async Task<Result<ProductResponseDto>> RemoveProductPromotionAsync(
        RemoveProductPromotionCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = ProductServiceSupport.ValidateProductId(command.ProductId);
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

            if (!product.TienePromocion)
            {
                return Result.Failure<ProductResponseDto>(
                    Error.Failure("Products.PromotionNotActive", "El producto no tiene una promoción activa para restaurar."));
            }

            Money previousPromotionalPrice = product.Precio;
            product.QuitarPromocion();

            await _productRepository.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await ProductServiceSupport.AuditProductEventAsync(
                _auditTrailService,
                product,
                "product.promotion.removed",
                $"Se retiró la promoción activa del producto con SKU '{product.Sku.Value}'.",
                new Dictionary<string, string>
                {
                    ["previousPromotionalPrice"] = previousPromotionalPrice.Amount.ToString(CultureInfo.InvariantCulture),
                    ["restoredBasePrice"] = product.PrecioBase.Amount.ToString(CultureInfo.InvariantCulture),
                    ["currency"] = product.Precio.Currency
                },
                cancellationToken);

            return Result.Success(product.ToProductResponseDto());
        }, "Products.Domain");
    }

    /// <inheritdoc />
    public async Task<Result<ProductResponseDto>> FeatureProductAsync(
        FeatureProductCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = ProductServiceSupport.ValidateProductId(command.ProductId);
        if (validationError is not null)
        {
            return Result.Failure<ProductResponseDto>(validationError);
        }

        return await ExecuteFeatureChangeAsync(
            command.ProductId,
            product => product.MarcarComoDestacado(),
            "product.featured",
            product => $"Se marcó como destacado el producto con SKU '{product.Sku.Value}'.",
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<ProductResponseDto>> UnfeatureProductAsync(
        UnfeatureProductCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = ProductServiceSupport.ValidateProductId(command.ProductId);
        if (validationError is not null)
        {
            return Result.Failure<ProductResponseDto>(validationError);
        }

        return await ExecuteFeatureChangeAsync(
            command.ProductId,
            product => product.QuitarDestacado(),
            "product.unfeatured",
            product => $"Se retiró la marca de destacado del producto con SKU '{product.Sku.Value}'.",
            cancellationToken);
    }

    private Task<Result<ProductResponseDto>> ExecuteFeatureChangeAsync(
        Guid productId,
        Action<Producto> featureChange,
        string action,
        Func<Producto, string> detailFactory,
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

            featureChange(product);

            await _productRepository.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await ProductServiceSupport.AuditProductEventAsync(
                _auditTrailService,
                product,
                action,
                detailFactory(product),
                new Dictionary<string, string>
                {
                    ["isFeatured"] = product.Destacado.ToString(),
                    ["sku"] = product.Sku.Value
                },
                cancellationToken);

            return Result.Success(product.ToProductResponseDto());
        }, "Products.Domain");
    }
}
