using System.Globalization;
using FluentValidation;
using PlataformaECommerce.Application.Common.Execution;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Products.Commands;
using PlataformaECommerce.Application.Features.Products.DTOs;
using PlataformaECommerce.Application.Features.Products.Queries;
using PlataformaECommerce.Application.Features.Products.Validators;
using PlataformaECommerce.Application.Interfaces.Services.Audit;
using PlataformaECommerce.Application.Interfaces.Persistence;
using PlataformaECommerce.Application.Interfaces.Repositories.Products;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Application.Interfaces.Services.Products;
using PlataformaECommerce.Application.Features.Products.Mappings;
using PlataformaECommerce.Domain.Entities.Products;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Application.Features.Products.Services;

/// <summary>
/// Proporciona los casos de uso de aplicación relacionados con la gestión de productos.
/// </summary>
/// <remarks>
/// Esta clase coordina la ejecución de operaciones de lectura y escritura
/// sobre el agregado de productos, actuando como servicio de aplicación.
///
/// Su responsabilidad incluye:
/// - validación de comandos y consultas,
/// - coordinación con repositorios,
/// - control de persistencia mediante unidad de trabajo,
/// - transformación de datos hacia DTOs,
/// - y orquestación de acciones de negocio sin invadir el dominio.
///
/// Este servicio constituye la implementación pública de los casos de uso del
/// módulo de productos, utilizando comandos y consultas como modelos de entrada
/// para orquestar decisiones de aplicación sin abrir una frontera paralela.
/// </remarks>
public sealed class ProductApplicationService : IProductApplicationService
{
    private const decimal MaxPromotionDiscountPercentage = 90m;

    #region Campos privados

    /// <summary>
    /// Repositorio de productos.
    /// </summary>
    private readonly IProductRepository _productRepository;

    /// <summary>
    /// Servicio transversal de auditoría.
    /// </summary>
    private readonly IAuditTrailService _auditTrailService;

    /// <summary>
    /// Unidad de trabajo asociada a la persistencia.
    /// </summary>
    private readonly IUnitOfWork _unitOfWork;

    private readonly IValidator<CreatePhysicalProductCommand> _createPhysicalProductCommandValidator;
    private readonly IValidator<CreateDigitalProductCommand> _createDigitalProductCommandValidator;
    private readonly IValidator<UpdateProductCommand> _updateProductCommandValidator;
    private readonly IValidator<UpdateProductStockCommand> _updateProductStockCommandValidator;

    #endregion

    #region Constructor

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="ProductApplicationService"/>.
    /// </summary>
    /// <param name="productRepository">Repositorio de productos.</param>
    /// <param name="auditTrailService">Servicio transversal de auditoría.</param>
    /// <param name="unitOfWork">Unidad de trabajo.</param>
    public ProductApplicationService(
        IProductRepository productRepository,
        IAuditTrailService auditTrailService,
        IUnitOfWork unitOfWork,
        IValidator<CreatePhysicalProductCommand> createPhysicalProductCommandValidator,
        IValidator<CreateDigitalProductCommand> createDigitalProductCommandValidator,
        IValidator<UpdateProductCommand> updateProductCommandValidator,
        IValidator<UpdateProductStockCommand> updateProductStockCommandValidator)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _auditTrailService = auditTrailService ?? throw new ArgumentNullException(nameof(auditTrailService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _createPhysicalProductCommandValidator = createPhysicalProductCommandValidator ?? throw new ArgumentNullException(nameof(createPhysicalProductCommandValidator));
        _createDigitalProductCommandValidator = createDigitalProductCommandValidator ?? throw new ArgumentNullException(nameof(createDigitalProductCommandValidator));
        _updateProductCommandValidator = updateProductCommandValidator ?? throw new ArgumentNullException(nameof(updateProductCommandValidator));
        _updateProductStockCommandValidator = updateProductStockCommandValidator ?? throw new ArgumentNullException(nameof(updateProductStockCommandValidator));
    }

    #endregion

    #region Casos de uso de creación

    /// <summary>
    /// Crea un nuevo producto físico dentro del sistema.
    /// </summary>
    /// <param name="command">Comando de creación del producto físico.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con el identificador del producto creado cuando la operación es exitosa.
    /// </returns>
    public async Task<Result<Guid>> CreatePhysicalProductAsync(
        CreatePhysicalProductCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = await ValidateAsync(command, _createPhysicalProductCommandValidator, cancellationToken);
        if (validationError is not null)
        {
            return Result.Failure<Guid>(validationError);
        }

        return await ExecuteAsync(async () =>
        {
            bool skuExists = await _productRepository.ExistsBySkuAsync(command.Sku, cancellationToken);
            if (skuExists)
            {
                return Result.Failure<Guid>(
                    Error.Conflict("Products.SkuAlreadyExists", $"Ya existe un producto registrado con el SKU '{command.Sku}'."));
            }

            ProductoFisico product = new(
                command.Name,
                command.Description,
                CreateSku(command.Sku),
                CreateMoney(command.Price, command.Currency),
                command.Stock,
                command.Slug,
                command.MainImageUrl,
                command.CategoryId,
                command.SubcategoryId,
                CreateTags(command.Tags),
                command.WeightKg,
                command.HeightCm,
                command.WidthCm,
                command.LengthCm,
                command.RequiresShipping);

            ApplyCommercialFlags(product, command.IsActive, command.IsFeatured);

            await _productRepository.AddAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await AuditProductEventAsync(
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

    /// <summary>
    /// Crea un nuevo producto digital dentro del sistema.
    /// </summary>
    /// <param name="command">Comando de creación del producto digital.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con el identificador del producto creado cuando la operación es exitosa.
    /// </returns>
    public async Task<Result<Guid>> CreateDigitalProductAsync(
        CreateDigitalProductCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = await ValidateAsync(command, _createDigitalProductCommandValidator, cancellationToken);
        if (validationError is not null)
        {
            return Result.Failure<Guid>(validationError);
        }

        return await ExecuteAsync(async () =>
        {
            bool skuExists = await _productRepository.ExistsBySkuAsync(command.Sku, cancellationToken);
            if (skuExists)
            {
                return Result.Failure<Guid>(
                    Error.Conflict("Products.SkuAlreadyExists", $"Ya existe un producto registrado con el SKU '{command.Sku}'."));
            }

            ProductoDigital product = new(
                command.Name,
                command.Description,
                CreateSku(command.Sku),
                CreateMoney(command.Price, command.Currency),
                command.Stock,
                command.Slug,
                command.MainImageUrl,
                command.CategoryId,
                null,
                CreateTags(command.Tags),
                command.FileFormat,
                command.FileSizeMb,
                command.RequiresLicense);

            ApplyCommercialFlags(product, command.IsActive, command.IsFeatured);

            await _productRepository.AddAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await AuditProductEventAsync(
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

    #endregion

    #region Casos de uso de actualización

    /// <summary>
    /// Actualiza la información integral de un producto existente.
    /// </summary>
    /// <param name="command">Comando de actualización del producto.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con la representación actualizada del producto cuando la operación es exitosa.
    /// </returns>
    public async Task<Result<ProductResponseDto>> UpdateProductAsync(
        UpdateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = await ValidateAsync(command, _updateProductCommandValidator, cancellationToken);
        if (validationError is not null)
        {
            return Result.Failure<ProductResponseDto>(validationError);
        }

        return await ExecuteAsync(async () =>
        {
            Producto? product = await FindProductByIdAsync(command.Id, cancellationToken);
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
                CreateSku(command.Sku),
                CreateMoney(command.Price, command.Currency),
                command.Slug,
                command.MainImageUrl);

            product.ActualizarStock(command.Stock);
            ApplyCommercialFlags(product, command.IsActive, command.IsFeatured);

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
            await AuditProductEventAsync(
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

            return Result.Success(product.ToProductResponseDto());
        }, "Products.Domain");
    }

    /// <summary>
    /// Actualiza el inventario de un producto existente.
    /// </summary>
    /// <param name="command">Comando de actualización de stock.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con la representación actualizada del producto cuando la operación es exitosa.
    /// </returns>
    public async Task<Result<ProductResponseDto>> UpdateProductStockAsync(
        UpdateProductStockCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = await ValidateAsync(command, _updateProductStockCommandValidator, cancellationToken);
        if (validationError is not null)
        {
            return Result.Failure<ProductResponseDto>(validationError);
        }

        return await ExecuteAsync(async () =>
        {
            Producto? product = await FindProductByIdAsync(command.ProductId, cancellationToken);
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
            await AuditProductEventAsync(
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

    /// <summary>
    /// Activa un producto existente dentro del sistema.
    /// </summary>
    /// <param name="command">Comando de activación del producto.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con la representación actualizada del producto cuando la operación es exitosa.
    /// </returns>
    public async Task<Result<ProductResponseDto>> ActivateProductAsync(
        ActivateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.ProductId == Guid.Empty)
        {
            return Result.Failure<ProductResponseDto>(
                Error.Validation("Products.InvalidId", "El identificador del producto es obligatorio."));
        }

        return await ExecuteAsync(async () =>
        {
            Producto? product = await FindProductByIdAsync(command.ProductId, cancellationToken);
            if (product is null)
            {
                return Result.Failure<ProductResponseDto>(
                    Error.NotFound("Products.NotFound", $"No se encontró un producto con identificador '{command.ProductId}'."));
            }

            product.Activar();

            await _productRepository.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await AuditProductEventAsync(
                product,
                "product.activated",
                $"Se activó el producto con SKU '{product.Sku.Value}'.",
                new Dictionary<string, string>
                {
                    ["isActive"] = product.Activo.ToString(),
                    ["sku"] = product.Sku.Value
                },
                cancellationToken);

            return Result.Success(product.ToProductResponseDto());
        }, "Products.Domain");
    }

    /// <summary>
    /// Desactiva un producto existente dentro del sistema.
    /// </summary>
    /// <param name="command">Comando de desactivación del producto.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con la representación actualizada del producto cuando la operación es exitosa.
    /// </returns>
    public async Task<Result<ProductResponseDto>> DeactivateProductAsync(
        DeactivateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.ProductId == Guid.Empty)
        {
            return Result.Failure<ProductResponseDto>(
                Error.Validation("Products.InvalidId", "El identificador del producto es obligatorio."));
        }

        return await ExecuteAsync(async () =>
        {
            Producto? product = await FindProductByIdAsync(command.ProductId, cancellationToken);
            if (product is null)
            {
                return Result.Failure<ProductResponseDto>(
                    Error.NotFound("Products.NotFound", $"No se encontró un producto con identificador '{command.ProductId}'."));
            }

            product.Desactivar();

            await _productRepository.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await AuditProductEventAsync(
                product,
                "product.deactivated",
                $"Se desactivó el producto con SKU '{product.Sku.Value}'.",
                new Dictionary<string, string>
                {
                    ["isActive"] = product.Activo.ToString(),
                    ["sku"] = product.Sku.Value
                },
                cancellationToken);

            return Result.Success(product.ToProductResponseDto());
        }, "Products.Domain");
    }

    /// <summary>
    /// Aplica una promoción porcentual sobre un producto disponible.
    /// </summary>
    /// <param name="command">Comando de promoción del producto.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con la representación actualizada del producto cuando la operación es exitosa.
    /// </returns>
    public async Task<Result<ProductResponseDto>> ApplyProductPromotionAsync(
        ApplyProductPromotionCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = ValidateProductId(command.ProductId)
            ?? ValidatePromotionPercentage(command.DiscountPercentage);

        if (validationError is not null)
        {
            return Result.Failure<ProductResponseDto>(validationError);
        }

        return await ExecuteAsync(async () =>
        {
            Producto? product = await FindProductByIdAsync(command.ProductId, cancellationToken);
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
            await AuditProductEventAsync(
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

    /// <summary>
    /// Retira una promoción activa y restaura el precio base del producto.
    /// </summary>
    /// <param name="command">Comando para retirar la promoción.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con la representación actualizada del producto cuando la operación es exitosa.
    /// </returns>
    public async Task<Result<ProductResponseDto>> RemoveProductPromotionAsync(
        RemoveProductPromotionCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = ValidateProductId(command.ProductId);
        if (validationError is not null)
        {
            return Result.Failure<ProductResponseDto>(validationError);
        }

        return await ExecuteAsync(async () =>
        {
            Producto? product = await FindProductByIdAsync(command.ProductId, cancellationToken);
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
            await AuditProductEventAsync(
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

    /// <summary>
    /// Marca un producto como destacado dentro del catálogo.
    /// </summary>
    /// <param name="command">Comando para destacar el producto.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con la representación actualizada del producto cuando la operación es exitosa.
    /// </returns>
    public async Task<Result<ProductResponseDto>> FeatureProductAsync(
        FeatureProductCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = ValidateProductId(command.ProductId);
        if (validationError is not null)
        {
            return Result.Failure<ProductResponseDto>(validationError);
        }

        return await ExecuteAsync(async () =>
        {
            Producto? product = await FindProductByIdAsync(command.ProductId, cancellationToken);
            if (product is null)
            {
                return Result.Failure<ProductResponseDto>(
                    Error.NotFound("Products.NotFound", $"No se encontró un producto con identificador '{command.ProductId}'."));
            }

            product.MarcarComoDestacado();

            await _productRepository.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await AuditProductEventAsync(
                product,
                "product.featured",
                $"Se marcó como destacado el producto con SKU '{product.Sku.Value}'.",
                new Dictionary<string, string>
                {
                    ["isFeatured"] = product.Destacado.ToString(),
                    ["sku"] = product.Sku.Value
                },
                cancellationToken);

            return Result.Success(product.ToProductResponseDto());
        }, "Products.Domain");
    }

    /// <summary>
    /// Retira la marca de destacado de un producto existente.
    /// </summary>
    /// <param name="command">Comando para retirar el destacado del producto.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con la representación actualizada del producto cuando la operación es exitosa.
    /// </returns>
    public async Task<Result<ProductResponseDto>> UnfeatureProductAsync(
        UnfeatureProductCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = ValidateProductId(command.ProductId);
        if (validationError is not null)
        {
            return Result.Failure<ProductResponseDto>(validationError);
        }

        return await ExecuteAsync(async () =>
        {
            Producto? product = await FindProductByIdAsync(command.ProductId, cancellationToken);
            if (product is null)
            {
                return Result.Failure<ProductResponseDto>(
                    Error.NotFound("Products.NotFound", $"No se encontró un producto con identificador '{command.ProductId}'."));
            }

            product.QuitarDestacado();

            await _productRepository.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await AuditProductEventAsync(
                product,
                "product.unfeatured",
                $"Se retiró la marca de destacado del producto con SKU '{product.Sku.Value}'.",
                new Dictionary<string, string>
                {
                    ["isFeatured"] = product.Destacado.ToString(),
                    ["sku"] = product.Sku.Value
                },
                cancellationToken);

            return Result.Success(product.ToProductResponseDto());
        }, "Products.Domain");
    }

    #endregion

    #region Casos de uso de consulta

    /// <summary>
    /// Obtiene el detalle de un producto por su identificador.
    /// </summary>
    /// <param name="query">Consulta de detalle del producto.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con el detalle del producto cuando la operación es exitosa.
    /// </returns>
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

        Producto? product = await FindProductByIdAsync(query.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure<ProductDetailDto>(
                Error.NotFound("Products.NotFound", $"No se encontró un producto con identificador '{query.ProductId}'."));
        }

        return Result.Success(product.ToProductDetailDto());
    }

    /// <summary>
    /// Obtiene un listado de productos aplicando filtros, ordenamiento y paginación.
    /// </summary>
    /// <param name="query">Consulta de listado de productos.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con la colección paginada de productos cuando la operación es exitosa.
    /// </returns>
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

        products = ApplySorting(products, query);

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

    #endregion

    #region Métodos privados de soporte

    /// <summary>
    /// Busca un producto por su identificador dentro del repositorio actual.
    /// </summary>
    /// <param name="productId">Identificador del producto.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Producto encontrado o <see langword="null"/> si no existe.</returns>
    private async Task<Producto?> FindProductByIdAsync(Guid productId, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Producto> products = await _productRepository.GetAllAsync(cancellationToken);
        return products.FirstOrDefault(product => product.Id == productId);
    }

    /// <summary>
    /// Aplica el estado comercial del producto según los indicadores solicitados.
    /// </summary>
    /// <param name="product">Producto a ajustar.</param>
    /// <param name="isActive">Indica si debe quedar activo.</param>
    /// <param name="isFeatured">Indica si debe quedar destacado.</param>
    private static void ApplyCommercialFlags(Producto product, bool isActive, bool isFeatured)
    {
        if (isActive)
        {
            product.Activar();
        }
        else
        {
            product.Desactivar();
        }

        if (isFeatured)
        {
            product.MarcarComoDestacado();
        }
        else
        {
            product.QuitarDestacado();
        }
    }

    /// <summary>
    /// Aplica el ordenamiento solicitado a la colección de productos.
    /// </summary>
    /// <param name="products">Colección base de productos.</param>
    /// <param name="query">Consulta que contiene las reglas de ordenamiento.</param>
    /// <returns>Colección ordenada.</returns>
    private static IEnumerable<Producto> ApplySorting(IEnumerable<Producto> products, GetProductsQuery query)
    {
        string sortBy = query.SortBy?.Trim().ToLowerInvariant() ?? "name";

        return (sortBy, query.SortDescending) switch
        {
            ("price", false) => products.OrderBy(product => product.Precio.Amount),
            ("price", true) => products.OrderByDescending(product => product.Precio.Amount),

            ("stock", false) => products.OrderBy(product => product.Stock),
            ("stock", true) => products.OrderByDescending(product => product.Stock),

            ("createdat", false) => products.OrderBy(product => product.FechaCreacionUtc),
            ("createdat", true) => products.OrderByDescending(product => product.FechaCreacionUtc),

            ("updatedat", false) => products.OrderBy(product => product.FechaActualizacionUtc),
            ("updatedat", true) => products.OrderByDescending(product => product.FechaActualizacionUtc),

            ("sku", false) => products.OrderBy(product => product.Sku.Value),
            ("sku", true) => products.OrderByDescending(product => product.Sku.Value),

            (_, false) => products.OrderBy(product => product.Nombre),
            (_, true) => products.OrderByDescending(product => product.Nombre)
        };
    }

    /// <summary>
    /// Construye un value object <see cref="Sku"/> a partir de un valor textual.
    /// </summary>
    /// <param name="value">Valor textual del SKU.</param>
    /// <returns>Instancia de <see cref="Sku"/>.</returns>
    private static Sku CreateSku(string value)
    {
        return new Sku(value);
    }

    /// <summary>
    /// Construye la colección de etiquetas de producto a partir de valores textuales.
    /// </summary>
    /// <param name="values">Valores textuales de las etiquetas.</param>
    /// <returns>Colección de etiquetas de dominio.</returns>
    private static IReadOnlyCollection<EtiquetaProducto> CreateTags(IEnumerable<string>? values)
    {
        if (values is null)
        {
            return Array.Empty<EtiquetaProducto>();
        }

        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => new EtiquetaProducto(value))
            .Distinct()
            .ToArray();
    }

    /// <summary>
    /// Construye un value object <see cref="Money"/> a partir de monto y moneda.
    /// </summary>
    /// <param name="amount">Monto monetario.</param>
    /// <param name="currency">Código de moneda.</param>
    /// <returns>Instancia de <see cref="Money"/>.</returns>
    private static Money CreateMoney(decimal amount, string currency)
    {
        return new Money(amount, currency);
    }

    /// <summary>
    /// Valida el identificador de un producto utilizado en operaciones administrativas.
    /// </summary>
    /// <param name="productId">Identificador del producto.</param>
    /// <returns>Error de validación o <see langword="null"/> cuando el identificador es válido.</returns>
    private static Error? ValidateProductId(Guid productId)
    {
        return productId == Guid.Empty
            ? Error.Validation("Products.InvalidId", "El identificador del producto es obligatorio.")
            : null;
    }

    /// <summary>
    /// Valida el porcentaje de descuento solicitado para una promoción.
    /// </summary>
    /// <param name="discountPercentage">Porcentaje de descuento solicitado.</param>
    /// <returns>Error de validación o <see langword="null"/> cuando el porcentaje es válido.</returns>
    private static Error? ValidatePromotionPercentage(decimal discountPercentage)
    {
        return discountPercentage <= 0m || discountPercentage > MaxPromotionDiscountPercentage
            ? Error.Validation(
                "Products.InvalidPromotionPercentage",
                $"El porcentaje de descuento debe ser mayor que cero y no superar el {MaxPromotionDiscountPercentage}%.")
            : null;
    }

    /// <summary>
    /// Obtiene el factor decimal aplicable al precio original a partir del porcentaje de descuento.
    /// </summary>
    /// <param name="discountPercentage">Porcentaje de descuento validado.</param>
    /// <returns>Factor decimal a aplicar sobre el precio actual.</returns>
    private static decimal ResolveDiscountFactor(decimal discountPercentage)
    {
        return decimal.Round(1m - (discountPercentage / 100m), 4, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Registra un evento de auditoría asociado a una operación exitosa sobre productos.
    /// </summary>
    /// <param name="product">Producto afectado por la operación.</param>
    /// <param name="action">Acción semántica auditada.</param>
    /// <param name="detail">Detalle legible del evento.</param>
    /// <param name="metadata">Metadatos complementarios del evento.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    private Task AuditProductEventAsync(
        Producto product,
        string action,
        string detail,
        IReadOnlyDictionary<string, string>? metadata,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(product);

        return _auditTrailService.RegisterAsync(
            product.Id,
            nameof(Producto),
            "Products",
            action,
            detail,
            metadata,
            cancellationToken);
    }

    private static Task<Error?> ValidateAsync<TCommand>(
        TCommand command,
        IValidator<TCommand> validator,
        CancellationToken cancellationToken)
    {
        return ApplicationExecution.ValidateAsync(
            command,
            validator,
            "Products.Validation",
            "La solicitud de producto contiene errores de validación.",
            cancellationToken);
    }

    private static Task<Result<TResponse>> ExecuteAsync<TResponse>(
        Func<Task<Result<TResponse>>> operation,
        string errorCode)
    {
        return ApplicationExecution.ExecuteAsync(operation, errorCode);
    }


    #endregion
}