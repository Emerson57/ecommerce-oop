using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlataformaECommerce.Infrastructure.Persistence.Context;

namespace PlataformaECommerce.Web.Initialization;

internal sealed class InfrastructureVerificationStartupTask : IStartupInitializationTask
{
    private readonly ECommerceDbContext _dbContext;
    private readonly ILogger<InfrastructureVerificationStartupTask> _logger;

    public InfrastructureVerificationStartupTask(
        ECommerceDbContext dbContext,
        ILogger<InfrastructureVerificationStartupTask> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "InfrastructureVerification";

    public StartupInitializationCategory Category => StartupInitializationCategory.InfrastructureVerification;

    public StartupInitializationExecutionMode ExecutionMode => StartupInitializationExecutionMode.Always;

    public int Order => 100;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        bool sqlAvailable = await _dbContext.Database.CanConnectAsync(cancellationToken).ConfigureAwait(false);
        if (!sqlAvailable)
        {
            throw new InvalidOperationException("La verificación de infraestructura no pudo conectarse a SQL Server durante el arranque del host web.");
        }
        _logger.LogInformation("La verificación de infraestructura confirmó conectividad SQL Server.");
    }
}
