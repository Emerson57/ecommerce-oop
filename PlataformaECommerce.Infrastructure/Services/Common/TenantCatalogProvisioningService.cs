using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Infrastructure.Configurations;
using PlataformaECommerce.Infrastructure.Persistence.Context;
using PlataformaECommerce.Infrastructure.Persistence.Entities;

namespace PlataformaECommerce.Infrastructure.Services.Common;

/// <summary>
/// Sincroniza el catálogo SaaS configurado hacia persistencia relacional y registra el estado operativo mínimo del aprovisionamiento inicial.
/// </summary>
public sealed class TenantCatalogProvisioningService : ITenantCatalogProvisioningService
{
    private readonly ECommerceDbContext _dbContext;
    private readonly IOptionsMonitor<SaaSPlatformOptions> _optionsMonitor;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<TenantCatalogProvisioningService> _logger;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="TenantCatalogProvisioningService"/>.
    /// </summary>
    public TenantCatalogProvisioningService(
        ECommerceDbContext dbContext,
        IOptionsMonitor<SaaSPlatformOptions> optionsMonitor,
        IDateTimeProvider dateTimeProvider,
        ILogger<TenantCatalogProvisioningService> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task SynchronizeConfiguredCatalogAsync(CancellationToken cancellationToken = default)
    {
        if (!await SaaSPersistenceMigrationGuard.IsPersistentCatalogReadyAsync(_dbContext, cancellationToken).ConfigureAwait(false))
        {
            _logger.LogWarning(
                "Se omitió la sincronización del catálogo SaaS persistente porque la migración requerida aún no fue aplicada. Use el flujo oficial de migraciones antes de habilitar persistencia SaaS.");
            return;
        }

        SaaSPlatformOptions options = _optionsMonitor.CurrentValue;
        DateTime synchronizedAtUtc = _dateTimeProvider.UtcNow;

        List<TenantFeatureEntity> persistedFeatures = await _dbContext.TenantFeatures
            .AsSplitQuery()
            .Include(feature => feature.PlanFeatures)
            .Include(feature => feature.TenantAssignments)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        Dictionary<string, TenantFeatureEntity> featuresById = persistedFeatures
            .ToDictionary(feature => feature.FeatureId, StringComparer.OrdinalIgnoreCase);

        HashSet<string> configuredFeatureIds = new(StringComparer.OrdinalIgnoreCase);

        foreach (SaaSPlatformOptions.FeatureOptions configuredFeature in options.Features)
        {
            string featureId = configuredFeature.FeatureId.Trim();
            configuredFeatureIds.Add(featureId);

            if (!featuresById.TryGetValue(featureId, out TenantFeatureEntity? featureEntity))
            {
                featureEntity = new TenantFeatureEntity { FeatureId = featureId };
                _dbContext.TenantFeatures.Add(featureEntity);
                featuresById.Add(featureId, featureEntity);
            }

            featureEntity.DisplayName = configuredFeature.DisplayName.Trim();
            featureEntity.Description = configuredFeature.Description.Trim();
            featureEntity.Category = configuredFeature.Category.Trim();
            featureEntity.Enabled = true;
        }

        foreach (TenantFeatureEntity persistedFeature in persistedFeatures.Where(feature => !configuredFeatureIds.Contains(feature.FeatureId)))
        {
            persistedFeature.Enabled = false;
        }

        List<TenantPlanEntity> persistedPlans = await _dbContext.TenantPlans
            .Include(plan => plan.PlanFeatures)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        Dictionary<string, TenantPlanEntity> plansById = persistedPlans
            .ToDictionary(plan => plan.PlanId, StringComparer.OrdinalIgnoreCase);

        HashSet<string> configuredPlanIds = new(StringComparer.OrdinalIgnoreCase);

        foreach (SaaSPlatformOptions.PlanOptions configuredPlan in options.Plans)
        {
            string planId = configuredPlan.PlanId.Trim();
            configuredPlanIds.Add(planId);

            if (!plansById.TryGetValue(planId, out TenantPlanEntity? planEntity))
            {
                planEntity = new TenantPlanEntity { PlanId = planId };
                _dbContext.TenantPlans.Add(planEntity);
                plansById.Add(planId, planEntity);
            }

            planEntity.DisplayName = configuredPlan.DisplayName.Trim();
            planEntity.Description = configuredPlan.Description.Trim();
            planEntity.MonthlyPrice = configuredPlan.MonthlyPrice;
            planEntity.Currency = configuredPlan.Currency.Trim();
            planEntity.IncludedAdministrators = configuredPlan.IncludedAdministrators;
            planEntity.IncludedProducts = configuredPlan.IncludedProducts;
            planEntity.Enabled = true;

            planEntity.PlanFeatures.Clear();
            foreach (string featureId in configuredPlan.IncludedFeatureIds
                         .Where(featureId => !string.IsNullOrWhiteSpace(featureId))
                         .Select(featureId => featureId.Trim())
                         .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                planEntity.PlanFeatures.Add(new TenantPlanFeatureEntity
                {
                    PlanId = planId,
                    FeatureId = featureId
                });
            }
        }

        foreach (TenantPlanEntity persistedPlan in persistedPlans.Where(plan => !configuredPlanIds.Contains(plan.PlanId)))
        {
            persistedPlan.Enabled = false;
            persistedPlan.PlanFeatures.Clear();
        }

        List<TenantEntity> persistedTenants = await _dbContext.Tenants
            .AsSplitQuery()
            .Include(tenant => tenant.Hostnames)
            .Include(tenant => tenant.FeatureAssignments)
            .Include(tenant => tenant.Subscription)
            .Include(tenant => tenant.Provisioning)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        Dictionary<string, TenantEntity> tenantsById = persistedTenants
            .ToDictionary(tenant => tenant.TenantId, StringComparer.OrdinalIgnoreCase);

        HashSet<string> configuredTenantIds = new(StringComparer.OrdinalIgnoreCase);

        foreach (SaaSPlatformOptions.TenantOptions configuredTenant in options.Tenants)
        {
            string tenantId = configuredTenant.TenantId.Trim();
            configuredTenantIds.Add(tenantId);

            if (!tenantsById.TryGetValue(tenantId, out TenantEntity? tenantEntity))
            {
                tenantEntity = new TenantEntity { TenantId = tenantId };
                _dbContext.Tenants.Add(tenantEntity);
                tenantsById.Add(tenantId, tenantEntity);
            }

            tenantEntity.DisplayName = configuredTenant.DisplayName.Trim();
            tenantEntity.Enabled = configuredTenant.Enabled;
            tenantEntity.StorefrontName = configuredTenant.StorefrontName.Trim();
            tenantEntity.BackofficeName = configuredTenant.BackofficeName.Trim();
            tenantEntity.StorefrontTagline = configuredTenant.StorefrontTagline.Trim();
            tenantEntity.LegalCompanyName = configuredTenant.LegalCompanyName.Trim();
            tenantEntity.SupportEmail = configuredTenant.SupportEmail.Trim();
            tenantEntity.SupportPhone = configuredTenant.SupportPhone.Trim();
            tenantEntity.SupportHours = configuredTenant.SupportHours.Trim();
            tenantEntity.SupportSla = configuredTenant.SupportSla.Trim();
            tenantEntity.PrimaryColor = configuredTenant.PrimaryColor.Trim();
            tenantEntity.AccentColor = configuredTenant.AccentColor.Trim();
            tenantEntity.AdminSidebarStartColor = configuredTenant.AdminSidebarStartColor.Trim();
            tenantEntity.AdminSidebarEndColor = configuredTenant.AdminSidebarEndColor.Trim();
            tenantEntity.LogoGlyph = configuredTenant.LogoGlyph.Trim();
            tenantEntity.Currency = configuredTenant.Currency.Trim();

            tenantEntity.Hostnames.Clear();
            foreach (string hostname in configuredTenant.Hostnames
                         .Where(hostname => !string.IsNullOrWhiteSpace(hostname))
                         .Select(hostname => hostname.Trim())
                         .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                tenantEntity.Hostnames.Add(new TenantHostnameEntity
                {
                    TenantId = tenantId,
                    Hostname = hostname
                });
            }

            tenantEntity.FeatureAssignments.Clear();
            foreach (string featureId in configuredTenant.EnabledFeatureIds
                         .Where(featureId => !string.IsNullOrWhiteSpace(featureId))
                         .Select(featureId => featureId.Trim())
                         .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                tenantEntity.FeatureAssignments.Add(new TenantFeatureAssignmentEntity
                {
                    TenantId = tenantId,
                    FeatureId = featureId
                });
            }

            tenantEntity.Subscription ??= new TenantSubscriptionEntity { TenantId = tenantId };
            tenantEntity.Subscription.PlanId = string.IsNullOrWhiteSpace(configuredTenant.PlanId) ? null : configuredTenant.PlanId.Trim();
            tenantEntity.Subscription.Status = configuredTenant.Subscription.Status.Trim();
            tenantEntity.Subscription.StartedAtUtc = configuredTenant.Subscription.StartedAtUtc;
            tenantEntity.Subscription.TrialEndsAtUtc = configuredTenant.Subscription.TrialEndsAtUtc;
            tenantEntity.Subscription.RenewalAtUtc = configuredTenant.Subscription.RenewalAtUtc;
            tenantEntity.Subscription.AutoRenew = configuredTenant.Subscription.AutoRenew;
            tenantEntity.Subscription.SeatsPurchased = configuredTenant.Subscription.SeatsPurchased;

            tenantEntity.Provisioning ??= new TenantProvisioningEntity { TenantId = tenantId };
            tenantEntity.Provisioning.BootstrapSuperUserEmail = configuredTenant.Provisioning.BootstrapSuperUserEmail.Trim();
            tenantEntity.Provisioning.SeedBaseCategories = configuredTenant.Provisioning.SeedBaseCategories;
            tenantEntity.Provisioning.SeedDemoCatalog = configuredTenant.Provisioning.SeedDemoCatalog;
            tenantEntity.Provisioning.EnablePublicStorefront = configuredTenant.Provisioning.EnablePublicStorefront;
            tenantEntity.Provisioning.Notes = string.IsNullOrWhiteSpace(configuredTenant.Provisioning.Notes)
                ? null
                : configuredTenant.Provisioning.Notes.Trim();
            tenantEntity.Provisioning.LastSynchronizedAtUtc = synchronizedAtUtc;
        }

        foreach (TenantEntity persistedTenant in persistedTenants.Where(tenant => !configuredTenantIds.Contains(tenant.TenantId)))
        {
            persistedTenant.Enabled = false;
        }

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task MarkSuperUserProvisionedAsync(string tenantId, string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ArgumentException("El tenant objetivo es obligatorio.", nameof(tenantId));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("El correo del super usuario provisionado es obligatorio.", nameof(email));
        }

        string normalizedTenantId = tenantId.Trim();
        string normalizedEmail = email.Trim();
        await UpdateProvisioningAsync(
                normalizedTenantId,
                (provisioning, timestampUtc) =>
                {
                    provisioning.BootstrapSuperUserEmail = normalizedEmail;
                    provisioning.SuperUserProvisionedAtUtc = timestampUtc;
                },
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task MarkBaseCategoriesProvisionedAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ArgumentException("El tenant objetivo es obligatorio.", nameof(tenantId));
        }

        return UpdateProvisioningAsync(
            tenantId.Trim(),
            static (provisioning, timestampUtc) => provisioning.BaseCategoriesProvisionedAtUtc = timestampUtc,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task MarkDemoCatalogProvisionedAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ArgumentException("El tenant objetivo es obligatorio.", nameof(tenantId));
        }

        return UpdateProvisioningAsync(
            tenantId.Trim(),
            static (provisioning, timestampUtc) => provisioning.DemoCatalogProvisionedAtUtc = timestampUtc,
            cancellationToken);
    }

    private async Task UpdateProvisioningAsync(
        string tenantId,
        Action<TenantProvisioningEntity, DateTime> updateAction,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(updateAction);

        if (!await SaaSPersistenceMigrationGuard.IsPersistentCatalogReadyAsync(_dbContext, cancellationToken).ConfigureAwait(false))
        {
            _logger.LogWarning(
                "Se omitió la actualización del estado persistente de provisioning para el tenant '{TenantId}' porque la migración SaaS requerida aún no fue aplicada.",
                tenantId);
            return;
        }

        DateTime timestampUtc = _dateTimeProvider.UtcNow;
        TenantProvisioningEntity provisioning = await GetOrCreateProvisioningAsync(tenantId, cancellationToken).ConfigureAwait(false);
        updateAction(provisioning, timestampUtc);
        provisioning.LastSynchronizedAtUtc ??= timestampUtc;

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<TenantProvisioningEntity> GetOrCreateProvisioningAsync(string tenantId, CancellationToken cancellationToken)
    {
        TenantProvisioningEntity? provisioning = await _dbContext.TenantProvisionings
            .FirstOrDefaultAsync(current => current.TenantId == tenantId, cancellationToken)
            .ConfigureAwait(false);

        if (provisioning is not null)
        {
            return provisioning;
        }

        bool tenantExists = await _dbContext.Tenants
            .AnyAsync(current => current.TenantId == tenantId, cancellationToken)
            .ConfigureAwait(false);

        if (!tenantExists)
        {
            throw new InvalidOperationException($"No existe un tenant persistido con identificador '{tenantId}'.");
        }

        provisioning = new TenantProvisioningEntity
        {
            TenantId = tenantId
        };

        await _dbContext.TenantProvisionings.AddAsync(provisioning, cancellationToken).ConfigureAwait(false);
        return provisioning;
    }
}
