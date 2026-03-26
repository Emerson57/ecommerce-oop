using PlataformaECommerce.Application.Features.Audit.Services;
using PlataformaECommerce.Application.Interfaces.Repositories.Audit;
using PlataformaECommerce.Application.Interfaces.Services.Audit;
using PlataformaECommerce.Application.Interfaces.Services.Common;

namespace PlataformaECommerce.Tests.Application.Audit;

[TestFixture]
public class AuditTrailApplicationServiceTests
{
    [Test]
    public async Task RegisterAsync_UsuarioConCorreo_RegistraActorEsperado()
    {
        FakeAuditRepository auditRepository = new();
        AuditTrailApplicationService service = new(
            auditRepository,
            new FakeCurrentUserService("auditor@plataforma.com", "auditor"),
            new FakeExecutionContextAccessor("trace-audit"));

        await service.RegisterAsync(Guid.NewGuid(), "Product", "Products", "product.created", "Evento de prueba.");

        Assert.That(auditRepository.LastEntry?.PerformedBy, Is.EqualTo("auditor@plataforma.com"));
    }

    [Test]
    public async Task RegisterAsync_SinUsuarioAutenticado_RegistraActorSystem()
    {
        FakeAuditRepository auditRepository = new();
        AuditTrailApplicationService service = new(
            auditRepository,
            new FakeCurrentUserService(null, null),
            new FakeExecutionContextAccessor("trace-audit"));

        await service.RegisterAsync(Guid.NewGuid(), "Cart", "Cart", "cart.created", "Evento de prueba.");

        Assert.That(auditRepository.LastEntry?.PerformedBy, Is.EqualTo("system"));
    }

    private sealed class FakeAuditRepository : IAuditRepository
    {
        public AuditEntry? LastEntry { get; private set; }

        public Task RegisterEventAsync(AuditEntry entry, CancellationToken cancellationToken = default)
        {
            LastEntry = entry;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<AuditEntry>> GetHistoryAsync(Guid aggregateId, string aggregateType, CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<AuditEntry> entries = LastEntry is null ? Array.Empty<AuditEntry>() : new[] { LastEntry };
            return Task.FromResult(entries);
        }

        public Task<AuditSearchResult> SearchAsync(AuditSearchFilter filter, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new AuditSearchResult
            {
                Items = LastEntry is null ? Array.Empty<AuditEntry>() : new[] { LastEntry },
                TotalCount = LastEntry is null ? 0 : 1,
                PageNumber = 1,
                PageSize = 25
            });
        }
    }

    private sealed class FakeCurrentUserService : ICurrentUserService
    {
        private readonly string? _email;
        private readonly string? _userName;

        public FakeCurrentUserService(string? email, string? userName)
        {
            _email = email;
            _userName = userName;
        }

        public Guid? UserId => _email is null && _userName is null ? null : Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        public string? UserName => _userName;
        public string? Email => _email;
        public string? Role => "Administrador";
        public bool IsAuthenticated => _email is not null || _userName is not null;

        public bool IsInRole(string role)
        {
            return string.Equals(role, Role, StringComparison.OrdinalIgnoreCase);
        }

        public string? GetClaimValue(string claimType)
        {
            return null;
        }

        public IReadOnlyCollection<string> GetClaimValues(string claimType)
        {
            return Array.Empty<string>();
        }
    }

    private sealed class FakeExecutionContextAccessor : IExecutionContextAccessor
    {
        public FakeExecutionContextAccessor(string correlationId)
        {
            CorrelationId = correlationId;
        }

        public string? CorrelationId { get; }
    }
}
