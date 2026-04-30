using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PlataformaECommerce.Application.Interfaces.Repositories.Audit;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Infrastructure.Persistence.Context;
using PlataformaECommerce.Infrastructure.Persistence.Entities;

namespace PlataformaECommerce.Infrastructure.Repositories.Audit;

/// <summary>
/// Implementa el repositorio transversal de auditoría sobre SQL Server.
/// </summary>
public sealed class SqlAuditRepository : IAuditRepository
{
    private static readonly JsonSerializerOptions MetadataSerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly ECommerceDbContext _context;
    private readonly ITenantContextAccessor _tenantContextAccessor;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="SqlAuditRepository"/>.
    /// </summary>
    public SqlAuditRepository(
        ECommerceDbContext context,
        ITenantContextAccessor tenantContextAccessor)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _tenantContextAccessor = tenantContextAccessor ?? throw new ArgumentNullException(nameof(tenantContextAccessor));
    }

    /// <inheritdoc />
    public async Task RegisterEventAsync(AuditEntry entry, CancellationToken cancellationToken = default)
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

        AuditEntryEntity entity = MapToEntity(entry, _tenantContextAccessor.TenantId);
        await _context.AuditEntries.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
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

        List<AuditEntryEntity> entities = await _context.AuditEntries
            .AsNoTracking()
            .Where(entry => entry.AggregateId == aggregateId && entry.AggregateType == aggregateType.Trim())
            .OrderByDescending(entry => entry.OccurredAtUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDomain).ToArray();
    }

    /// <inheritdoc />
    public async Task<AuditSearchResult> SearchAsync(
        AuditSearchFilter filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        IQueryable<AuditEntryEntity> query = _context.AuditEntries
            .AsNoTracking();

        if (filter.AggregateId.HasValue && filter.AggregateId.Value != Guid.Empty)
        {
            Guid aggregateId = filter.AggregateId.Value;
            query = query.Where(entry => entry.AggregateId == aggregateId);
        }

        if (!string.IsNullOrWhiteSpace(filter.AggregateType))
        {
            string aggregateType = filter.AggregateType.Trim();
            query = query.Where(entry => entry.AggregateType == aggregateType);
        }

        if (!string.IsNullOrWhiteSpace(filter.Module))
        {
            string module = filter.Module.Trim();
            query = query.Where(entry => entry.Module == module);
        }

        if (!string.IsNullOrWhiteSpace(filter.Action))
        {
            string action = filter.Action.Trim();
            query = query.Where(entry => entry.Action == action);
        }

        if (!string.IsNullOrWhiteSpace(filter.PerformedBy))
        {
            string performedBy = filter.PerformedBy.Trim();
            query = query.Where(entry => entry.PerformedBy == performedBy);
        }

        if (!string.IsNullOrWhiteSpace(filter.CorrelationId))
        {
            string correlationId = filter.CorrelationId.Trim();
            query = query.Where(entry => entry.CorrelationId == correlationId);
        }

        if (filter.FromUtc.HasValue)
        {
            DateTime fromUtc = filter.FromUtc.Value;
            query = query.Where(entry => entry.OccurredAtUtc >= fromUtc);
        }

        if (filter.ToUtc.HasValue)
        {
            DateTime toUtc = filter.ToUtc.Value;
            query = query.Where(entry => entry.OccurredAtUtc <= toUtc);
        }

        query = filter.SortDescending
            ? query.OrderByDescending(entry => entry.OccurredAtUtc)
            : query.OrderBy(entry => entry.OccurredAtUtc);

        int pageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
        int pageSize = filter.PageSize <= 0 ? 25 : filter.PageSize;
        int skip = (pageNumber - 1) * pageSize;

        int totalCount = await query.CountAsync(cancellationToken);
        List<AuditEntryEntity> entities = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new AuditSearchResult
        {
            Items = entities.Select(MapToDomain).ToArray(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    private static AuditEntryEntity MapToEntity(AuditEntry entry, string currentTenantId)
    {
        IReadOnlyDictionary<string, string> metadata = entry.Metadata
            ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        return new AuditEntryEntity
        {
            Id = Guid.NewGuid(),
            TenantId = string.IsNullOrWhiteSpace(entry.TenantId) ? currentTenantId : entry.TenantId.Trim(),
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
            MetadataJson = JsonSerializer.Serialize(metadata, MetadataSerializerOptions)
        };
    }

    private static AuditEntry MapToDomain(AuditEntryEntity entity)
    {
        return new AuditEntry
        {
            AggregateId = entity.AggregateId,
            AggregateType = entity.AggregateType,
            Module = entity.Module,
            Action = entity.Action,
            Detail = entity.Detail,
            PerformedBy = entity.PerformedBy,
            PerformedByUserId = entity.PerformedByUserId,
            OccurredAtUtc = entity.OccurredAtUtc,
            CorrelationId = entity.CorrelationId,
            TenantId = entity.TenantId,
            Source = entity.Source,
            Metadata = DeserializeMetadata(entity.MetadataJson)
        };
    }

    private static IReadOnlyDictionary<string, string> DeserializeMetadata(string metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        Dictionary<string, string>? metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(metadataJson, MetadataSerializerOptions);
        return metadata is null
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(metadata, StringComparer.OrdinalIgnoreCase);
    }
}
