using PlataformaECommerce.Application.Interfaces.Repositories.Audit;
using PlataformaECommerce.Application.Interfaces.Services.Audit;
using PlataformaECommerce.Application.Interfaces.Services.Common;

namespace PlataformaECommerce.Application.Features.Audit.Services;

/// <summary>
/// Implementa el servicio de aplicación responsable de construir y registrar
/// trazas de auditoría homogéneas para los distintos módulos funcionales.
/// </summary>
/// <remarks>
/// Esta implementación centraliza la resolución del actor responsable, el identificador
/// de correlación y el origen técnico del evento, permitiendo que otros servicios de
/// aplicación se concentren únicamente en la semántica del cambio auditado.
/// </remarks>
public sealed class AuditTrailApplicationService : IAuditTrailService
{
    private readonly IAuditRepository _auditRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly ITenantContextAccessor _tenantContextAccessor;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="AuditTrailApplicationService"/>.
    /// </summary>
    /// <param name="auditRepository">Repositorio de auditoría transversal.</param>
    /// <param name="currentUserService">Servicio de usuario actual.</param>
    /// <param name="executionContextAccessor">Servicio de contexto de ejecución.</param>
    public AuditTrailApplicationService(
        IAuditRepository auditRepository,
        ICurrentUserService currentUserService,
        IExecutionContextAccessor executionContextAccessor,
        ITenantContextAccessor tenantContextAccessor)
    {
        _auditRepository = auditRepository ?? throw new ArgumentNullException(nameof(auditRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _executionContextAccessor = executionContextAccessor ?? throw new ArgumentNullException(nameof(executionContextAccessor));
        _tenantContextAccessor = tenantContextAccessor ?? throw new ArgumentNullException(nameof(tenantContextAccessor));
    }

    /// <inheritdoc />
    public Task RegisterAsync(
        Guid aggregateId,
        string aggregateType,
        string module,
        string action,
        string detail,
        IReadOnlyDictionary<string, string>? metadata = null,
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

        if (string.IsNullOrWhiteSpace(module))
        {
            throw new ArgumentException("El módulo auditado es obligatorio.", nameof(module));
        }

        if (string.IsNullOrWhiteSpace(action))
        {
            throw new ArgumentException("La acción auditada es obligatoria.", nameof(action));
        }

        if (string.IsNullOrWhiteSpace(detail))
        {
            throw new ArgumentException("El detalle del evento auditado es obligatorio.", nameof(detail));
        }

        AuditEntry entry = new()
        {
            AggregateId = aggregateId,
            AggregateType = aggregateType.Trim(),
            Module = module.Trim(),
            Action = action.Trim(),
            Detail = detail.Trim(),
            PerformedBy = ResolvePerformedBy(),
            PerformedByUserId = _currentUserService.UserId?.ToString(),
            OccurredAtUtc = DateTime.UtcNow,
            CorrelationId = _executionContextAccessor.CorrelationId,
            TenantId = _tenantContextAccessor.TenantId,
            Source = $"Application.{module.Trim()}",
            Metadata = metadata
        };

        return _auditRepository.RegisterEventAsync(entry, cancellationToken);
    }

    private string ResolvePerformedBy()
    {
        if (!string.IsNullOrWhiteSpace(_currentUserService.Email))
        {
            return _currentUserService.Email;
        }

        if (!string.IsNullOrWhiteSpace(_currentUserService.UserName))
        {
            return _currentUserService.UserName;
        }

        return "system";
    }
}
