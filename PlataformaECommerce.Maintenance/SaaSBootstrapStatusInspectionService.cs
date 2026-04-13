using Microsoft.EntityFrameworkCore;
using PlataformaECommerce.Application.Common.SaaS;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Infrastructure.Persistence.Context;
using PlataformaECommerce.Infrastructure.Persistence.Entities;

namespace PlataformaECommerce.Maintenance;

internal sealed class SaaSBootstrapStatusInspectionService
{
    private readonly ECommerceDbContext _dbContext;
    private readonly ITenantCatalogService _tenantCatalogService;

    public SaaSBootstrapStatusInspectionService(
        ECommerceDbContext dbContext,
        ITenantCatalogService tenantCatalogService)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _tenantCatalogService = tenantCatalogService ?? throw new ArgumentNullException(nameof(tenantCatalogService));
    }

    public async Task<IReadOnlyCollection<SaaSBootstrapStatusResult>> InspectAsync(
        string? tenantOverride,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<TenantDefinition> configuredTenants = await _tenantCatalogService
            .GetConfiguredTenantsAsync(cancellationToken)
            .ConfigureAwait(false);

        IReadOnlyCollection<TenantDefinition> targetTenants = string.IsNullOrWhiteSpace(tenantOverride)
            ? configuredTenants
            : configuredTenants
                .Where(tenant => string.Equals(tenant.TenantId, tenantOverride.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToArray();

        if (targetTenants.Count == 0)
        {
            return Array.Empty<SaaSBootstrapStatusResult>();
        }

        string[] tenantIds = targetTenants.Select(tenant => tenant.TenantId.Trim()).ToArray();

        Dictionary<string, TenantEntity> persistedTenants = await _dbContext.Tenants
            .AsNoTracking()
            .Where(tenant => tenantIds.Contains(tenant.TenantId))
            .ToDictionaryAsync(tenant => tenant.TenantId, StringComparer.OrdinalIgnoreCase, cancellationToken)
            .ConfigureAwait(false);

        Dictionary<string, TenantProvisioningEntity> persistedProvisionings = await _dbContext.TenantProvisionings
            .AsNoTracking()
            .Where(provisioning => tenantIds.Contains(provisioning.TenantId))
            .ToDictionaryAsync(provisioning => provisioning.TenantId, StringComparer.OrdinalIgnoreCase, cancellationToken)
            .ConfigureAwait(false);

        return targetTenants
            .OrderBy(tenant => tenant.DisplayName, StringComparer.OrdinalIgnoreCase)
            .Select(tenant => CreateResult(tenant, persistedTenants, persistedProvisionings))
            .ToArray();
    }

    private static SaaSBootstrapStatusResult CreateResult(
        TenantDefinition tenant,
        IReadOnlyDictionary<string, TenantEntity> persistedTenants,
        IReadOnlyDictionary<string, TenantProvisioningEntity> persistedProvisionings)
    {
        ArgumentNullException.ThrowIfNull(tenant);
        ArgumentNullException.ThrowIfNull(persistedTenants);
        ArgumentNullException.ThrowIfNull(persistedProvisionings);

        bool tenantExists = persistedTenants.TryGetValue(tenant.TenantId, out _);
        persistedProvisionings.TryGetValue(tenant.TenantId, out TenantProvisioningEntity? provisioning);

        return new SaaSBootstrapStatusResult(
            TenantId: tenant.TenantId,
            DisplayName: tenant.DisplayName,
            PersistedTenantExists: tenantExists,
            ProvisioningStateExists: provisioning is not null,
            CatalogSynchronizedAtUtc: provisioning?.LastSynchronizedAtUtc,
            BaseCategoriesRequired: tenant.Provisioning.SeedBaseCategories,
            BaseCategoriesProvisioned: provisioning?.BaseCategoriesProvisionedAtUtc is not null,
            BaseCategoriesProvisionedAtUtc: provisioning?.BaseCategoriesProvisionedAtUtc,
            DemoCatalogRequired: tenant.Provisioning.SeedDemoCatalog,
            DemoCatalogProvisioned: provisioning?.DemoCatalogProvisionedAtUtc is not null,
            DemoCatalogProvisionedAtUtc: provisioning?.DemoCatalogProvisionedAtUtc,
            ExpectedSuperUserEmail: tenant.Provisioning.BootstrapSuperUserEmail.Trim(),
            SuperUserProvisioned: provisioning?.SuperUserProvisionedAtUtc is not null,
            SuperUserProvisionedAtUtc: provisioning?.SuperUserProvisionedAtUtc);
    }
}
