using PlataformaECommerce.Application.Common.Execution;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Audit.DTOs;
using PlataformaECommerce.Application.Features.Audit.Queries;
using PlataformaECommerce.Application.Interfaces.Repositories.Audit;
using PlataformaECommerce.Application.Interfaces.Services.Audit;

namespace PlataformaECommerce.Application.Features.Audit.Services;

/// <summary>
/// Implementa los casos de uso de lectura asociados al rastro transversal de auditoría.
/// </summary>
/// <remarks>
/// Esta implementación coordina la validación básica de filtros, la consulta sobre el
/// repositorio documental y la proyección de eventos auditados hacia DTOs consumibles
/// por capas superiores como Razor Pages administrativas.
/// </remarks>
public sealed class AuditQueryApplicationService : IAuditApplicationService
{
    private const int DefaultPageNumber = 1;
    private const int DefaultPageSize = 25;
    private const int MaxPageSize = 100;
    private readonly IAuditRepository _auditRepository;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="AuditQueryApplicationService"/>.
    /// </summary>
    /// <param name="auditRepository">Repositorio transversal de auditoría.</param>
    public AuditQueryApplicationService(IAuditRepository auditRepository)
    {
        _auditRepository = auditRepository ?? throw new ArgumentNullException(nameof(auditRepository));
    }

    /// <inheritdoc />
    public async Task<Result<AuditQueryResultDto>> GetAuditTrailAsync(
        GetAuditTrailQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        Error? validationError = ValidateQuery(query);
        if (validationError is not null)
        {
            return Result.Failure<AuditQueryResultDto>(validationError);
        }

        return await ExecuteAsync(async () =>
        {
            AuditSearchFilter filter = new()
            {
                AggregateId = query.AggregateId,
                AggregateType = Normalize(query.AggregateType),
                Module = Normalize(query.Module),
                Action = Normalize(query.Action),
                PerformedBy = Normalize(query.PerformedBy),
                CorrelationId = Normalize(query.CorrelationId),
                FromUtc = query.FromUtc,
                ToUtc = query.ToUtc,
                PageNumber = NormalizePageNumber(query.PageNumber),
                PageSize = NormalizePageSize(query.PageSize),
                SortDescending = query.SortDescending
            };

            AuditSearchResult searchResult = await _auditRepository.SearchAsync(filter, cancellationToken);
            AuditEntryDto[] items = searchResult.Items.Select(MapToDto).ToArray();
            int totalPages = searchResult.TotalCount == 0
                ? 0
                : (int)Math.Ceiling(searchResult.TotalCount / (double)searchResult.PageSize);

            return Result.Success(new AuditQueryResultDto
            {
                Items = items,
                TotalCount = searchResult.TotalCount,
                ReturnedCount = items.Length,
                PageNumber = searchResult.PageNumber,
                PageSize = searchResult.PageSize,
                TotalPages = totalPages,
                HasPreviousPage = searchResult.PageNumber > 1 && totalPages > 0,
                HasNextPage = totalPages > 0 && searchResult.PageNumber < totalPages
            });
        }, "Audit.Query");
    }

    private static AuditEntryDto MapToDto(AuditEntry entry)
    {
        return new AuditEntryDto
        {
            AggregateId = entry.AggregateId,
            AggregateType = entry.AggregateType,
            Module = entry.Module,
            Action = entry.Action,
            Detail = entry.Detail,
            PerformedBy = entry.PerformedBy,
            PerformedByUserId = entry.PerformedByUserId,
            OccurredAtUtc = entry.OccurredAtUtc,
            CorrelationId = entry.CorrelationId,
            Source = entry.Source,
            Metadata = entry.Metadata is null
                ? new Dictionary<string, string>()
                : new Dictionary<string, string>(entry.Metadata, StringComparer.OrdinalIgnoreCase)
        };
    }

    private static int NormalizePageNumber(int pageNumber)
    {
        return pageNumber <= 0
            ? DefaultPageNumber
            : pageNumber;
    }

    private static int NormalizePageSize(int pageSize)
    {
        if (pageSize <= 0)
        {
            return DefaultPageSize;
        }

        return Math.Min(pageSize, MaxPageSize);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static Error? ValidateQuery(GetAuditTrailQuery query)
    {
        if (query.FromUtc.HasValue && query.FromUtc.Value.Kind != DateTimeKind.Utc)
        {
            return Error.Validation("Audit.InvalidFromUtc", "La fecha inicial debe estar expresada en UTC.");
        }

        if (query.ToUtc.HasValue && query.ToUtc.Value.Kind != DateTimeKind.Utc)
        {
            return Error.Validation("Audit.InvalidToUtc", "La fecha final debe estar expresada en UTC.");
        }

        if (query.FromUtc.HasValue && query.ToUtc.HasValue && query.FromUtc > query.ToUtc)
        {
            return Error.Validation("Audit.InvalidRange", "La fecha inicial no puede ser posterior a la fecha final.");
        }

        return null;
    }

    private static Task<Result<TResponse>> ExecuteAsync<TResponse>(
        Func<Task<Result<TResponse>>> operation,
        string errorCode)
    {
        return ApplicationExecution.ExecuteAsync(operation, errorCode);
    }
}
