using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlataformaECommerce.Web.Initialization;

namespace PlataformaECommerce.Maintenance;

internal static class SaaSBootstrapMaintenanceCommands
{
    public const string BootstrapStatus = "readiness/bootstrap-status";
    public const string SyncSaaSCatalog = "sync-saas-catalog";
    public const string SeedConfiguredTenants = "seed-configured-tenants";
    public const string BootstrapSuperUser = "bootstrap-superuser";
    public const string RunSaaSBootstrap = "run-saas-bootstrap";

    public static bool IsSupported(string commandName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandName);

        return string.Equals(commandName, SyncSaaSCatalog, StringComparison.Ordinal)
            || string.Equals(commandName, BootstrapStatus, StringComparison.Ordinal)
            || string.Equals(commandName, SeedConfiguredTenants, StringComparison.Ordinal)
            || string.Equals(commandName, BootstrapSuperUser, StringComparison.Ordinal)
            || string.Equals(commandName, RunSaaSBootstrap, StringComparison.Ordinal);
    }

    public static bool RequiresExclusiveLock(string commandName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandName);
        return IsSupported(commandName)
            && !string.Equals(commandName, BootstrapStatus, StringComparison.Ordinal);
    }

    public static string GetLockResource(MaintenanceCommandRequest commandRequest)
    {
        ArgumentNullException.ThrowIfNull(commandRequest);

        return commandRequest.CommandName switch
        {
            BootstrapStatus => throw new InvalidOperationException($"El comando SaaS '{commandRequest.CommandName}' no requiere lock exclusivo."),
            SyncSaaSCatalog => "maintenance:saas-catalog-sync",
            SeedConfiguredTenants => "maintenance:seed-configured-tenants",
            BootstrapSuperUser => BuildTenantScopedResource("maintenance:bootstrap-superuser", commandRequest.TenantOverride),
            RunSaaSBootstrap => BuildTenantScopedResource("maintenance:run-saas-bootstrap", commandRequest.TenantOverride),
            _ => throw new InvalidOperationException($"El comando SaaS '{commandRequest.CommandName}' no está soportado.")
        };
    }

    public static async Task ExecuteAsync(
        IServiceProvider services,
        ILogger logger,
        MaintenanceCommandRequest commandRequest,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(commandRequest);

        ConfiguredTenantProvisioningService configuredTenantProvisioningService = services.GetRequiredService<ConfiguredTenantProvisioningService>();
        SaaSBootstrapStatusInspectionService statusInspectionService = services.GetRequiredService<SaaSBootstrapStatusInspectionService>();
        SuperUserBootstrapService superUserBootstrapService = services.GetRequiredService<SuperUserBootstrapService>();

        switch (commandRequest.CommandName)
        {
            case BootstrapStatus:
                await ReportBootstrapStatusAsync(statusInspectionService, logger, commandRequest, cancellationToken).ConfigureAwait(false);
                return;
            case SyncSaaSCatalog:
                await configuredTenantProvisioningService.SynchronizeConfiguredCatalogAsync(cancellationToken).ConfigureAwait(false);
                return;
            case SeedConfiguredTenants:
                await configuredTenantProvisioningService.ProvisionConfiguredTenantsAsync(cancellationToken).ConfigureAwait(false);
                return;
            case BootstrapSuperUser:
                await superUserBootstrapService.BootstrapAsync(cancellationToken).ConfigureAwait(false);
                return;
            case RunSaaSBootstrap:
                await configuredTenantProvisioningService.SynchronizeConfiguredCatalogAsync(cancellationToken).ConfigureAwait(false);
                await configuredTenantProvisioningService.ProvisionConfiguredTenantsAsync(cancellationToken).ConfigureAwait(false);
                await superUserBootstrapService.BootstrapAsync(cancellationToken).ConfigureAwait(false);
                return;
            default:
                throw new InvalidOperationException($"El comando SaaS '{commandRequest.CommandName}' no está soportado.");
        }
    }

    public static void WriteHelp(TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteLine($"  {BootstrapStatus} [--tenant=<tenantId>]         Inspecciona el estado de sync, seed y superusuario sin mutar datos.");
        writer.WriteLine($"  {SyncSaaSCatalog} [--tenant=<tenantId>]         Sincroniza el catálogo SaaS persistente desde configuración.");
        writer.WriteLine($"  {SeedConfiguredTenants} [--tenant=<tenantId>]   Ejecuta la siembra funcional configurada para tenants.");
        writer.WriteLine($"  {BootstrapSuperUser} [--tenant=<tenantId>]      Ejecuta el bootstrap explícito del superusuario inicial.");
        writer.WriteLine($"  {RunSaaSBootstrap} [--tenant=<tenantId>]        Ejecuta sync, seed funcional y bootstrap en una sola operación protegida.");
    }

    private static async Task ReportBootstrapStatusAsync(
        SaaSBootstrapStatusInspectionService statusInspectionService,
        ILogger logger,
        MaintenanceCommandRequest commandRequest,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(statusInspectionService);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(commandRequest);

        IReadOnlyCollection<SaaSBootstrapStatusResult> results = await statusInspectionService
            .InspectAsync(commandRequest.TenantOverride, cancellationToken)
            .ConfigureAwait(false);

        if (results.Count == 0)
        {
            logger.LogWarning(
                "El comando de bootstrap status no encontró tenants configurados que coincidan con el filtro actual. TenantOverride: {TenantOverride}.",
                commandRequest.TenantOverride ?? "<none>");
            return;
        }

        foreach (SaaSBootstrapStatusResult result in results)
        {
            logger.LogInformation(
                "Bootstrap status tenant '{TenantId}' ({DisplayName}). PersistedTenantExists: {PersistedTenantExists}. ProvisioningStateExists: {ProvisioningStateExists}. CatalogSynchronizedAtUtc: {CatalogSynchronizedAtUtc}. BaseCategoriesRequired: {BaseCategoriesRequired}. BaseCategoriesProvisioned: {BaseCategoriesProvisioned}. DemoCatalogRequired: {DemoCatalogRequired}. DemoCatalogProvisioned: {DemoCatalogProvisioned}. ExpectedSuperUserEmail: {ExpectedSuperUserEmail}. SuperUserProvisioned: {SuperUserProvisioned}. BootstrapReady: {BootstrapReady}.",
                result.TenantId,
                result.DisplayName,
                result.PersistedTenantExists,
                result.ProvisioningStateExists,
                result.CatalogSynchronizedAtUtc,
                result.BaseCategoriesRequired,
                result.BaseCategoriesProvisioned,
                result.DemoCatalogRequired,
                result.DemoCatalogProvisioned,
                result.ExpectedSuperUserEmail,
                result.SuperUserProvisioned,
                result.BootstrapReady);
        }
    }

    private static string BuildTenantScopedResource(string prefix, string? tenantOverride)
    {
        string normalizedTenant = string.IsNullOrWhiteSpace(tenantOverride)
            ? "all-tenants"
            : tenantOverride.Trim().ToLowerInvariant();

        return $"{prefix}:{normalizedTenant}";
    }
}
