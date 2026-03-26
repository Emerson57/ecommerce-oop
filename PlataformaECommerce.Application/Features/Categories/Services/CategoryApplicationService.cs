using FluentValidation;
using PlataformaECommerce.Application.Common.Execution;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Categories.Commands;
using PlataformaECommerce.Application.Features.Categories.DTOs;
using PlataformaECommerce.Application.Features.Categories.Importing;
using PlataformaECommerce.Application.Features.Categories.Mappings;
using PlataformaECommerce.Application.Features.Categories.Queries;
using PlataformaECommerce.Application.Interfaces.Persistence;
using PlataformaECommerce.Application.Interfaces.Repositories.Categories;
using PlataformaECommerce.Application.Interfaces.Repositories.Products;
using PlataformaECommerce.Application.Interfaces.Services.Categories;
using PlataformaECommerce.Domain.Entities.Categories;

namespace PlataformaECommerce.Application.Features.Categories.Services;

/// <summary>
/// Orquesta los casos de uso del módulo de categorías del catálogo.
/// </summary>
public sealed class CategoryApplicationService : ICategoryApplicationService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateCategoryCommand> _createCategoryCommandValidator;
    private readonly IValidator<ImportCategoriesFromXmlCommand> _importCategoriesFromXmlCommandValidator;
    private readonly IValidator<UpdateCategoryCommand> _updateCategoryCommandValidator;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="CategoryApplicationService"/>.
    /// </summary>
    public CategoryApplicationService(
        ICategoryRepository categoryRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateCategoryCommand> createCategoryCommandValidator,
        IValidator<ImportCategoriesFromXmlCommand> importCategoriesFromXmlCommandValidator,
        IValidator<UpdateCategoryCommand> updateCategoryCommandValidator)
    {
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _createCategoryCommandValidator = createCategoryCommandValidator ?? throw new ArgumentNullException(nameof(createCategoryCommandValidator));
        _importCategoriesFromXmlCommandValidator = importCategoriesFromXmlCommandValidator ?? throw new ArgumentNullException(nameof(importCategoriesFromXmlCommandValidator));
        _updateCategoryCommandValidator = updateCategoryCommandValidator ?? throw new ArgumentNullException(nameof(updateCategoryCommandValidator));
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyCollection<CategoryDto>>> GetCategoriesAsync(GetCategoriesQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        IReadOnlyCollection<CategoriaProducto> categories = query.ParentCategoryId.HasValue || query.RootOnly
            ? await _categoryRepository.GetByParentCategoryIdAsync(query.RootOnly ? null : query.ParentCategoryId, cancellationToken)
            : await _categoryRepository.GetAllAsync(cancellationToken);

        IEnumerable<CategoriaProducto> filteredCategories = categories;

        if (query.OnlyActive)
        {
            filteredCategories = filteredCategories.Where(category => category.Activa);
        }

        CategoryDto[] items = filteredCategories
            .OrderBy(category => category.ParentCategoryId.HasValue)
            .ThenBy(category => category.Nombre)
            .Select(category => category.ToCategoryDto())
            .ToArray();

        return Result.Success<IReadOnlyCollection<CategoryDto>>(items);
    }

    /// <inheritdoc />
    public async Task<Result<CategoryDto>> GetCategoryByIdAsync(GetCategoryByIdQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.CategoryId == Guid.Empty)
        {
            return Result.Failure<CategoryDto>(Error.Validation("Categories.InvalidId", "El identificador de la categoría es obligatorio."));
        }

        CategoriaProducto? category = await _categoryRepository.GetByIdAsync(query.CategoryId, cancellationToken);
        if (category is null)
        {
            return Result.Failure<CategoryDto>(Error.NotFound("Categories.NotFound", $"No se encontró una categoría con identificador '{query.CategoryId}'."));
        }

        return Result.Success(category.ToCategoryDto());
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> CreateCategoryAsync(CreateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = await ValidateAsync(command, _createCategoryCommandValidator, cancellationToken);
        if (validationError is not null)
        {
            return Result.Failure<Guid>(validationError);
        }

        return await ApplicationExecution.ExecuteAsync(async () =>
        {
            Error? parentValidationError = await ValidateParentCategoryAsync(command.ParentCategoryId, null, cancellationToken);
            if (parentValidationError is not null)
            {
                return Result.Failure<Guid>(parentValidationError);
            }

            bool slugExists = await _categoryRepository.ExistsBySlugAsync(command.Slug, cancellationToken: cancellationToken);
            if (slugExists)
            {
                return Result.Failure<Guid>(Error.Conflict("Categories.SlugAlreadyExists", $"Ya existe una categoría registrada con el slug '{command.Slug}'."));
            }

            CategoriaProducto category = CreateCategory(command.Name, command.Slug, command.Description, command.ParentCategoryId, command.IsActive);

            await _categoryRepository.AddAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(category.Id);
        }, "Categories.Domain");
    }

    /// <inheritdoc />
    public async Task<Result<CategoryImportResultDto>> ImportCategoriesFromXmlAsync(ImportCategoriesFromXmlCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = await ValidateAsync(command, _importCategoriesFromXmlCommandValidator, cancellationToken);
        if (validationError is not null)
        {
            return Result.Failure<CategoryImportResultDto>(validationError);
        }

        Result<IReadOnlyCollection<CategoryXmlImportParser.ImportedCategoryDefinition>> parsingResult = CategoryXmlImportParser.Parse(command.XmlContent);
        if (parsingResult.IsFailure)
        {
            return Result.Failure<CategoryImportResultDto>(parsingResult.Error);
        }

        return await ApplicationExecution.ExecuteAsync(async () =>
        {
            IReadOnlyCollection<CategoriaProducto> existingCategories = await _categoryRepository.GetAllAsync(cancellationToken);
            HashSet<string> existingSlugs = existingCategories
                .Select(category => category.Slug)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            string? conflictingSlug = parsingResult.Value
                .Select(category => category.Slug)
                .FirstOrDefault(existingSlugs.Contains);

            if (!string.IsNullOrWhiteSpace(conflictingSlug))
            {
                return Result.Failure<CategoryImportResultDto>(
                    Error.Conflict("Categories.ImportSlugAlreadyExists", $"Ya existe una categoría registrada con el slug '{conflictingSlug}'."));
            }

            IReadOnlyCollection<CategoryXmlImportParser.ImportedCategoryDefinition> importedRootCategories = parsingResult.Value
                .Where(category => string.IsNullOrWhiteSpace(category.ParentCategoryName))
                .ToArray();

            IReadOnlyCollection<CategoryXmlImportParser.ImportedCategoryDefinition> importedSubcategories = parsingResult.Value
                .Where(category => !string.IsNullOrWhiteSpace(category.ParentCategoryName))
                .ToArray();

            string? duplicatedImportedRootName = importedRootCategories
                .GroupBy(category => category.Name, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(duplicatedImportedRootName))
            {
                return Result.Failure<CategoryImportResultDto>(
                    Error.Validation("Categories.ImportDuplicatedRootName", $"El nombre de categoría raíz '{duplicatedImportedRootName}' está repetido dentro del archivo XML."));
            }

            int rootCategoriesCreated = 0;
            int subcategoriesCreated = 0;
            Dictionary<string, CategoriaProducto> importedRootsByName = new(StringComparer.OrdinalIgnoreCase);

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                foreach (CategoryXmlImportParser.ImportedCategoryDefinition importedCategory in importedRootCategories)
                {
                    CategoriaProducto rootCategory = CreateCategory(importedCategory.Name, importedCategory.Slug, importedCategory.Description, null, importedCategory.IsActive);
                    await _categoryRepository.AddAsync(rootCategory, cancellationToken);
                    importedRootsByName[rootCategory.Nombre] = rootCategory;
                    rootCategoriesCreated++;
                }

                foreach (CategoryXmlImportParser.ImportedCategoryDefinition importedSubcategory in importedSubcategories)
                {
                    Result<Guid> parentResolutionResult = ResolveParentCategoryId(importedSubcategory.ParentCategoryName!, importedRootsByName, existingCategories);
                    if (parentResolutionResult.IsFailure)
                    {
                        await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                        return Result.Failure<CategoryImportResultDto>(parentResolutionResult.Error);
                    }

                    CategoriaProducto subcategory = CreateCategory(importedSubcategory.Name, importedSubcategory.Slug, importedSubcategory.Description, parentResolutionResult.Value, importedSubcategory.IsActive);
                    await _categoryRepository.AddAsync(subcategory, cancellationToken);
                    subcategoriesCreated++;
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }

            return Result.Success(new CategoryImportResultDto
            {
                RootCategoriesCreated = rootCategoriesCreated,
                SubcategoriesCreated = subcategoriesCreated
            });
        }, "Categories.Domain");
    }

    /// <inheritdoc />
    public async Task<Result<CategoryDto>> UpdateCategoryAsync(UpdateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = await ValidateAsync(command, _updateCategoryCommandValidator, cancellationToken);
        if (validationError is not null)
        {
            return Result.Failure<CategoryDto>(validationError);
        }

        return await ApplicationExecution.ExecuteAsync(async () =>
        {
            CategoriaProducto? category = await _categoryRepository.GetByIdAsync(command.Id, cancellationToken);
            if (category is null)
            {
                return Result.Failure<CategoryDto>(Error.NotFound("Categories.NotFound", $"No se encontró una categoría con identificador '{command.Id}'."));
            }

            Error? parentValidationError = await ValidateParentCategoryAsync(command.ParentCategoryId, command.Id, cancellationToken);
            if (parentValidationError is not null)
            {
                return Result.Failure<CategoryDto>(parentValidationError);
            }

            bool slugExists = await _categoryRepository.ExistsBySlugAsync(command.Slug, command.Id, cancellationToken);
            if (slugExists)
            {
                return Result.Failure<CategoryDto>(Error.Conflict("Categories.SlugAlreadyExists", $"Ya existe una categoría registrada con el slug '{command.Slug}'."));
            }

            category.ActualizarInformacionBasica(command.Name, command.Slug, command.Description);

            if (command.ParentCategoryId.HasValue)
            {
                category.ReasignarPadre(command.ParentCategoryId.Value);
            }
            else
            {
                category.ConvertirEnCategoriaRaiz();
            }

            Error? statusError = await ApplyCategoryStatusAsync(category, command.IsActive, cancellationToken);
            if (statusError is not null)
            {
                return Result.Failure<CategoryDto>(statusError);
            }

            await _categoryRepository.UpdateAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(category.ToCategoryDto());
        }, "Categories.Domain");
    }

    /// <inheritdoc />
    public async Task<Result<CategoryDto>> ChangeCategoryStatusAsync(ChangeCategoryStatusCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.CategoryId == Guid.Empty)
        {
            return Result.Failure<CategoryDto>(Error.Validation("Categories.InvalidId", "El identificador de la categoría es obligatorio."));
        }

        return await ApplicationExecution.ExecuteAsync(async () =>
        {
            CategoriaProducto? category = await _categoryRepository.GetByIdAsync(command.CategoryId, cancellationToken);
            if (category is null)
            {
                return Result.Failure<CategoryDto>(Error.NotFound("Categories.NotFound", $"No se encontró una categoría con identificador '{command.CategoryId}'."));
            }

            Error? statusError = await ApplyCategoryStatusAsync(category, command.IsActive, cancellationToken);
            if (statusError is not null)
            {
                return Result.Failure<CategoryDto>(statusError);
            }

            await _categoryRepository.UpdateAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(category.ToCategoryDto());
        }, "Categories.Domain");
    }

    private async Task<Error?> ValidateParentCategoryAsync(Guid? parentCategoryId, Guid? currentCategoryId, CancellationToken cancellationToken)
    {
        if (!parentCategoryId.HasValue)
        {
            return null;
        }

        if (currentCategoryId.HasValue && parentCategoryId.Value == currentCategoryId.Value)
        {
            return Error.Validation("Categories.InvalidParent", "Una categoría no puede depender de sí misma.");
        }

        CategoriaProducto? parentCategory = await _categoryRepository.GetByIdAsync(parentCategoryId.Value, cancellationToken);
        if (parentCategory is null)
        {
            return Error.Validation("Categories.ParentNotFound", "La categoría padre indicada no existe.");
        }

        if (!parentCategory.EsCategoriaRaiz)
        {
            return Error.Validation("Categories.InvalidParent", "En el MVP una subcategoría solo puede depender de una categoría principal.");
        }

        return null;
    }

    private async Task<Error?> ApplyCategoryStatusAsync(CategoriaProducto category, bool shouldBeActive, CancellationToken cancellationToken)
    {
        if (shouldBeActive)
        {
            category.Activar();
            return null;
        }

        IReadOnlyCollection<CategoriaProducto> childCategories = await _categoryRepository.GetByParentCategoryIdAsync(category.Id, cancellationToken);
        if (childCategories.Any(child => child.Activa))
        {
            return Error.Validation("Categories.HasActiveChildren", "No es posible desactivar una categoría que aún tiene subcategorías activas.");
        }

        IReadOnlyCollection<PlataformaECommerce.Domain.Entities.Products.Producto> products = await _productRepository.GetAllAsync(cancellationToken);
        bool hasAssignedProducts = products.Any(product => product.CategoriaId == category.Id || product.SubcategoriaId == category.Id);
        if (hasAssignedProducts)
        {
            return Error.Validation("Categories.HasAssignedProducts", "No es posible desactivar una categoría que todavía está asignada a productos del catálogo.");
        }

        category.Desactivar();
        return null;
    }

    private static CategoriaProducto CreateCategory(string name, string slug, string? description, Guid? parentCategoryId, bool isActive)
    {
        CategoriaProducto category = new(name, slug, description, parentCategoryId);
        if (isActive)
        {
            category.Activar();
        }

        return category;
    }

    private static Result<Guid> ResolveParentCategoryId(
        string parentCategoryName,
        IReadOnlyDictionary<string, CategoriaProducto> importedRootsByName,
        IReadOnlyCollection<CategoriaProducto> existingCategories)
    {
        if (importedRootsByName.TryGetValue(parentCategoryName, out CategoriaProducto? importedRootCategory))
        {
            return Result.Success(importedRootCategory.Id);
        }

        CategoriaProducto[] existingRootMatches = existingCategories
            .Where(category => category.EsCategoriaRaiz && string.Equals(category.Nombre, parentCategoryName, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (existingRootMatches.Length == 1)
        {
            return Result.Success(existingRootMatches[0].Id);
        }

        if (existingRootMatches.Length > 1)
        {
            return Result.Failure<Guid>(
                Error.Validation("Categories.ImportParentAmbiguous", $"El nombre de categoría padre '{parentCategoryName}' coincide con múltiples categorías raíz existentes."));
        }

        return Result.Failure<Guid>(
            Error.Validation("Categories.ImportParentNotFound", $"No se encontró una categoría raíz con el nombre '{parentCategoryName}' para resolver 'ParentCategoryName'."));
    }

    private static async Task<Error?> ValidateAsync<TCommand>(TCommand command, IValidator<TCommand> validator, CancellationToken cancellationToken)
    {
        FluentValidation.Results.ValidationResult validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (validationResult.IsValid)
        {
            return null;
        }

        FluentValidation.Results.ValidationFailure failure = validationResult.Errors[0];
        return Error.Validation($"Categories.{failure.PropertyName}", failure.ErrorMessage);
    }
}
