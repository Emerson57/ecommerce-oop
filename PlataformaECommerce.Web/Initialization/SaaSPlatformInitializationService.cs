using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Web.Configuration;
using PlataformaECommerce.Web.Services.Categories;

namespace PlataformaECommerce.Web.Initialization;

/// <summary>
/// Orquesta la sincronización y provisión inicial controlada de tenants SaaS durante el arranque web.
/// </summary>
public sealed class SaaSPlatformInitializationService
{
    private readonly ITenantCatalogProvisioningService _tenantCatalogProvisioningService;
    private readonly ITenantCatalogService _tenantCatalogService;
    private readonly ITenantContextAccessor _tenantContextAccessor;
    private readonly ICategoryApplicationService _categoryApplicationService;
    private readonly IProductQueryService _productQueryService;
    private readonly IProductCommandService _productCommandService;
    private readonly SuperUserBootstrapService _superUserBootstrapService;
    private readonly BootstrapSuperUserOptions _bootstrapOptions;
    private readonly ILogger<SaaSPlatformInitializationService> _logger;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="SaaSPlatformInitializationService"/>.
    /// </summary>
    /// <param name="tenantCatalogProvisioningService">Servicio responsable de sincronizar y registrar el estado de provisión SaaS.</param>
    /// <param name="tenantCatalogService">Servicio de lectura del catálogo SaaS efectivo.</param>
    /// <param name="tenantContextAccessor">Accesor al tenant activo u overrideado durante el arranque.</param>
    /// <param name="categoryApplicationService">Servicio de aplicación de categorías usado para seeds iniciales.</param>
    /// <param name="productQueryService">Servicio de consulta de productos usado para detectar semillas previas.</param>
    /// <param name="productCommandService">Servicio de escritura de productos usado para importar catálogo demo.</param>
    /// <param name="superUserBootstrapService">Servicio de bootstrap del super usuario inicial.</param>
    /// <param name="bootstrapOptions">Opciones de bootstrap del super usuario.</param>
    /// <param name="logger">Registrador estructurado del proceso de inicialización.</param>
    public SaaSPlatformInitializationService(
        ITenantCatalogProvisioningService tenantCatalogProvisioningService,
        ITenantCatalogService tenantCatalogService,
        ITenantContextAccessor tenantContextAccessor,
        ICategoryApplicationService categoryApplicationService,
        IProductQueryService productQueryService,
        IProductCommandService productCommandService,
        SuperUserBootstrapService superUserBootstrapService,
        IOptions<BootstrapSuperUserOptions> bootstrapOptions,
        ILogger<SaaSPlatformInitializationService> logger)
    {
        ArgumentNullException.ThrowIfNull(bootstrapOptions);

        _tenantCatalogProvisioningService = tenantCatalogProvisioningService ?? throw new ArgumentNullException(nameof(tenantCatalogProvisioningService));
        _tenantCatalogService = tenantCatalogService ?? throw new ArgumentNullException(nameof(tenantCatalogService));
        _tenantContextAccessor = tenantContextAccessor ?? throw new ArgumentNullException(nameof(tenantContextAccessor));
        _categoryApplicationService = categoryApplicationService ?? throw new ArgumentNullException(nameof(categoryApplicationService));
        _productQueryService = productQueryService ?? throw new ArgumentNullException(nameof(productQueryService));
        _productCommandService = productCommandService ?? throw new ArgumentNullException(nameof(productCommandService));
        _superUserBootstrapService = superUserBootstrapService ?? throw new ArgumentNullException(nameof(superUserBootstrapService));
        _bootstrapOptions = bootstrapOptions.Value;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Ejecuta la sincronización y provisión inicial controlada del catálogo SaaS y de los tenants configurados.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando sincronización y provisión SaaS controlada del arranque web.");

        await _tenantCatalogProvisioningService.SynchronizeConfiguredCatalogAsync(cancellationToken).ConfigureAwait(false);
        await ProvisionConfiguredTenantsAsync(cancellationToken).ConfigureAwait(false);
        await BootstrapConfiguredSuperUserAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Finalizó la sincronización y provisión SaaS controlada del arranque web.");
    }

    private async Task ProvisionConfiguredTenantsAsync(CancellationToken cancellationToken)
    {
        IReadOnlyCollection<TenantDefinition> tenants = await _tenantCatalogService
            .GetConfiguredTenantsAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (TenantDefinition tenant in tenants.OrderBy(current => current.DisplayName, StringComparer.OrdinalIgnoreCase))
        {
            using IDisposable tenantScope = _tenantContextAccessor.BeginTenantScope(tenant.TenantId);
            await ProvisionTenantAsync(tenant, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task ProvisionTenantAsync(TenantDefinition tenant, CancellationToken cancellationToken)
    {
        if (tenant.Provisioning.SeedBaseCategories && tenant.Provisioning.BaseCategoriesProvisionedAtUtc is null)
        {
            await EnsureBaseCategoriesProvisionedAsync(tenant, cancellationToken).ConfigureAwait(false);
        }

        if (tenant.Provisioning.SeedDemoCatalog && tenant.Provisioning.DemoCatalogProvisionedAtUtc is null)
        {
            await EnsureDemoCatalogProvisionedAsync(tenant, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task EnsureBaseCategoriesProvisionedAsync(TenantDefinition tenant, CancellationToken cancellationToken)
    {
        Result<IReadOnlyCollection<CategoryDto>> categoriesResult = await _categoryApplicationService
            .GetCategoriesAsync(new GetCategoriesQuery(), cancellationToken)
            .ConfigureAwait(false);

        if (categoriesResult.IsFailure)
        {
            throw new InvalidOperationException($"No fue posible consultar categorías iniciales para el tenant '{tenant.TenantId}'. {categoriesResult.Error.Code}: {categoriesResult.Error.Message}");
        }

        if (categoriesResult.Value.Count > 0)
        {
            _logger.LogInformation(
                "Se omitió la siembra de categorías base para el tenant '{TenantId}' porque ya existen categorías persistidas.",
                tenant.TenantId);

            await _tenantCatalogProvisioningService.MarkBaseCategoriesProvisionedAsync(tenant.TenantId, cancellationToken).ConfigureAwait(false);
            return;
        }

        Result<CategoryImportResultDto> importResult = await _categoryApplicationService
            .ImportCategoriesFromXmlAsync(
                new ImportCategoriesFromXmlCommand
                {
                    XmlContent = CategoryXmlTemplateProvider.BuildTemplate()
                },
                cancellationToken)
            .ConfigureAwait(false);

        if (importResult.IsFailure)
        {
            throw new InvalidOperationException($"No fue posible sembrar categorías base para el tenant '{tenant.TenantId}'. {importResult.Error.Code}: {importResult.Error.Message}");
        }

        await _tenantCatalogProvisioningService.MarkBaseCategoriesProvisionedAsync(tenant.TenantId, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Se completó la siembra de categorías base para el tenant '{TenantId}'. Categorías raíz: {RootCategories}. Subcategorías: {Subcategories}.",
            tenant.TenantId,
            importResult.Value.RootCategoriesCreated,
            importResult.Value.SubcategoriesCreated);
    }

    private async Task EnsureDemoCatalogProvisionedAsync(TenantDefinition tenant, CancellationToken cancellationToken)
    {
        Result<ProductQueryResultDto> productsResult = await _productQueryService
            .GetProductsAsync(
                new GetProductsQuery
                {
                    PageNumber = 1,
                    PageSize = 1,
                    SortBy = "createdAt",
                    SortDescending = false
                },
                cancellationToken)
            .ConfigureAwait(false);

        if (productsResult.IsFailure)
        {
            throw new InvalidOperationException($"No fue posible consultar el catálogo demo para el tenant '{tenant.TenantId}'. {productsResult.Error.Code}: {productsResult.Error.Message}");
        }

        if (productsResult.Value.TotalCount > 0)
        {
            _logger.LogInformation(
                "Se omitió la siembra del catálogo demo para el tenant '{TenantId}' porque ya existen productos persistidos.",
                tenant.TenantId);

            await _tenantCatalogProvisioningService.MarkDemoCatalogProvisionedAsync(tenant.TenantId, cancellationToken).ConfigureAwait(false);
            return;
        }

        await EnsureActiveRootCategoriesAvailableAsync(tenant, cancellationToken).ConfigureAwait(false);

        Result<ProductImportResultDto> importResult = await _productCommandService
            .ImportProductsAsync(
                new ImportProductsCommand
                {
                    Rows = BuildDemoCatalogRows(tenant)
                },
                cancellationToken)
            .ConfigureAwait(false);

        if (importResult.IsFailure)
        {
            throw new InvalidOperationException($"No fue posible sembrar el catálogo demo para el tenant '{tenant.TenantId}'. {importResult.Error.Code}: {importResult.Error.Message}");
        }

        await _tenantCatalogProvisioningService.MarkDemoCatalogProvisionedAsync(tenant.TenantId, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Se completó la siembra del catálogo demo para el tenant '{TenantId}'. Productos físicos: {PhysicalProducts}. Productos digitales: {DigitalProducts}.",
            tenant.TenantId,
            importResult.Value.PhysicalProductsCreated,
            importResult.Value.DigitalProductsCreated);
    }

    private async Task EnsureActiveRootCategoriesAvailableAsync(TenantDefinition tenant, CancellationToken cancellationToken)
    {
        Result<IReadOnlyCollection<CategoryDto>> categoriesResult = await _categoryApplicationService
            .GetCategoriesAsync(new GetCategoriesQuery { OnlyActive = true }, cancellationToken)
            .ConfigureAwait(false);

        if (categoriesResult.IsFailure)
        {
            throw new InvalidOperationException($"No fue posible validar categorías activas para el tenant '{tenant.TenantId}'. {categoriesResult.Error.Code}: {categoriesResult.Error.Message}");
        }

        if (categoriesResult.Value.Any(category => category.IsRootCategory))
        {
            return;
        }

        _logger.LogInformation(
            "El tenant '{TenantId}' solicitó catálogo demo sin categorías activas; se sembrarán categorías base como prerrequisito.",
            tenant.TenantId);

        await EnsureBaseCategoriesProvisionedAsync(tenant, cancellationToken).ConfigureAwait(false);
    }

    private async Task BootstrapConfiguredSuperUserAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_bootstrapOptions.TenantId))
        {
            await _superUserBootstrapService.BootstrapAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        using IDisposable tenantScope = _tenantContextAccessor.BeginTenantScope(_bootstrapOptions.TenantId);
        await _superUserBootstrapService.BootstrapAsync(cancellationToken).ConfigureAwait(false);
    }

    private static IReadOnlyCollection<ImportProductRowCommand> BuildDemoCatalogRows(TenantDefinition tenant)
    {
        string currency = string.IsNullOrWhiteSpace(tenant.Currency)
            ? "USD"
            : tenant.Currency.Trim().ToUpperInvariant();

        return
        [
            new ImportProductRowCommand
            {
                RowNumber = 2,
                Name = $"Kit inicial {tenant.DisplayName}",
                Description = "Producto físico de muestra para validación operativa del storefront y backoffice.",
                Sku = $"{tenant.TenantId.ToUpperInvariant()}-STARTER-KIT",
                Price = 149m,
                Currency = currency,
                Stock = 25,
                IsActive = true,
                ProductType = TipoProducto.Fisico,
                Slug = $"kit-inicial-{tenant.TenantId.ToLowerInvariant()}",
                CategoryName = "Tecnologia",
                SubcategoryName = "Laptops",
                SerializedTags = "demo,inicial,saas",
                WeightKg = 1.25m,
                HeightCm = 12m,
                WidthCm = 32m,
                LengthCm = 42m,
                RequiresShipping = true
            },
            new ImportProductRowCommand
            {
                RowNumber = 3,
                Name = $"Guía digital {tenant.DisplayName}",
                Description = "Contenido digital de muestra para probar flujos de catálogo y checkout.",
                Sku = $"{tenant.TenantId.ToUpperInvariant()}-DIGITAL-GUIDE",
                Price = 49m,
                Currency = currency,
                Stock = 100,
                IsActive = true,
                ProductType = TipoProducto.Digital,
                Slug = $"guia-digital-{tenant.TenantId.ToLowerInvariant()}",
                CategoryName = "Tecnologia",
                FileFormat = "PDF",
                FileSizeMb = 12m,
                RequiresLicense = false
            }
        ];
    }
}
