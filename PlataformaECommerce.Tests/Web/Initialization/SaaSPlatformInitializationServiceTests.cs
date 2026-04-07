using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Common.SaaS;
using PlataformaECommerce.Application.Features.Admin.Commands;
using PlataformaECommerce.Application.Features.Admin.DTOs;
using PlataformaECommerce.Application.Features.Admin.Queries;
using PlataformaECommerce.Application.Features.Categories.Commands;
using PlataformaECommerce.Application.Features.Categories.DTOs;
using PlataformaECommerce.Application.Features.Categories.Queries;
using PlataformaECommerce.Application.Features.Products.Commands;
using PlataformaECommerce.Application.Features.Products.DTOs;
using PlataformaECommerce.Application.Features.Products.Queries;
using PlataformaECommerce.Application.Interfaces.Repositories.Users;
using PlataformaECommerce.Application.Interfaces.Services.Admin;
using PlataformaECommerce.Application.Interfaces.Services.Categories;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Application.Interfaces.Services.Products;
using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.ValueObjects;
using PlataformaECommerce.Web.Configuration;
using PlataformaECommerce.Web.Initialization;

namespace PlataformaECommerce.Tests.Web.Initialization;

[TestFixture]
public class SaaSPlatformInitializationServiceTests
{
    [Test]
    public async Task InitializeAsync_SincronizaCatalogoYBootstrappeaSuperUsuarioConfigurado()
    {
        FakeTenantCatalogProvisioningService tenantCatalogProvisioningService = new();
        FakeTenantCatalogService tenantCatalogService = new(
            CreateTenantDefinition(seedBaseCategories: false, seedDemoCatalog: false));
        FakeTenantContextAccessor tenantContextAccessor = new("platform-default");
        FakeCategoryApplicationService categoryApplicationService = new();
        FakeProductQueryService productQueryService = new();
        FakeProductCommandService productCommandService = new();
        FakeUserRepository userRepository = new();
        FakeAdminApplicationService adminApplicationService = new();
        SuperUserBootstrapService superUserBootstrapService = CreateBootstrapService(
            CreateEnabledBootstrapOptions(),
            userRepository,
            adminApplicationService,
            tenantCatalogProvisioningService,
            tenantContextAccessor);
        SaaSPlatformInitializationService service = CreateService(
            tenantCatalogProvisioningService,
            tenantCatalogService,
            tenantContextAccessor,
            categoryApplicationService,
            productQueryService,
            productCommandService,
            superUserBootstrapService,
            CreateEnabledBootstrapOptions());

        await service.InitializeAsync(CancellationToken.None);

        Assert.That(tenantCatalogProvisioningService.SynchronizeCalls, Is.EqualTo(1));
        Assert.That(adminApplicationService.RegisterCalls, Is.EqualTo(1));
        Assert.That(tenantCatalogProvisioningService.MarkSuperUserCalls, Is.EqualTo(1));
    }

    [Test]
    public async Task InitializeAsync_CategoriasBasePendientes_ImportaPlantillaYMarcaProvision()
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
        SaaSPlatformInitializationService service = CreateService(
            tenantCatalogProvisioningService,
            tenantCatalogService,
            tenantContextAccessor,
            categoryApplicationService,
            new FakeProductQueryService(),
            new FakeProductCommandService(),
            CreateBootstrapService(
                new BootstrapSuperUserOptions { Enabled = false },
                new FakeUserRepository(),
                new FakeAdminApplicationService(),
                tenantCatalogProvisioningService,
                tenantContextAccessor),
            new BootstrapSuperUserOptions { Enabled = false });

        await service.InitializeAsync(CancellationToken.None);

        Assert.That(categoryApplicationService.ImportCalls, Is.EqualTo(1));
        Assert.That(categoryApplicationService.LastImportCommand?.XmlContent, Does.Contain("Tecnologia"));
        Assert.That(tenantCatalogProvisioningService.MarkBaseCategoriesCalls, Is.EqualTo(1));
    }

    [Test]
    public async Task InitializeAsync_CatalogoDemoPendiente_ImportaProductosYMarcaProvision()
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
        SaaSPlatformInitializationService service = CreateService(
            tenantCatalogProvisioningService,
            tenantCatalogService,
            tenantContextAccessor,
            categoryApplicationService,
            productQueryService,
            productCommandService,
            CreateBootstrapService(
                new BootstrapSuperUserOptions { Enabled = false },
                new FakeUserRepository(),
                new FakeAdminApplicationService(),
                tenantCatalogProvisioningService,
                tenantContextAccessor),
            new BootstrapSuperUserOptions { Enabled = false });

        await service.InitializeAsync(CancellationToken.None);

        Assert.That(productCommandService.ImportCalls, Is.EqualTo(1));
        Assert.That(productCommandService.LastImportCommand?.Rows.Count, Is.EqualTo(2));
        Assert.That(tenantCatalogProvisioningService.MarkDemoCatalogCalls, Is.EqualTo(1));
    }

    private static SaaSPlatformInitializationService CreateService(
        FakeTenantCatalogProvisioningService tenantCatalogProvisioningService,
        FakeTenantCatalogService tenantCatalogService,
        FakeTenantContextAccessor tenantContextAccessor,
        FakeCategoryApplicationService categoryApplicationService,
        FakeProductQueryService productQueryService,
        FakeProductCommandService productCommandService,
        SuperUserBootstrapService superUserBootstrapService,
        BootstrapSuperUserOptions bootstrapOptions)
    {
        return new SaaSPlatformInitializationService(
            tenantCatalogProvisioningService,
            tenantCatalogService,
            tenantContextAccessor,
            categoryApplicationService,
            productQueryService,
            productCommandService,
            superUserBootstrapService,
            Options.Create(bootstrapOptions),
            NullLogger<SaaSPlatformInitializationService>.Instance);
    }

    private static SuperUserBootstrapService CreateBootstrapService(
        BootstrapSuperUserOptions options,
        FakeUserRepository userRepository,
        FakeAdminApplicationService adminApplicationService,
        FakeTenantCatalogProvisioningService tenantCatalogProvisioningService,
        FakeTenantContextAccessor tenantContextAccessor)
    {
        return new SuperUserBootstrapService(
            Options.Create(options),
            userRepository,
            adminApplicationService,
            tenantCatalogProvisioningService,
            tenantContextAccessor,
            new FakeHostEnvironment(),
            NullLogger<SuperUserBootstrapService>.Instance);
    }

    private static BootstrapSuperUserOptions CreateEnabledBootstrapOptions()
    {
        return new BootstrapSuperUserOptions
        {
            Enabled = true,
            TenantId = "tenant-demo",
            Name = "Root Demo",
            Email = "root@tenant-demo.example",
            Password = "Password#2026",
            Area = "Plataforma"
        };
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

    private sealed class FakeHostEnvironment : Microsoft.Extensions.Hosting.IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "PlataformaECommerce.Web";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
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

    private sealed class FakeAdminApplicationService : IAdminApplicationService
    {
        public int RegisterCalls { get; private set; }

        public Task<Result<AdminDto>> RegisterAdminAsync(RegisterAdminCommand command, CancellationToken cancellationToken = default)
        {
            RegisterCalls++;
            return Task.FromResult(Result.Success(new AdminDto()));
        }

        public Task<Result<AdminRegistrationDefinitionDto>> GetAdminRegistrationDefinitionAsync(GetAdminRegistrationDefinitionQuery query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<AdminDashboardDto>> GetDashboardAsync(GetAdminDashboardQuery query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<AdminUsersBackofficeDto>> GetUsersAsync(GetAdminUsersQuery query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<AdminBackofficeUserDto>> ResetUserPasswordAsync(ResetUserPasswordCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public Task<IReadOnlyCollection<Usuario>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Usuario>>(Array.Empty<Usuario>());

        public Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<Usuario?>(null);

        public Task<Usuario?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
            => Task.FromResult<Usuario?>(null);

        public Task<IReadOnlyCollection<Usuario>> GetByRoleAsync(RolUsuario rol, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Usuario>>(Array.Empty<Usuario>());

        public Task<IReadOnlyCollection<Cliente>> GetCustomersAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Cliente>>(Array.Empty<Cliente>());

        public Task<IReadOnlyCollection<Administrador>> GetAdministratorsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Administrador>>(Array.Empty<Administrador>());

        public Task<Cliente?> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<Cliente?>(null);

        public Task<Administrador?> GetAdministratorByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<Administrador?>(null);

        public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<bool> ExistsByRoleAsync(RolUsuario rol, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task UpdateAsync(Usuario usuario, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
