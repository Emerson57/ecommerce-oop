using System.Threading;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PlataformaECommerce.Application.Interfaces.Repositories.Audit;
using PlataformaECommerce.Infrastructure.Mongo;
using PlataformaECommerce.Infrastructure.Mongo.Repositories.Audit;

namespace PlataformaECommerce.Infrastructure.Mongo.Repositories;

/// <summary>
/// Implementa el repositorio transversal de auditoría sobre MongoDB.
/// </summary>
/// <remarks>
/// Esta implementación persiste eventos de trazabilidad de productos, usuarios,
/// carritos y pedidos en una colección documental compartida, permitiendo análisis
/// históricos y correlación transversal sin afectar las operaciones OLTP principales.
/// </remarks>
public sealed class MongoAuditRepository : IAuditRepository
{
    private static int _indexesInitialized;
    private readonly IMongoCollection<AuditDocument> _collection;

    /// <summary>
    /// Inicializa una nueva instancia del repositorio transversal de auditoría.
    /// </summary>
    /// <param name="database">Base de datos MongoDB asociada a auditoría.</param>
    /// <param name="settingsOptions">Opciones tipadas de MongoDB.</param>
    public MongoAuditRepository(
        IMongoDatabase database,
        IOptions<MongoDbSettings> settingsOptions)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(settingsOptions);

        MongoDbSettings settings = settingsOptions.Value;
        if (string.IsNullOrWhiteSpace(settings.AuditCollectionName))
        {
            throw new InvalidOperationException("La configuración de MongoDB debe definir el nombre de la colección de auditoría transversal.");
        }

        _collection = database.GetCollection<AuditDocument>(settings.AuditCollectionName);

        if (settings.EnsureIndexesOnStartup)
        {
            EnsureIndexes();
        }
    }

    /// <inheritdoc />
    public Task RegisterEventAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (entry.AggregateId == Guid.Empty)
        {
            throw new ArgumentException("El identificador del agregado auditado es obligatorio.", nameof(entry));
        }

        if (string.IsNullOrWhiteSpace(entry.AggregateType))
        {
            throw new ArgumentException("El tipo de agregado auditado es obligatorio.", nameof(entry));
        }

        if (string.IsNullOrWhiteSpace(entry.Module))
        {
            throw new ArgumentException("El módulo auditado es obligatorio.", nameof(entry));
        }

        if (string.IsNullOrWhiteSpace(entry.Action))
        {
            throw new ArgumentException("La acción auditada es obligatoria.", nameof(entry));
        }

        if (string.IsNullOrWhiteSpace(entry.Detail))
        {
            throw new ArgumentException("El detalle del evento auditado es obligatorio.", nameof(entry));
        }

        if (string.IsNullOrWhiteSpace(entry.PerformedBy))
        {
            throw new ArgumentException("El actor responsable del evento auditado es obligatorio.", nameof(entry));
        }

        AuditDocument document = new()
        {
            AggregateId = entry.AggregateId,
            AggregateType = entry.AggregateType.Trim(),
            Module = entry.Module.Trim(),
            Action = entry.Action.Trim(),
            Detail = entry.Detail.Trim(),
            PerformedBy = entry.PerformedBy.Trim(),
            PerformedByUserId = string.IsNullOrWhiteSpace(entry.PerformedByUserId) ? null : entry.PerformedByUserId.Trim(),
            OccurredAtUtc = entry.OccurredAtUtc == default ? DateTime.UtcNow : entry.OccurredAtUtc,
            CorrelationId = string.IsNullOrWhiteSpace(entry.CorrelationId) ? null : entry.CorrelationId.Trim(),
            Source = string.IsNullOrWhiteSpace(entry.Source) ? null : entry.Source.Trim(),
            Metadata = entry.Metadata?.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase)
                ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        };

        return _collection.InsertOneAsync(document, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<AuditEntry>> GetHistoryAsync(
        Guid aggregateId,
        string aggregateType,
        CancellationToken cancellationToken = default)
    {
        if (aggregateId == Guid.Empty)
        {
            throw new ArgumentException("El identificador del agregado auditado es obligatorio.", nameof(aggregateId));
        }

        if (string.IsNullOrWhiteSpace(aggregateType))
        {
            throw new ArgumentException("El tipo de agregado auditado es obligatorio.", nameof(aggregateType));
        }

        List<AuditDocument> documents = await _collection
            .Find(document => document.AggregateId == aggregateId && document.AggregateType == aggregateType)
            .SortByDescending(document => document.OccurredAtUtc)
            .ToListAsync(cancellationToken);

        return documents.Select(MapToEntry).ToArray();
    }

    /// <inheritdoc />
    public async Task<AuditSearchResult> SearchAsync(
        AuditSearchFilter filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        FilterDefinitionBuilder<AuditDocument> filters = Builders<AuditDocument>.Filter;
        List<FilterDefinition<AuditDocument>> filterDefinitions = new();

        if (filter.AggregateId.HasValue && filter.AggregateId.Value != Guid.Empty)
        {
            filterDefinitions.Add(filters.Eq(document => document.AggregateId, filter.AggregateId.Value));
        }

        if (!string.IsNullOrWhiteSpace(filter.AggregateType))
        {
            filterDefinitions.Add(filters.Eq(document => document.AggregateType, filter.AggregateType.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(filter.Module))
        {
            filterDefinitions.Add(filters.Eq(document => document.Module, filter.Module.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(filter.Action))
        {
            filterDefinitions.Add(filters.Eq(document => document.Action, filter.Action.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(filter.PerformedBy))
        {
            filterDefinitions.Add(filters.Eq(document => document.PerformedBy, filter.PerformedBy.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(filter.CorrelationId))
        {
            filterDefinitions.Add(filters.Eq(document => document.CorrelationId, filter.CorrelationId.Trim()));
        }

        if (filter.FromUtc.HasValue)
        {
            filterDefinitions.Add(filters.Gte(document => document.OccurredAtUtc, filter.FromUtc.Value));
        }

        if (filter.ToUtc.HasValue)
        {
            filterDefinitions.Add(filters.Lte(document => document.OccurredAtUtc, filter.ToUtc.Value));
        }

        FilterDefinition<AuditDocument> finalFilter = filterDefinitions.Count == 0
            ? filters.Empty
            : filters.And(filterDefinitions);

        IFindFluent<AuditDocument, AuditDocument> query = _collection.Find(finalFilter);

        query = filter.SortDescending
            ? query.SortByDescending(document => document.OccurredAtUtc)
            : query.SortBy(document => document.OccurredAtUtc);

        int pageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
        int pageSize = filter.PageSize <= 0 ? 25 : filter.PageSize;
        int skip = (pageNumber - 1) * pageSize;

        long totalCount = await _collection.CountDocumentsAsync(finalFilter, cancellationToken: cancellationToken);

        List<AuditDocument> documents = await query
            .Skip(skip)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return new AuditSearchResult
        {
            Items = documents.Select(MapToEntry).ToArray(),
            TotalCount = (int)totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    private static AuditEntry MapToEntry(AuditDocument document)
    {
        return new AuditEntry
        {
            AggregateId = document.AggregateId,
            AggregateType = document.AggregateType,
            Module = document.Module,
            Action = document.Action,
            Detail = document.Detail,
            PerformedBy = document.PerformedBy,
            PerformedByUserId = document.PerformedByUserId,
            OccurredAtUtc = document.OccurredAtUtc,
            CorrelationId = document.CorrelationId,
            Source = document.Source,
            Metadata = document.Metadata
        };
    }

    private void EnsureIndexes()
    {
        if (Interlocked.Exchange(ref _indexesInitialized, 1) == 1)
        {
            return;
        }

        CreateIndexModel<AuditDocument>[] indexes =
        {
            new(Builders<AuditDocument>.IndexKeys.Ascending(document => document.AggregateId)),
            new(Builders<AuditDocument>.IndexKeys
                .Ascending(document => document.AggregateType)
                .Ascending(document => document.AggregateId)),
            new(Builders<AuditDocument>.IndexKeys.Ascending(document => document.Module)),
            new(Builders<AuditDocument>.IndexKeys.Descending(document => document.OccurredAtUtc)),
            new(Builders<AuditDocument>.IndexKeys
                .Ascending(document => document.AggregateType)
                .Ascending(document => document.AggregateId)
                .Descending(document => document.OccurredAtUtc)),
            new(Builders<AuditDocument>.IndexKeys.Ascending(document => document.CorrelationId)),
            new(Builders<AuditDocument>.IndexKeys.Ascending(document => document.PerformedBy)),
            new(Builders<AuditDocument>.IndexKeys.Ascending(document => document.Action))
        };

        _collection.Indexes.CreateMany(indexes);
    }
}
