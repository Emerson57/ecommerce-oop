namespace PlataformaECommerce.Web.Initialization;

internal sealed class SuperUserBootstrapStartupTask : IStartupInitializationTask
{
    private readonly SuperUserBootstrapService _superUserBootstrapService;

    public SuperUserBootstrapStartupTask(SuperUserBootstrapService superUserBootstrapService)
    {
        _superUserBootstrapService = superUserBootstrapService ?? throw new ArgumentNullException(nameof(superUserBootstrapService));
    }

    public string Name => "SuperUserBootstrap";

    public StartupInitializationCategory Category => StartupInitializationCategory.BootstrapUnique;

    public StartupInitializationExecutionMode ExecutionMode => StartupInitializationExecutionMode.OneTimeIdempotent;

    public int Order => 300;

    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return _superUserBootstrapService.BootstrapAsync(cancellationToken);
    }
}
