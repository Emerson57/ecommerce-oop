using System.Globalization;
using FluentValidation;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Products.Commands;
using PlataformaECommerce.Application.Features.Products.DTOs;
using PlataformaECommerce.Application.Interfaces.Persistence;
using PlataformaECommerce.Application.Interfaces.Repositories.Categories;
using PlataformaECommerce.Application.Interfaces.Repositories.Products;
using PlataformaECommerce.Application.Interfaces.Services.Audit;
using PlataformaECommerce.Application.Interfaces.Services.Products;
using PlataformaECommerce.Domain.Entities.Products;
using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Application.Features.Products.Services;

/// <summary>
/// Orquesta las operaciones de escritura principal del módulo de productos.
/// </summary>
public sealed class ProductCommandService : IProductCommandService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IAuditTrailService _auditTrailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreatePhysicalProductCommand> _createPhysicalProductCommandValidator;
    private readonly IValidator<CreateDigitalProductCommand> _createDigitalProductCommandValidator;
    private readonly IValidator<ImportProductsCommand> _importProductsCommandValidator;
    private readonly IValidator<UpdateProductCommand> _updateProductCommandValidator;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="ProductCommandService"/>.
    /// </summary>
    public ProductCommandService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IAuditTrailService auditTrailService,
        IUnitOfWork unitOfWork,
        IValidator<CreatePhysicalProductCommand> createPhysicalProductCommandValidator,
        IValidator<CreateDigitalProductCommand> createDigitalProductCommandValidator,
        IValidator<ImportProductsCommand> importProductsCommandValidator,
        IValidator<UpdateProductCommand> updateProductCommandValidator)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _auditTrailService = auditTrailService ?? throw new ArgumentNullException(nameof(auditTrailService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _createPhysicalProductCommandValidator = createPhysicalProductCommandValidator ?? throw new ArgumentNullException(nameof(createPhysicalProductCommandValidator));
        _createDigitalProductCommandValidator = createDigitalProductCommandValidator ?? throw new ArgumentNullException(nameof(createDigitalProductCommandValidator));
        _importProductsCommandValidator = importProductsCommandValidator ?? throw new ArgumentNullException(nameof(importProductsCommandValidator));
        _updateProductCommandValidator = updateProductCommandValidator ?? throw new ArgumentNullException(nameof(updateProductCommandValidator));
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> CreatePhysicalProductAsync(
        CreatePhysicalProductCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = await ProductServiceSupport.ValidateAsync(command, _createPhysicalProductCommandValidator, cancellationToken);
        if (validationError is not null)
        {
            return Result.Failure<Guid>(validationError);
        }

        Error? categoryValidationError = await ProductServiceSupport.ValidateCategoryAssignmentAsync(
            _categoryRepository,
            command.CategoryId,
            command.SubcategoryId,
            cancellationToken);
        if (categoryValidationError is not null)
        {
            return Result.Failure<Guid>(categoryValidationError);
        }

        return await ProductServiceSupport.ExecuteAsync(async () =>
        {
            bool skuExists = await _productRepository.ExistsBySkuAsync(command.Sku, cancellationToken);
            if (skuExists)
            {
                return Result.Failure<Guid>(
                    Error.Conflict("Products.SkuAlreadyExists", $"Ya existe un producto registrado con el SKU '{command.Sku}'."));
            }

            ProductoFisico product = ProductServiceSupport.CreatePhysicalProduct(command);
            ProductServiceSupport.ApplyCommercialFlags(product, command.IsActive, command.IsFeatured);

            await _productRepository.AddAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await ProductServiceSupport.AuditProductEventAsync(
                _auditTrailService,
                product,
                "product.created",
                $"Se registró un nuevo producto físico con SKU '{product.Sku.Value}'.",
                new Dictionary<string, string>
                {
                    ["productType"] = product.TipoProducto.ToString(),
                    ["sku"] = product.Sku.Value,
                    ["currency"] = product.Precio.Currency,
                    ["initialStock"] = product.Stock.ToString(CultureInfo.InvariantCulture)
                },
                cancellationToken);

            return Result.Success(product.Id);
        }, "Products.Domain");
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> CreateDigitalProductAsync(
        CreateDigitalProductCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = await ProductServiceSupport.ValidateAsync(command, _createDigitalProductCommandValidator, cancellationToken);
        if (validationError is not null)
        {
            return Result.Failure<Guid>(validationError);
        }

        Error? categoryValidationError = await ProductServiceSupport.ValidateCategoryAssignmentAsync(
            _categoryRepository,
            command.CategoryId,
            command.SubcategoryId,
            cancellationToken);
        if (categoryValidationError is not null)
        {
            return Result.Failure<Guid>(categoryValidationError);
        }

        return await ProductServiceSupport.ExecuteAsync(async () =>
        {
            bool skuExists = await _productRepository.ExistsBySkuAsync(command.Sku, cancellationToken);
            if (skuExists)
            {
                return Result.Failure<Guid>(
                    Error.Conflict("Products.SkuAlreadyExists", $"Ya existe un producto registrado con el SKU '{command.Sku}'."));
            }

            ProductoDigital product = ProductServiceSupport.CreateDigitalProduct(command);
            ProductServiceSupport.ApplyCommercialFlags(product, command.IsActive, command.IsFeatured);

            await _productRepository.AddAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await ProductServiceSupport.AuditProductEventAsync(
                _auditTrailService,
                product,
                "product.created",
                $"Se registró un nuevo producto digital con SKU '{product.Sku.Value}'.",
                new Dictionary<string, string>
                {
                    ["productType"] = product.TipoProducto.ToString(),
                    ["sku"] = product.Sku.Value,
                    ["currency"] = product.Precio.Currency,
                    ["initialStock"] = product.Stock.ToString(CultureInfo.InvariantCulture)
                },
                cancellationToken);

            return Result.Success(product.Id);
        }, "Products.Domain");
    }

    /// <inheritdoc />
    public async Task<Result<ProductImportResultDto>> ImportProductsAsync(
        ImportProductsCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = await ProductServiceSupport.ValidateAsync(command, _importProductsCommandValidator, cancellationToken);
        if (validationError is not null)
        {
            return Result.Failure<ProductImportResultDto>(validationError);
        }

        return await ProductServiceSupport.ExecuteAsync(async () =>
        {
            IReadOnlyCollection<Producto> existingProducts = await _productRepository.GetAllAsync(cancellationToken);
            IReadOnlyCollection<PlataformaECommerce.Domain.Entities.Categories.CategoriaProducto> categories = await _categoryRepository.GetAllAsync(cancellationToken);

            string? duplicatedImportSku = command.Rows
                .GroupBy(row => row.Sku, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(duplicatedImportSku))
            {
                return Result.Failure<ProductImportResultDto>(
                    Error.Validation("Products.ImportDuplicatedSku", $"El SKU '{duplicatedImportSku}' está repetido dentro del archivo de importación."));
            }

            string? existingSkuConflict = command.Rows
                .Select(row => row.Sku)
                .FirstOrDefault(sku => existingProducts.Any(product => product.Sku.Value.Equals(sku, StringComparison.OrdinalIgnoreCase)));

            if (!string.IsNullOrWhiteSpace(existingSkuConflict))
            {
                return Result.Failure<ProductImportResultDto>(
                    Error.Conflict("Products.SkuAlreadyExists", $"Ya existe un producto registrado con el SKU '{existingSkuConflict}'."));
            }

            int physicalProductsCreated = 0;
            int digitalProductsCreated = 0;
            List<Producto> importedProducts = [];

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                foreach (ImportProductRowCommand row in command.Rows.OrderBy(current => current.RowNumber))
                {
                    Result<(Guid CategoryId, Guid? SubcategoryId)> categoryResolution = ProductServiceSupport.ResolveCategoryAssignment(row, categories);
                    if (categoryResolution.IsFailure)
                    {
                        await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                        return Result.Failure<ProductImportResultDto>(categoryResolution.Error);
                    }

                    switch (row.ProductType)
                    {
                        case TipoProducto.Fisico:
                        {
                            CreatePhysicalProductCommand physicalCommand = new()
                            {
                                Name = row.Name,
                                Description = row.Description,
                                Sku = row.Sku,
                                Price = row.Price,
                                Currency = row.Currency,
                                Stock = row.Stock,
                                Slug = row.Slug,
                                MainImageUrl = null,
                                ImageGallery = Array.Empty<string>(),
                                IsActive = row.IsActive,
                                IsFeatured = false,
                                CategoryId = categoryResolution.Value.CategoryId,
                                SubcategoryId = categoryResolution.Value.SubcategoryId,
                                Tags = ProductServiceSupport.ParseSerializedTags(row.SerializedTags),
                                WeightKg = row.WeightKg ?? 0m,
                                HeightCm = row.HeightCm ?? 0m,
                                WidthCm = row.WidthCm ?? 0m,
                                LengthCm = row.LengthCm ?? 0m,
                                RequiresShipping = row.RequiresShipping ?? true
                            };

                            Error? rowValidationError = await ProductServiceSupport.ValidateAsync(physicalCommand, _createPhysicalProductCommandValidator, cancellationToken);
                            if (rowValidationError is not null)
                            {
                                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                                return Result.Failure<ProductImportResultDto>(ProductServiceSupport.WrapImportRowError(row.RowNumber, rowValidationError));
                            }

                            ProductoFisico physicalProduct = ProductServiceSupport.CreatePhysicalProduct(physicalCommand);
                            ProductServiceSupport.ApplyCommercialFlags(physicalProduct, physicalCommand.IsActive, physicalCommand.IsFeatured);
                            await _productRepository.AddAsync(physicalProduct, cancellationToken);
                            importedProducts.Add(physicalProduct);
                            physicalProductsCreated++;
                            break;
                        }
                        case TipoProducto.Digital:
                        {
                            CreateDigitalProductCommand digitalCommand = new()
                            {
                                Name = row.Name,
                                Description = row.Description,
                                Sku = row.Sku,
                                Price = row.Price,
                                Currency = row.Currency,
                                Stock = row.Stock,
                                Slug = row.Slug,
                                MainImageUrl = null,
                                ImageGallery = Array.Empty<string>(),
                                IsActive = row.IsActive,
                                IsFeatured = false,
                                CategoryId = categoryResolution.Value.CategoryId,
                                SubcategoryId = categoryResolution.Value.SubcategoryId,
                                Tags = ProductServiceSupport.ParseSerializedTags(row.SerializedTags),
                                FileFormat = ProductServiceSupport.Normalize(row.FileFormat) ?? string.Empty,
                                FileSizeMb = row.FileSizeMb,
                                RequiresLicense = row.RequiresLicense ?? false
                            };

                            Error? rowValidationError = await ProductServiceSupport.ValidateAsync(digitalCommand, _createDigitalProductCommandValidator, cancellationToken);
                            if (rowValidationError is not null)
                            {
                                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                                return Result.Failure<ProductImportResultDto>(ProductServiceSupport.WrapImportRowError(row.RowNumber, rowValidationError));
                            }

                            ProductoDigital digitalProduct = ProductServiceSupport.CreateDigitalProduct(digitalCommand);
                            ProductServiceSupport.ApplyCommercialFlags(digitalProduct, digitalCommand.IsActive, digitalCommand.IsFeatured);
                            await _productRepository.AddAsync(digitalProduct, cancellationToken);
                            importedProducts.Add(digitalProduct);
                            digitalProductsCreated++;
                            break;
                        }
                        default:
                            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                            return Result.Failure<ProductImportResultDto>(
                                Error.Validation("Products.ImportInvalidType", $"La fila {row.RowNumber} contiene un tipo de producto no soportado."));
                    }
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                foreach (Producto importedProduct in importedProducts)
                {
                    await ProductServiceSupport.AuditProductEventAsync(
                        _auditTrailService,
                        importedProduct,
                        "product.imported",
                        $"Se importó el producto con SKU '{importedProduct.Sku.Value}' desde la plantilla Excel administrativa.",
                        new Dictionary<string, string>
                        {
                            ["productType"] = importedProduct.TipoProducto.ToString(),
                            ["sku"] = importedProduct.Sku.Value,
                            ["currency"] = importedProduct.Precio.Currency,
                            ["stock"] = importedProduct.Stock.ToString(CultureInfo.InvariantCulture)
                        },
                        cancellationToken);
                }

                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }

            return Result.Success(new ProductImportResultDto
            {
                PhysicalProductsCreated = physicalProductsCreated,
                DigitalProductsCreated = digitalProductsCreated
            });
        }, "Products.Domain");
    }

    /// <inheritdoc />
    public async Task<Result<ProductResponseDto>> UpdateProductAsync(
        UpdateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = await ProductServiceSupport.ValidateAsync(command, _updateProductCommandValidator, cancellationToken);
        if (validationError is not null)
        {
            return Result.Failure<ProductResponseDto>(validationError);
        }

        Error? categoryValidationError = await ProductServiceSupport.ValidateCategoryAssignmentAsync(
            _categoryRepository,
            command.CategoryId,
            command.SubcategoryId,
            cancellationToken);
        if (categoryValidationError is not null)
        {
            return Result.Failure<ProductResponseDto>(categoryValidationError);
        }

        return await ProductServiceSupport.ExecuteAsync(async () =>
        {
            Producto? product = await _productRepository.GetByIdAsync(command.Id, cancellationToken);
            if (product is null)
            {
                return Result.Failure<ProductResponseDto>(
                    Error.NotFound("Products.NotFound", $"No se encontró un producto con identificador '{command.Id}'."));
            }

            Producto? productWithSameSku = await _productRepository.GetBySkuAsync(command.Sku, cancellationToken);
            if (productWithSameSku is not null && productWithSameSku.Id != product.Id)
            {
                return Result.Failure<ProductResponseDto>(
                    Error.Conflict("Products.SkuAlreadyExists", $"Ya existe un producto registrado con el SKU '{command.Sku}'."));
            }

            product.ActualizarInformacionBasica(
                command.Name,
                command.Description,
                new PlataformaECommerce.Domain.ValueObjects.Sku(command.Sku),
                ProductServiceSupport.CreateMoney(command.Price, command.Currency),
                command.Slug,
                command.MainImageUrl);
            product.ActualizarGaleriaImagenes(command.ImageGallery);
            product.ActualizarClasificacion(
                command.CategoryId,
                command.SubcategoryId,
                ProductServiceSupport.CreateTags(command.Tags));
            product.ActualizarStock(command.Stock);
            ProductServiceSupport.ApplyCommercialFlags(product, command.IsActive, command.IsFeatured);

            switch (product)
            {
                case ProductoFisico physicalProduct when command.ProductType == TipoProducto.Fisico:
                    physicalProduct.ActualizarInformacionFisica(
                        command.WeightKg!.Value,
                        command.HeightCm!.Value,
                        command.WidthCm!.Value,
                        command.LengthCm!.Value,
                        command.RequiresShipping!.Value);
                    break;
                case ProductoDigital digitalProduct when command.ProductType == TipoProducto.Digital:
                    digitalProduct.ActualizarInformacionDigital(
                        command.FileFormat!,
                        command.FileSizeMb,
                        command.RequiresLicense!.Value);
                    break;
                case ProductoFisico:
                    return Result.Failure<ProductResponseDto>(
                        Error.Validation("Products.InvalidTypeChange", "No es válido actualizar un producto físico utilizando información de producto digital."));
                case ProductoDigital:
                    return Result.Failure<ProductResponseDto>(
                        Error.Validation("Products.InvalidTypeChange", "No es válido actualizar un producto digital utilizando información de producto físico."));
            }

            await _productRepository.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await ProductServiceSupport.AuditProductEventAsync(
                _auditTrailService,
                product,
                "product.updated",
                $"Se actualizó integralmente el producto con SKU '{product.Sku.Value}'.",
                new Dictionary<string, string>
                {
                    ["productType"] = product.TipoProducto.ToString(),
                    ["sku"] = product.Sku.Value,
                    ["currency"] = product.Precio.Currency,
                    ["stock"] = product.Stock.ToString(CultureInfo.InvariantCulture),
                    ["isActive"] = product.Activo.ToString(),
                    ["isFeatured"] = product.Destacado.ToString()
                },
                cancellationToken);

            return Result.Success(PlataformaECommerce.Application.Features.Products.Mappings.ProductMappings.ToProductResponseDto(product));
        }, "Products.Domain");
    }
}
