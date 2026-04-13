namespace PlataformaECommerce.Web.Initialization;

internal sealed class TenantCatalogSynchronizationStartupTask : IStartupInitializationTask
{
    private readonly ConfiguredTenantProvisioningService _configuredTenantProvisioningService;

    public TenantCatalogSynchronizationStartupTask(ConfiguredTenantProvisioningService configuredTenantProvisioningService)
    {
        _configuredTenantProvisioningService = configuredTenantProvisioningService ?? throw new ArgumentNullException(nameof(configuredTenantProvisioningService));
    }

    public string Name => "TenantCatalogSynchronization";

    public StartupInitializationCategory Category => StartupInitializationCategory.BootstrapUnique;

    public StartupInitializationExecutionMode ExecutionMode => StartupInitializationExecutionMode.OneTimeIdempotent;

    public int Order => 100;

    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return _configuredTenantProvisioningService.SynchronizeConfiguredCatalogAsync(cancellationToken);
    }
}
