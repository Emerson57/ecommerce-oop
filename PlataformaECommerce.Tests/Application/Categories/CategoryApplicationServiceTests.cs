using FluentValidation;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Categories.Commands;
using PlataformaECommerce.Application.Features.Categories.DTOs;
using PlataformaECommerce.Application.Features.Categories.Services;
using PlataformaECommerce.Application.Features.Categories.Validators;
using PlataformaECommerce.Application.Interfaces.Persistence;
using PlataformaECommerce.Application.Interfaces.Repositories.Categories;
using PlataformaECommerce.Application.Interfaces.Repositories.Products;
using PlataformaECommerce.Domain.Entities.Categories;
using PlataformaECommerce.Domain.Entities.Products;

namespace PlataformaECommerce.Tests.Application.Categories;

[TestFixture]
public class CategoryApplicationServiceTests
{
    [Test]
    public async Task ImportCategoriesFromXmlAsync_XmlValido_CreaCategoriasRaizYSubcategoriasEnUnaTransaccion()
    {
        FakeCategoryRepository categoryRepository = new();
        FakeUnitOfWork unitOfWork = new(categoryRepository);
        CategoryApplicationService service = CreateService(categoryRepository, unitOfWork);
        string xmlContent = """
                            <Categories>
                              <Category>
                                <Name>Tecnologia</Name>
                                <Slug>tecnologia</Slug>
                                <Description>Categoria principal.</Description>
                                <IsActive>true</IsActive>
                                <ParentCategoryName></ParentCategoryName>
                              </Category>
                              <Category>
                                <Name>Laptops</Name>
                                <Slug>laptops</Slug>
                                <Description>Equipos portatiles.</Description>
                                <IsActive>true</IsActive>
                                <ParentCategoryName>Tecnologia</ParentCategoryName>
                              </Category>
                            </Categories>
                            """;

        Result<CategoryImportResultDto> result = await service.ImportCategoriesFromXmlAsync(
            new ImportCategoriesFromXmlCommand { XmlContent = xmlContent },
            CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.RootCategoriesCreated, Is.EqualTo(1));
        Assert.That(result.Value.SubcategoriesCreated, Is.EqualTo(1));
        Assert.That(unitOfWork.BeginTransactionCalls, Is.EqualTo(1));
        Assert.That(unitOfWork.CommitTransactionCalls, Is.EqualTo(1));
        Assert.That(unitOfWork.SaveChangesCalls, Is.EqualTo(1));
        Assert.That(categoryRepository.AddedCategories.Count, Is.EqualTo(2));
        Assert.That(categoryRepository.AddedCategories.Single(category => category.Nombre == "Laptops").ParentCategoryId, Is.EqualTo(categoryRepository.AddedCategories.Single(category => category.Nombre == "Tecnologia").Id));
    }

    [Test]
    public async Task ImportCategoriesFromXmlAsync_SlugDuplicadoEnArchivo_RetornaErrorYNoPersisteCambios()
    {
        FakeCategoryRepository categoryRepository = new();
        FakeUnitOfWork unitOfWork = new(categoryRepository);
        CategoryApplicationService service = CreateService(categoryRepository, unitOfWork);
        string xmlContent = """
                            <Categories>
                              <Category>
                                <Name>Tecnologia</Name>
                                <Slug>tecnologia</Slug>
                                <IsActive>true</IsActive>
                                <ParentCategoryName></ParentCategoryName>
                              </Category>
                              <Category>
                                <Name>Audio</Name>
                                <Slug>tecnologia</Slug>
                                <IsActive>true</IsActive>
                                <ParentCategoryName>Tecnologia</ParentCategoryName>
                              </Category>
                            </Categories>
                            """;

        Result<CategoryImportResultDto> result = await service.ImportCategoriesFromXmlAsync(
            new ImportCategoriesFromXmlCommand { XmlContent = xmlContent },
            CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Message, Is.EqualTo("El slug 'tecnologia' está repetido dentro del archivo XML."));
        Assert.That(unitOfWork.BeginTransactionCalls, Is.EqualTo(0));
        Assert.That(categoryRepository.AddedCategories, Is.Empty);
    }

    [Test]
    public async Task ImportCategoriesFromXmlAsync_ParentCategoryNameInvalido_RetornaErrorYNoPersisteCambios()
    {
        FakeCategoryRepository categoryRepository = new();
        FakeUnitOfWork unitOfWork = new(categoryRepository);
        CategoryApplicationService service = CreateService(categoryRepository, unitOfWork);
        string xmlContent = """
                            <Categories>
                              <Category>
                                <Name>Laptops</Name>
                                <Slug>laptops</Slug>
                                <Description>Equipos portatiles.</Description>
                                <IsActive>true</IsActive>
                                <ParentCategoryName>Tecnologia</ParentCategoryName>
                              </Category>
                            </Categories>
                            """;

        Result<CategoryImportResultDto> result = await service.ImportCategoriesFromXmlAsync(
            new ImportCategoriesFromXmlCommand { XmlContent = xmlContent },
            CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Message, Is.EqualTo("No se encontró una categoría raíz con el nombre 'Tecnologia' para resolver 'ParentCategoryName'."));
        Assert.That(unitOfWork.BeginTransactionCalls, Is.EqualTo(1));
        Assert.That(unitOfWork.RollbackTransactionCalls, Is.EqualTo(1));
        Assert.That(categoryRepository.AddedCategories, Is.Empty);
    }

    private static CategoryApplicationService CreateService(FakeCategoryRepository categoryRepository, FakeUnitOfWork unitOfWork)
    {
        IValidator<CreateCategoryCommand> createValidator = new CreateCategoryCommandValidator();
        IValidator<ImportCategoriesFromXmlCommand> importValidator = new ImportCategoriesFromXmlCommandValidator();
        IValidator<UpdateCategoryCommand> updateValidator = new UpdateCategoryCommandValidator();
        return new CategoryApplicationService(categoryRepository, new FakeProductRepository(), unitOfWork, createValidator, importValidator, updateValidator);
    }

    private sealed class FakeCategoryRepository : ICategoryRepository
    {
        public List<CategoriaProducto> ExistingCategories { get; } = [];
        public List<CategoriaProducto> AddedCategories { get; } = [];

        public Task<IReadOnlyCollection<CategoriaProducto>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<CategoriaProducto>>(ExistingCategories.ToArray());

        public Task<CategoriaProducto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(ExistingCategories.Concat(AddedCategories).FirstOrDefault(category => category.Id == id));

        public Task<IReadOnlyCollection<CategoriaProducto>> GetByParentCategoryIdAsync(Guid? parentCategoryId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<CategoriaProducto>>(ExistingCategories.Where(category => category.ParentCategoryId == parentCategoryId).ToArray());

        public Task<bool> ExistsBySlugAsync(string slug, Guid? excludedCategoryId = null, CancellationToken cancellationToken = default)
            => Task.FromResult(ExistingCategories.Any(category => string.Equals(category.Slug, slug, StringComparison.OrdinalIgnoreCase)));

        public Task AddAsync(CategoriaProducto categoria, CancellationToken cancellationToken = default)
        {
            AddedCategories.Add(categoria);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(CategoriaProducto categoria, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeProductRepository : IProductRepository
    {
        public Task<IReadOnlyCollection<Producto>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Producto>>(Array.Empty<Producto>());

        public Task<Producto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<Producto?>(null);

        public Task<Producto?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
            => Task.FromResult<Producto?>(null);

        public Task<IReadOnlyCollection<Producto>> GetActiveProductsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Producto>>(Array.Empty<Producto>());

        public Task<IReadOnlyCollection<Producto>> GetFeaturedProductsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Producto>>(Array.Empty<Producto>());

        public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<bool> ExistsBySkuAsync(string sku, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task AddAsync(Producto producto, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task UpdateAsync(Producto producto, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeUnitOfWork(FakeCategoryRepository? categoryRepository = null) : IUnitOfWork
    {
        public int SaveChangesCalls { get; private set; }
        public int BeginTransactionCalls { get; private set; }
        public int CommitTransactionCalls { get; private set; }
        public int RollbackTransactionCalls { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCalls++;
            return Task.FromResult(1);
        }

        public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            BeginTransactionCalls++;
            return Task.CompletedTask;
        }

        public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            CommitTransactionCalls++;
            return Task.CompletedTask;
        }

        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            RollbackTransactionCalls++;
            categoryRepository?.AddedCategories.Clear();
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
            => ValueTask.CompletedTask;
    }
}
