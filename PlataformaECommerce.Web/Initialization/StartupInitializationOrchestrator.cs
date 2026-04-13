using Microsoft.Extensions.Logging;

namespace PlataformaECommerce.Web.Initialization;

internal sealed class StartupInitializationOrchestrator
{
    private readonly IReadOnlyCollection<IStartupInitializationTask> _tasks;
    private readonly ILogger<StartupInitializationOrchestrator> _logger;

    public StartupInitializationOrchestrator(
        IEnumerable<IStartupInitializationTask> tasks,
        ILogger<StartupInitializationOrchestrator> logger)
    {
        ArgumentNullException.ThrowIfNull(tasks);

        _tasks = tasks.ToArray();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        foreach (IStartupInitializationTask task in _tasks
                     .OrderBy(task => task.Category)
                     .ThenBy(task => task.Order)
                     .ThenBy(task => task.Name, StringComparer.Ordinal))
        {
            _logger.LogInformation(
                "Ejecutando tarea de startup '{TaskName}'. Categoria: {Category}. Modo: {ExecutionMode}.",
                task.Name,
                task.Category,
                task.ExecutionMode);

            await task.ExecuteAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation(
                "Finalizó la tarea de startup '{TaskName}'. Categoria: {Category}.",
                task.Name,
                task.Category);
        }
    }
}
