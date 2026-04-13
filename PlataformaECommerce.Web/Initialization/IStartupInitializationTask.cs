namespace PlataformaECommerce.Web.Initialization;

internal interface IStartupInitializationTask
{
    string Name { get; }

    StartupInitializationCategory Category { get; }

    StartupInitializationExecutionMode ExecutionMode { get; }

    int Order { get; }

    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
