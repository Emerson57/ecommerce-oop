using FluentValidation;
using PlataformaECommerce.Application.Common.Execution;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Products.Commands;
using PlataformaECommerce.Application.Features.Products.Queries;
using PlataformaECommerce.Application.Interfaces.Repositories.Categories;
using PlataformaECommerce.Application.Interfaces.Services.Audit;
using PlataformaECommerce.Domain.Entities.Products;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Application.Features.Products.Services;

internal static class ProductServiceSupport
{
    internal const decimal MaxPromotionDiscountPercentage = 90m;

    internal static Task<Error?> ValidateAsync<TCommand>(
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

    internal static Task<Result<TResponse>> ExecuteAsync<TResponse>(
        Func<Task<Result<TResponse>>> operation,
        string errorCode)
    {
        return ApplicationExecution.ExecuteAsync(operation, errorCode);
    }

    internal static Task AuditProductEventAsync(
        IAuditTrailService auditTrailService,
        Producto product,
        string action,
        string detail,
        IReadOnlyDictionary<string, string>? metadata,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(auditTrailService);
        ArgumentNullException.ThrowIfNull(product);

        return auditTrailService.RegisterAsync(
            product.Id,
            nameof(Producto),
            "Products",
            action,
            detail,
            metadata,
            cancellationToken);
    }

    internal static void ApplyCommercialFlags(Producto product, bool isActive, bool isFeatured)
    {
        ArgumentNullException.ThrowIfNull(product);

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

    internal static Result<(Guid CategoryId, Guid? SubcategoryId)> ResolveCategoryAssignment(
        ImportProductRowCommand row,
        IReadOnlyCollection<PlataformaECommerce.Domain.Entities.Categories.CategoriaProducto> categories)
    {
        PlataformaECommerce.Domain.Entities.Categories.CategoriaProducto[] mainCategoryMatches = categories
            .Where(category => category.EsCategoriaRaiz
                && category.Activa
                && category.Nombre.Equals(row.CategoryName, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (mainCategoryMatches.Length == 0)
        {
            return Result.Failure<(Guid CategoryId, Guid? SubcategoryId)>(
                Error.Validation("Products.ImportCategoryNotFound", $"La fila {row.RowNumber} referencia una categoría principal inexistente o inactiva: '{row.CategoryName}'."));
        }

        if (mainCategoryMatches.Length > 1)
        {
            return Result.Failure<(Guid CategoryId, Guid? SubcategoryId)>(
                Error.Validation("Products.ImportCategoryAmbiguous", $"La fila {row.RowNumber} referencia una categoría principal ambigua: '{row.CategoryName}'."));
        }

        PlataformaECommerce.Domain.Entities.Categories.CategoriaProducto mainCategory = mainCategoryMatches[0];

        if (string.IsNullOrWhiteSpace(row.SubcategoryName))
        {
            return Result.Success((mainCategory.Id, (Guid?)null));
        }

        PlataformaECommerce.Domain.Entities.Categories.CategoriaProducto[] subcategoryMatches = categories
            .Where(category => category.EsSubcategoria
                && category.Activa
                && category.ParentCategoryId == mainCategory.Id
                && category.Nombre.Equals(row.SubcategoryName, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (subcategoryMatches.Length == 0)
        {
            return Result.Failure<(Guid CategoryId, Guid? SubcategoryId)>(
                Error.Validation("Products.ImportSubcategoryNotFound", $"La fila {row.RowNumber} referencia una subcategoría inexistente o inactiva para la categoría '{row.CategoryName}': '{row.SubcategoryName}'."));
        }

        if (subcategoryMatches.Length > 1)
        {
            return Result.Failure<(Guid CategoryId, Guid? SubcategoryId)>(
                Error.Validation("Products.ImportSubcategoryAmbiguous", $"La fila {row.RowNumber} referencia una subcategoría ambigua para la categoría '{row.CategoryName}': '{row.SubcategoryName}'."));
        }

        return Result.Success((mainCategory.Id, (Guid?)subcategoryMatches[0].Id));
    }

    internal static IEnumerable<Producto> ApplySorting(IEnumerable<Producto> products, GetProductsQuery query)
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

    internal static ProductoFisico CreatePhysicalProduct(CreatePhysicalProductCommand command)
    {
        return new ProductoFisico(
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
            command.RequiresShipping,
            command.ImageGallery);
    }

    internal static ProductoDigital CreateDigitalProduct(CreateDigitalProductCommand command)
    {
        return new ProductoDigital(
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
            command.FileFormat,
            command.FileSizeMb,
            command.RequiresLicense,
            command.ImageGallery);
    }

    internal static IReadOnlyCollection<EtiquetaProducto> CreateTags(IEnumerable<string>? values)
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

    internal static IReadOnlyCollection<string> ParseSerializedTags(string? serializedTags)
    {
        if (string.IsNullOrWhiteSpace(serializedTags))
        {
            return Array.Empty<string>();
        }

        return serializedTags
            .Split([',', ';', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    internal static Money CreateMoney(decimal amount, string currency)
    {
        return new Money(amount, currency);
    }

    internal static Error? ValidateProductId(Guid productId)
    {
        return productId == Guid.Empty
            ? Error.Validation("Products.InvalidId", "El identificador del producto es obligatorio.")
            : null;
    }

    internal static Error? ValidatePromotionPercentage(decimal discountPercentage)
    {
        return discountPercentage <= 0m || discountPercentage > MaxPromotionDiscountPercentage
            ? Error.Validation(
                "Products.InvalidPromotionPercentage",
                $"El porcentaje de descuento debe ser mayor que cero y no superar el {MaxPromotionDiscountPercentage}%.")
            : null;
    }

    internal static Error WrapImportRowError(int rowNumber, Error error)
    {
        return Error.Validation(error.Code, $"Fila {rowNumber}: {error.Message}");
    }

    internal static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    internal static async Task<Error?> ValidateCategoryAssignmentAsync(
        ICategoryRepository categoryRepository,
        Guid? categoryId,
        Guid? subcategoryId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(categoryRepository);

        if (!categoryId.HasValue || categoryId.Value == Guid.Empty)
        {
            return Error.Validation("Products.CategoryRequired", "La categoría principal del producto es obligatoria.");
        }

        var category = await categoryRepository.GetByIdAsync(categoryId.Value, cancellationToken);
        if (category is null)
        {
            return Error.Validation("Products.CategoryNotFound", "La categoría principal indicada no existe.");
        }

        if (!category.Activa)
        {
            return Error.Validation("Products.CategoryInactive", "La categoría principal indicada no se encuentra activa.");
        }

        if (!category.EsCategoriaRaiz)
        {
            return Error.Validation("Products.InvalidCategory", "En el MVP el producto debe pertenecer a una categoría principal raíz.");
        }

        if (!subcategoryId.HasValue)
        {
            return null;
        }

        if (subcategoryId.Value == Guid.Empty)
        {
            return Error.Validation("Products.InvalidSubcategory", "La subcategoría del producto no puede ser un identificador vacío.");
        }

        var subcategory = await categoryRepository.GetByIdAsync(subcategoryId.Value, cancellationToken);
        if (subcategory is null)
        {
            return Error.Validation("Products.SubcategoryNotFound", "La subcategoría indicada no existe.");
        }

        if (!subcategory.Activa)
        {
            return Error.Validation("Products.SubcategoryInactive", "La subcategoría indicada no se encuentra activa.");
        }

        if (!subcategory.EsSubcategoria || subcategory.ParentCategoryId != category.Id)
        {
            return Error.Validation("Products.InvalidSubcategory", "La subcategoría indicada no pertenece a la categoría principal seleccionada.");
        }

        return null;
    }

    private static Sku CreateSku(string value)
    {
        return new Sku(value);
    }
}
