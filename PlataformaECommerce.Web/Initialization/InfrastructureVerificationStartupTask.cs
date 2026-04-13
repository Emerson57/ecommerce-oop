using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using PlataformaECommerce.Infrastructure.Mongo;
using PlataformaECommerce.Infrastructure.Persistence.Context;

namespace PlataformaECommerce.Web.Initialization;

internal sealed class InfrastructureVerificationStartupTask : IStartupInitializationTask
{
    private readonly ECommerceDbContext _dbContext;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<MongoDbSettings> _mongoDbSettings;
    private readonly ILogger<InfrastructureVerificationStartupTask> _logger;

    public InfrastructureVerificationStartupTask(
        ECommerceDbContext dbContext,
        IServiceProvider serviceProvider,
        IOptions<MongoDbSettings> mongoDbSettings,
        ILogger<InfrastructureVerificationStartupTask> logger)
    {
        ArgumentNullException.ThrowIfNull(mongoDbSettings);

        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _mongoDbSettings = mongoDbSettings;
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

        MongoDbSettings mongoDbSettings = _mongoDbSettings.Value;
        if (!mongoDbSettings.Enabled)
        {
            _logger.LogInformation("La verificación de infraestructura omitió MongoDB porque la auditoría documental está deshabilitada.");
            return;
        }

        IMongoDatabase? mongoDatabase = _serviceProvider.GetService<IMongoDatabase>();
        if (mongoDatabase is null)
        {
            throw new InvalidOperationException("La verificación de infraestructura requiere un `IMongoDatabase` cuando MongoDB está habilitado.");
        }

        await mongoDatabase.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
