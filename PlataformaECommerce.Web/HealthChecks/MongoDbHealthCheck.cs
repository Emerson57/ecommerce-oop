using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using PlataformaECommerce.Infrastructure.Mongo;

namespace PlataformaECommerce.Web.HealthChecks;

/// <summary>
/// Verifica la disponibilidad operativa del almacenamiento de auditoría MongoDB.
/// </summary>
public sealed class MongoDbHealthCheck : IHealthCheck
{
    private readonly MongoDbSettings _settings;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="MongoDbHealthCheck"/>.
    /// </summary>
    public MongoDbHealthCheck(IOptions<MongoDbSettings> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _settings = options.Value;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
        {
            return HealthCheckResult.Healthy("La auditoría MongoDB está deshabilitada para este entorno.");
        }

        try
        {
            MongoClient client = new(_settings.ConnectionString);
            IMongoDatabase database = client.GetDatabase(_settings.DatabaseName);
            BsonDocument response = await database
                .RunCommandAsync((Command<BsonDocument>)"{ ping: 1 }", cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return response.Contains("ok") && response["ok"].ToDouble() >= 1d
                ? HealthCheckResult.Healthy("MongoDB respondió satisfactoriamente al comando ping.")
                : HealthCheckResult.Unhealthy("MongoDB respondió sin confirmar un estado saludable.");
        }
        catch (TimeoutException exception)
        {
            return HealthCheckResult.Unhealthy("MongoDB no respondió dentro del tiempo esperado.", exception);
        }
        catch (MongoException exception)
        {
            return HealthCheckResult.Unhealthy("MongoDB no está disponible para la auditoría operacional.", exception);
        }
    }
}
