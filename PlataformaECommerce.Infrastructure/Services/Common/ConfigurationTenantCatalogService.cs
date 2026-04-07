using Microsoft.Extensions.Options;
using PlataformaECommerce.Application.Common.SaaS;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Infrastructure.Configurations;

namespace PlataformaECommerce.Infrastructure.Services.Common;

/// <summary>
/// Proyecta el catálogo SaaS configurado en memoria hacia contratos estables consumibles por la aplicación.
/// </summary>
public sealed class ConfigurationTenantCatalogService : ITenantCatalogService
{
    private readonly IOptionsMonitor<SaaSPlatformOptions> _optionsMonitor;
    private readonly ITenantContextAccessor _tenantContextAccessor;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="ConfigurationTenantCatalogService"/>.
    /// </summary>
    public ConfigurationTenantCatalogService(
        IOptionsMonitor<SaaSPlatformOptions> optionsMonitor,
        ITenantContextAccessor tenantContextAccessor)
    {
        _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
        _tenantContextAccessor = tenantContextAccessor ?? throw new ArgumentNullException(nameof(tenantContextAccessor));
    }

    /// <inheritdoc />
    public string DataIsolationMode => _optionsMonitor.CurrentValue.DataIsolationMode;

    /// <inheritdoc />
    public Task<TenantDefinition> GetCurrentTenantAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        SaaSPlatformOptions options = _optionsMonitor.CurrentValue;
        string tenantId = _tenantContextAccessor.TenantId;
        SaaSPlatformOptions.TenantOptions tenant = options.Tenants.FirstOrDefault(current =>
            current.Enabled && string.Equals(current.TenantId, tenantId, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"No se encontró una definición SaaS habilitada para el tenant '{tenantId}'.");

        return Task.FromResult(MapTenant(tenant, options));
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<TenantDefinition>> GetConfiguredTenantsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        SaaSPlatformOptions options = _optionsMonitor.CurrentValue;

        IReadOnlyCollection<TenantDefinition> tenants = options.Tenants
            .Where(tenant => tenant.Enabled)
            .Select(tenant => MapTenant(tenant, options))
            .OrderBy(tenant => tenant.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return Task.FromResult(tenants);
    }

    private static TenantDefinition MapTenant(
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
                Notes = string.IsNullOrWhiteSpace(tenant.Provisioning.Notes) ? null : tenant.Provisioning.Notes.Trim(),
                SuperUserProvisionedAtUtc = null,
                BaseCategoriesProvisionedAtUtc = null,
                DemoCatalogProvisionedAtUtc = null
            }
        };
    }
}
