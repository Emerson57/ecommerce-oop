using Microsoft.Extensions.Logging.Abstractions;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Common.SaaS;
using PlataformaECommerce.Application.Features.Categories.Commands;
using PlataformaECommerce.Application.Features.Categories.DTOs;
using PlataformaECommerce.Application.Features.Categories.Queries;
using PlataformaECommerce.Application.Features.Products.Commands;
using PlataformaECommerce.Application.Features.Products.DTOs;
using PlataformaECommerce.Application.Features.Products.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Categories;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Application.Interfaces.Services.Products;
using PlataformaECommerce.Web.Initialization;

namespace PlataformaECommerce.Tests.Web.Initialization;

[TestFixture]
public class ConfiguredTenantProvisioningServiceTests
{
    [Test]
    public async Task SynchronizeConfiguredCatalogAsync_DelegatesToProvisioningService()
    {
        FakeTenantCatalogProvisioningService tenantCatalogProvisioningService = new();
        ConfiguredTenantProvisioningService service = CreateService(
            tenantCatalogProvisioningService,
            new FakeTenantCatalogService(CreateTenantDefinition(seedBaseCategories: false, seedDemoCatalog: false)),
            new FakeTenantContextAccessor("platform-default"),
            new FakeCategoryApplicationService(),
            new FakeProductQueryService(),
            new FakeProductCommandService());

        await service.SynchronizeConfiguredCatalogAsync(CancellationToken.None);

        Assert.That(tenantCatalogProvisioningService.SynchronizeCalls, Is.EqualTo(1));
    }

    [Test]
    public async Task ProvisionConfiguredTenantsAsync_CategoriasBasePendientes_ImportaPlantillaYMarcaProvision()
    {
        FakeTenantCatalogProvisioningService tenantCatalogProvisioningService = new();
        FakeTenantCatalogService tenantCatalogService = new(
            CreateTenantDefinition(seedBaseCategories: true, seedDemoCatalog: false));
        FakeTenantContextAccessor tenantContextAccessor = new("platform-default");
        FakeCategoryApplicationService categoryApplicationService = new()
        {
            CategoriesResult = Result.Success<IReadOnlyCollection<CategoryDto>>(Array.Empty<CategoryDto>()),
            ImportResult = Result.Success(new CategoryImportResultDto
            {
                RootCategoriesCreated = 2,
                SubcategoriesCreated = 3
            })
        };
        ConfiguredTenantProvisioningService service = CreateService(
            tenantCatalogProvisioningService,
            tenantCatalogService,
            tenantContextAccessor,
            categoryApplicationService,
            new FakeProductQueryService(),
            new FakeProductCommandService());

        await service.ProvisionConfiguredTenantsAsync(CancellationToken.None);

        Assert.That(categoryApplicationService.ImportCalls, Is.EqualTo(1));
        Assert.That(categoryApplicationService.LastImportCommand?.XmlContent, Does.Contain("Tecnologia"));
        Assert.That(tenantCatalogProvisioningService.MarkBaseCategoriesCalls, Is.EqualTo(1));
    }

    [Test]
    public async Task ProvisionConfiguredTenantsAsync_CatalogoDemoPendiente_ImportaProductosYMarcaProvision()
    {
        FakeTenantCatalogProvisioningService tenantCatalogProvisioningService = new();
        FakeTenantCatalogService tenantCatalogService = new(
            CreateTenantDefinition(seedBaseCategories: false, seedDemoCatalog: true));
        FakeTenantContextAccessor tenantContextAccessor = new("platform-default");
        FakeCategoryApplicationService categoryApplicationService = new()
        {
            CategoriesResult = Result.Success<IReadOnlyCollection<CategoryDto>>(
            [
                new CategoryDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Tecnologia",
                    Slug = "tecnologia",
                    IsActive = true,
                    IsRootCategory = true
                }
            ])
        };
        FakeProductQueryService productQueryService = new()
        {
            QueryResult = Result.Success(new ProductQueryResultDto
            {
                Items = Array.Empty<ProductDto>(),
                TotalCount = 0,
                ReturnedCount = 0,
                PageNumber = 1,
                PageSize = 1,
                TotalPages = 0,
                HasPreviousPage = false,
                HasNextPage = false
            })
        };
        FakeProductCommandService productCommandService = new()
        {
            ImportResult = Result.Success(new ProductImportResultDto
            {
                PhysicalProductsCreated = 1,
                DigitalProductsCreated = 1
            })
        };
        ConfiguredTenantProvisioningService service = CreateService(
            tenantCatalogProvisioningService,
            tenantCatalogService,
            tenantContextAccessor,
            categoryApplicationService,
            productQueryService,
            productCommandService);

        await service.ProvisionConfiguredTenantsAsync(CancellationToken.None);

        Assert.That(productCommandService.ImportCalls, Is.EqualTo(1));
        Assert.That(productCommandService.LastImportCommand?.Rows.Count, Is.EqualTo(2));
        Assert.That(tenantCatalogProvisioningService.MarkDemoCatalogCalls, Is.EqualTo(1));
    }

    private static ConfiguredTenantProvisioningService CreateService(
        FakeTenantCatalogProvisioningService tenantCatalogProvisioningService,
        FakeTenantCatalogService tenantCatalogService,
        FakeTenantContextAccessor tenantContextAccessor,
        FakeCategoryApplicationService categoryApplicationService,
        FakeProductQueryService productQueryService,
        FakeProductCommandService productCommandService)
    {
        return new ConfiguredTenantProvisioningService(
            tenantCatalogProvisioningService,
            tenantCatalogService,
            tenantContextAccessor,
            categoryApplicationService,
            productQueryService,
            productCommandService,
            NullLogger<ConfiguredTenantProvisioningService>.Instance);
    }

    private static TenantDefinition CreateTenantDefinition(bool seedBaseCategories, bool seedDemoCatalog)
    {
        return new TenantDefinition
        {
            TenantId = "tenant-demo",
            DisplayName = "Tenant Demo",
            Currency = "COP",
            Provisioning = new TenantProvisioningDefinition
            {
                BootstrapSuperUserEmail = "root@tenant-demo.example",
                SeedBaseCategories = seedBaseCategories,
                SeedDemoCatalog = seedDemoCatalog,
                EnablePublicStorefront = true
            }
        };
    }

    private sealed class FakeTenantContextAccessor(string initialTenantId) : ITenantContextAccessor
    {
        private string _tenantId = initialTenantId;

        public string TenantId => _tenantId;

        public bool IsAvailable => !string.IsNullOrWhiteSpace(_tenantId);

        public IDisposable BeginTenantScope(string tenantId)
        {
            string previousTenantId = _tenantId;
            _tenantId = tenantId;
            return new Scope(() => _tenantId = previousTenantId);
        }

        private sealed class Scope(Action restoreAction) : IDisposable
        {
            private bool _disposed;

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                restoreAction();
                _disposed = true;
            }
        }
    }

    private sealed class FakeTenantCatalogService(params TenantDefinition[] tenants) : ITenantCatalogService
    {
        private readonly IReadOnlyCollection<TenantDefinition> _tenants = tenants;

        public string DataIsolationMode => "SharedDatabaseSharedSchema";

        public Task<TenantDefinition> GetCurrentTenantAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_tenants.First());

        public Task<IReadOnlyCollection<TenantDefinition>> GetConfiguredTenantsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_tenants);
    }

    private sealed class FakeTenantCatalogProvisioningService : ITenantCatalogProvisioningService
    {
        public int SynchronizeCalls { get; private set; }
        public int MarkSuperUserCalls { get; private set; }
        public int MarkBaseCategoriesCalls { get; private set; }
        public int MarkDemoCatalogCalls { get; private set; }

        public Task SynchronizeConfiguredCatalogAsync(CancellationToken cancellationToken = default)
        {
            SynchronizeCalls++;
            return Task.CompletedTask;
        }

        public Task MarkSuperUserProvisionedAsync(string tenantId, string email, CancellationToken cancellationToken = default)
        {
            MarkSuperUserCalls++;
            return Task.CompletedTask;
        }

        public Task MarkBaseCategoriesProvisionedAsync(string tenantId, CancellationToken cancellationToken = default)
        {
            MarkBaseCategoriesCalls++;
            return Task.CompletedTask;
        }

        public Task MarkDemoCatalogProvisionedAsync(string tenantId, CancellationToken cancellationToken = default)
        {
            MarkDemoCatalogCalls++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeCategoryApplicationService : ICategoryApplicationService
    {
        public int ImportCalls { get; private set; }
        public ImportCategoriesFromXmlCommand? LastImportCommand { get; private set; }
        public Result<IReadOnlyCollection<CategoryDto>> CategoriesResult { get; set; } = Result.Success<IReadOnlyCollection<CategoryDto>>(Array.Empty<CategoryDto>());
        public Result<CategoryImportResultDto> ImportResult { get; set; } = Result.Success(new CategoryImportResultDto());

        public Task<Result<IReadOnlyCollection<CategoryDto>>> GetCategoriesAsync(GetCategoriesQuery query, CancellationToken cancellationToken = default)
            => Task.FromResult(CategoriesResult);

        public Task<Result<CategoryDto>> GetCategoryByIdAsync(GetCategoryByIdQuery query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<Guid>> CreateCategoryAsync(CreateCategoryCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<CategoryImportResultDto>> ImportCategoriesFromXmlAsync(ImportCategoriesFromXmlCommand command, CancellationToken cancellationToken = default)
        {
            ImportCalls++;
            LastImportCommand = command;
            return Task.FromResult(ImportResult);
        }

        public Task<Result<CategoryDto>> UpdateCategoryAsync(UpdateCategoryCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<CategoryDto>> ChangeCategoryStatusAsync(ChangeCategoryStatusCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class FakeProductQueryService : IProductQueryService
    {
        public Result<ProductQueryResultDto> QueryResult { get; set; } = Result.Success(new ProductQueryResultDto
        {
            Items = Array.Empty<ProductDto>(),
            TotalCount = 0,
            ReturnedCount = 0,
            PageNumber = 1,
            PageSize = 1,
            TotalPages = 0,
            HasPreviousPage = false,
            HasNextPage = false
        });

        public Task<Result<ProductDetailDto>> GetProductByIdAsync(GetProductByIdQuery query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<ProductQueryResultDto>> GetProductsAsync(GetProductsQuery query, CancellationToken cancellationToken = default)
            => Task.FromResult(QueryResult);
    }

    private sealed class FakeProductCommandService : IProductCommandService
    {
        public int ImportCalls { get; private set; }
        public ImportProductsCommand? LastImportCommand { get; private set; }
        public Result<ProductImportResultDto> ImportResult { get; set; } = Result.Success(new ProductImportResultDto());

        public Task<Result<Guid>> CreatePhysicalProductAsync(CreatePhysicalProductCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<Guid>> CreateDigitalProductAsync(CreateDigitalProductCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<ProductImportResultDto>> ImportProductsAsync(ImportProductsCommand command, CancellationToken cancellationToken = default)
        {
            ImportCalls++;
            LastImportCommand = command;
            return Task.FromResult(ImportResult);
        }

        public Task<Result<ProductResponseDto>> UpdateProductAsync(UpdateProductCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
