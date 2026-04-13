namespace PlataformaECommerce.Web.Initialization;

internal sealed class TenantProvisioningStartupTask : IStartupInitializationTask
{
    private readonly ConfiguredTenantProvisioningService _configuredTenantProvisioningService;

    public TenantProvisioningStartupTask(ConfiguredTenantProvisioningService configuredTenantProvisioningService)
    {
        _configuredTenantProvisioningService = configuredTenantProvisioningService ?? throw new ArgumentNullException(nameof(configuredTenantProvisioningService));
    }

    public string Name => "TenantProvisioning";

    public StartupInitializationCategory Category => StartupInitializationCategory.BootstrapUnique;

    public StartupInitializationExecutionMode ExecutionMode => StartupInitializationExecutionMode.OneTimeIdempotent;

    public int Order => 200;

    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return _configuredTenantProvisioningService.ProvisionConfiguredTenantsAsync(cancellationToken);
    }
}
