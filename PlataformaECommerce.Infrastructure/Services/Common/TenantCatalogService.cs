using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Application.Common.SaaS;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Infrastructure.Configurations;
using PlataformaECommerce.Infrastructure.Persistence.Context;
using PlataformaECommerce.Infrastructure.Persistence.Entities;

namespace PlataformaECommerce.Infrastructure.Services.Common;

/// <summary>
/// Resuelve el catálogo SaaS efectivo desde persistencia relacional con fallback controlado a configuración.
/// </summary>
public sealed class TenantCatalogService : ITenantCatalogService
{
    private readonly ECommerceDbContext _dbContext;
    private readonly IOptionsMonitor<SaaSPlatformOptions> _optionsMonitor;
    private readonly ITenantContextAccessor _tenantContextAccessor;
    private readonly ILogger<TenantCatalogService> _logger;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="TenantCatalogService"/>.
    /// </summary>
    public TenantCatalogService(
        ECommerceDbContext dbContext,
        IOptionsMonitor<SaaSPlatformOptions> optionsMonitor,
        ITenantContextAccessor tenantContextAccessor,
        ILogger<TenantCatalogService> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
        _tenantContextAccessor = tenantContextAccessor ?? throw new ArgumentNullException(nameof(tenantContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public string DataIsolationMode => _optionsMonitor.CurrentValue.DataIsolationMode;

    /// <inheritdoc />
    public async Task<TenantDefinition> GetCurrentTenantAsync(CancellationToken cancellationToken = default)
    {
        string tenantId = _tenantContextAccessor.TenantId;

        if (!await SaaSPersistenceMigrationGuard.IsPersistentCatalogReadyAsync(_dbContext, cancellationToken).ConfigureAwait(false))
        {
            _logger.LogInformation(
                "Se usó fallback a configuración para resolver el tenant '{TenantId}' porque la migración SaaS requerida aún no fue aplicada.",
                tenantId);
            return GetConfiguredTenant(tenantId);
        }

        TenantDefinition? persistedTenant = await TryGetPersistedTenantAsync(tenantId, cancellationToken).ConfigureAwait(false);
        if (persistedTenant is not null)
        {
            return persistedTenant;
        }

        return GetConfiguredTenant(tenantId);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<TenantDefinition>> GetConfiguredTenantsAsync(CancellationToken cancellationToken = default)
    {
        if (!await SaaSPersistenceMigrationGuard.IsPersistentCatalogReadyAsync(_dbContext, cancellationToken).ConfigureAwait(false))
        {
            _logger.LogInformation("Se usó fallback a configuración para listar tenants porque la migración SaaS requerida aún no fue aplicada.");
            SaaSPlatformOptions configuredOptions = _optionsMonitor.CurrentValue;
            return configuredOptions.Tenants
                .Where(tenant => tenant.Enabled)
                .Select(tenant => MapConfiguredTenant(tenant, configuredOptions))
                .OrderBy(tenant => tenant.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        IReadOnlyCollection<TenantDefinition> persistedTenants = await GetPersistedTenantsAsync(cancellationToken).ConfigureAwait(false);
        if (persistedTenants.Count > 0)
        {
            return persistedTenants;
        }

        SaaSPlatformOptions options = _optionsMonitor.CurrentValue;
        return options.Tenants
            .Where(tenant => tenant.Enabled)
            .Select(tenant => MapConfiguredTenant(tenant, options))
            .OrderBy(tenant => tenant.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private async Task<TenantDefinition?> TryGetPersistedTenantAsync(string tenantId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return null;
        }

        TenantEntity? tenant = await _dbContext.Tenants
            .AsNoTracking()
            .AsSplitQuery()
            .Where(current => current.Enabled && current.TenantId == tenantId)
            .Include(current => current.Hostnames)
            .Include(current => current.FeatureAssignments)
                .ThenInclude(assignment => assignment.Feature)
            .Include(current => current.Subscription)
                .ThenInclude(subscription => subscription!.Plan)
                    .ThenInclude(plan => plan!.PlanFeatures)
                        .ThenInclude(planFeature => planFeature.Feature)
            .Include(current => current.Provisioning)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return tenant is null ? null : MapPersistedTenant(tenant);
    }

    private async Task<IReadOnlyCollection<TenantDefinition>> GetPersistedTenantsAsync(CancellationToken cancellationToken)
    {
        TenantEntity[] tenants = await _dbContext.Tenants
            .AsNoTracking()
            .AsSplitQuery()
            .Where(tenant => tenant.Enabled)
            .Include(tenant => tenant.Hostnames)
            .Include(tenant => tenant.FeatureAssignments)
                .ThenInclude(assignment => assignment.Feature)
            .Include(tenant => tenant.Subscription)
                .ThenInclude(subscription => subscription!.Plan)
                    .ThenInclude(plan => plan!.PlanFeatures)
                        .ThenInclude(planFeature => planFeature.Feature)
            .Include(tenant => tenant.Provisioning)
            .OrderBy(tenant => tenant.DisplayName)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return tenants.Select(MapPersistedTenant).ToArray();
    }

    private TenantDefinition GetConfiguredTenant(string tenantId)
    {
        SaaSPlatformOptions options = _optionsMonitor.CurrentValue;
        SaaSPlatformOptions.TenantOptions tenant = options.Tenants.FirstOrDefault(current =>
            current.Enabled && string.Equals(current.TenantId, tenantId, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"No se encontró una definición SaaS habilitada para el tenant '{tenantId}'.");

        return MapConfiguredTenant(tenant, options);
    }

    private static TenantDefinition MapPersistedTenant(TenantEntity tenant)
    {
        TenantPlanEntity? currentPlan = tenant.Subscription?.Plan is { Enabled: true } plan ? plan : null;
        HashSet<string> featureIds = new(StringComparer.OrdinalIgnoreCase);

        if (currentPlan is not null)
        {
            foreach (string featureId in currentPlan.PlanFeatures
                         .Where(planFeature => planFeature.Feature.Enabled)
                         .Select(planFeature => planFeature.FeatureId))
            {
                featureIds.Add(featureId);
            }
        }

        foreach (string featureId in tenant.FeatureAssignments
                     .Where(assignment => assignment.Feature.Enabled)
                     .Select(assignment => assignment.FeatureId))
        {
            featureIds.Add(featureId);
        }

        TenantFeatureDefinition[] features = tenant.FeatureAssignments
            .Where(assignment => assignment.Feature.Enabled)
            .Select(assignment => assignment.Feature)
            .Concat(currentPlan?.PlanFeatures
                .Where(planFeature => planFeature.Feature.Enabled)
                .Select(planFeature => planFeature.Feature)
                ?? Array.Empty<TenantFeatureEntity>())
            .Where(feature => featureIds.Contains(feature.FeatureId))
            .DistinctBy(feature => feature.FeatureId, StringComparer.OrdinalIgnoreCase)
            .OrderBy(feature => feature.DisplayName, StringComparer.OrdinalIgnoreCase)
            .Select(feature => new TenantFeatureDefinition
            {
                FeatureId = feature.FeatureId,
                DisplayName = feature.DisplayName,
                Description = feature.Description,
                Category = feature.Category
            })
            .ToArray();

        return new TenantDefinition
        {
            TenantId = tenant.TenantId,
            DisplayName = tenant.DisplayName,
            StorefrontName = tenant.StorefrontName,
            BackofficeName = tenant.BackofficeName,
            StorefrontTagline = tenant.StorefrontTagline,
            LegalCompanyName = tenant.LegalCompanyName,
            SupportEmail = tenant.SupportEmail,
            SupportPhone = tenant.SupportPhone,
            SupportHours = tenant.SupportHours,
            SupportSla = tenant.SupportSla,
            PrimaryColor = tenant.PrimaryColor,
            AccentColor = tenant.AccentColor,
            AdminSidebarStartColor = tenant.AdminSidebarStartColor,
            AdminSidebarEndColor = tenant.AdminSidebarEndColor,
            LogoGlyph = tenant.LogoGlyph,
            Currency = tenant.Currency,
            Hostnames = tenant.Hostnames
                .Select(hostname => hostname.Hostname)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(hostname => hostname, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            CurrentPlan = currentPlan is null
                ? null
                : new TenantPlanDefinition
                {
                    PlanId = currentPlan.PlanId,
                    DisplayName = currentPlan.DisplayName,
                    Description = currentPlan.Description,
                    MonthlyPrice = currentPlan.MonthlyPrice,
                    Currency = currentPlan.Currency,
                    IncludedAdministrators = currentPlan.IncludedAdministrators,
                    IncludedProducts = currentPlan.IncludedProducts,
                    IncludedFeatureIds = currentPlan.PlanFeatures
                        .Where(planFeature => planFeature.Feature.Enabled)
                        .Select(planFeature => planFeature.FeatureId)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToArray()
                },
            Subscription = tenant.Subscription is null
                ? new TenantSubscriptionDefinition()
                : new TenantSubscriptionDefinition
                {
                    PlanId = tenant.Subscription.PlanId ?? string.Empty,
                    Status = tenant.Subscription.Status,
                    StartedAtUtc = tenant.Subscription.StartedAtUtc,
                    TrialEndsAtUtc = tenant.Subscription.TrialEndsAtUtc,
                    RenewalAtUtc = tenant.Subscription.RenewalAtUtc,
                    AutoRenew = tenant.Subscription.AutoRenew,
                    SeatsPurchased = tenant.Subscription.SeatsPurchased
                },
            Features = features,
            Provisioning = tenant.Provisioning is null
                ? new TenantProvisioningDefinition()
                : new TenantProvisioningDefinition
                {
                    BootstrapSuperUserEmail = tenant.Provisioning.BootstrapSuperUserEmail,
                    SeedBaseCategories = tenant.Provisioning.SeedBaseCategories,
                    SeedDemoCatalog = tenant.Provisioning.SeedDemoCatalog,
                    EnablePublicStorefront = tenant.Provisioning.EnablePublicStorefront,
                    Notes = tenant.Provisioning.Notes,
                    SuperUserProvisionedAtUtc = tenant.Provisioning.SuperUserProvisionedAtUtc,
                    BaseCategoriesProvisionedAtUtc = tenant.Provisioning.BaseCategoriesProvisionedAtUtc,
                    DemoCatalogProvisionedAtUtc = tenant.Provisioning.DemoCatalogProvisionedAtUtc
                }
        };
    }

    private static TenantDefinition MapConfiguredTenant(
        SaaSPlatformOptions.TenantOptions tenant,
        SaaSPlatformOptions options)
    {
        SaaSPlatformOptions.PlanOptions? currentPlan = options.Plans.FirstOrDefault(plan =>
            !string.IsNullOrWhiteSpace(tenant.PlanId)
            && string.Equals(plan.PlanId, tenant.PlanId, StringComparison.OrdinalIgnoreCase));

        HashSet<string> featureIds = new(StringComparer.OrdinalIgnoreCase);

        if (currentPlan is not null)
        {
            foreach (string featureId in currentPlan.IncludedFeatureIds.Where(value => !string.IsNullOrWhiteSpace(value)))
            {
                featureIds.Add(featureId.Trim());
            }
        }

        foreach (string featureId in tenant.EnabledFeatureIds.Where(value => !string.IsNullOrWhiteSpace(value)))
        {
            featureIds.Add(featureId.Trim());
        }

        TenantFeatureDefinition[] features = options.Features
            .Where(feature => featureIds.Contains(feature.FeatureId))
            .Select(feature => new TenantFeatureDefinition
            {
                FeatureId = feature.FeatureId,
                DisplayName = feature.DisplayName,
                Description = feature.Description,
                Category = feature.Category
            })
            .OrderBy(feature => feature.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new TenantDefinition
        {
            TenantId = tenant.TenantId.Trim(),
            DisplayName = tenant.DisplayName.Trim(),
            StorefrontName = tenant.StorefrontName.Trim(),
            BackofficeName = tenant.BackofficeName.Trim(),
            StorefrontTagline = tenant.StorefrontTagline.Trim(),
            LegalCompanyName = tenant.LegalCompanyName.Trim(),
            SupportEmail = tenant.SupportEmail.Trim(),
            SupportPhone = tenant.SupportPhone.Trim(),
            SupportHours = tenant.SupportHours.Trim(),
            SupportSla = tenant.SupportSla.Trim(),
            PrimaryColor = tenant.PrimaryColor.Trim(),
            AccentColor = tenant.AccentColor.Trim(),
            AdminSidebarStartColor = tenant.AdminSidebarStartColor.Trim(),
            AdminSidebarEndColor = tenant.AdminSidebarEndColor.Trim(),
            LogoGlyph = tenant.LogoGlyph.Trim(),
            Currency = tenant.Currency.Trim(),
            Hostnames = tenant.Hostnames
                .Where(hostname => !string.IsNullOrWhiteSpace(hostname))
                .Select(hostname => hostname.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            CurrentPlan = currentPlan is null
                ? null
                : new TenantPlanDefinition
                {
                    PlanId = currentPlan.PlanId,
                    DisplayName = currentPlan.DisplayName,
                    Description = currentPlan.Description,
                    MonthlyPrice = currentPlan.MonthlyPrice,
                    Currency = currentPlan.Currency,
                    IncludedAdministrators = currentPlan.IncludedAdministrators,
                    IncludedProducts = currentPlan.IncludedProducts,
                    IncludedFeatureIds = currentPlan.IncludedFeatureIds
                        .Where(featureId => !string.IsNullOrWhiteSpace(featureId))
                        .Select(featureId => featureId.Trim())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToArray()
                },
            Subscription = new TenantSubscriptionDefinition
            {
                PlanId = tenant.PlanId?.Trim() ?? string.Empty,
                Status = tenant.Subscription.Status.Trim(),
                StartedAtUtc = tenant.Subscription.StartedAtUtc,
                TrialEndsAtUtc = tenant.Subscription.TrialEndsAtUtc,
                RenewalAtUtc = tenant.Subscription.RenewalAtUtc,
                AutoRenew = tenant.Subscription.AutoRenew,
                SeatsPurchased = tenant.Subscription.SeatsPurchased
            },
            Features = features,
            Provisioning = new TenantProvisioningDefinition
            {
                BootstrapSuperUserEmail = tenant.Provisioning.BootstrapSuperUserEmail.Trim(),
                SeedBaseCategories = tenant.Provisioning.SeedBaseCategories,
                SeedDemoCatalog = tenant.Provisioning.SeedDemoCatalog,
                EnablePublicStorefront = tenant.Provisioning.EnablePublicStorefront,
                Notes = string.IsNullOrWhiteSpace(tenant.Provisioning.Notes) ? null : tenant.Provisioning.Notes.Trim()
            }
        };
    }
}
