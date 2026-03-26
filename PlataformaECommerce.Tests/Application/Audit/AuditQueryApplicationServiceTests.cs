using PlataformaECommerce.Application.Features.Audit.Queries;
using PlataformaECommerce.Application.Features.Audit.Services;
using PlataformaECommerce.Application.Interfaces.Repositories.Audit;

namespace PlataformaECommerce.Tests.Application.Audit;

[TestFixture]
public class AuditQueryApplicationServiceTests
{
    [Test]
    public async Task GetAuditTrailAsync_FiltroValido_RetornaEventosMapeados()
    {
        FakeAuditRepository repository = new();
        AuditQueryApplicationService service = new(repository);

        var result = await service.GetAuditTrailAsync(new GetAuditTrailQuery
        {
            Module = "Products",
            PageSize = 10
        });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.ReturnedCount, Is.EqualTo(1));
    }

    [Test]
    public async Task GetAuditTrailAsync_RangoInvalido_RetornaFalloControlado()
    {
        FakeAuditRepository repository = new();
        AuditQueryApplicationService service = new(repository);

        var result = await service.GetAuditTrailAsync(new GetAuditTrailQuery
        {
            FromUtc = new DateTime(2026, 3, 20, 0, 0, 0, DateTimeKind.Utc),
            ToUtc = new DateTime(2026, 3, 19, 0, 0, 0, DateTimeKind.Utc)
        });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo("Audit.InvalidRange"));
    }

    private sealed class FakeAuditRepository : IAuditRepository
    {
        public Task RegisterEventAsync(AuditEntry entry, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<AuditEntry>> GetHistoryAsync(Guid aggregateId, string aggregateType, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<AuditEntry>>(Array.Empty<AuditEntry>());
        }

        public Task<AuditSearchResult> SearchAsync(AuditSearchFilter filter, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new AuditSearchResult
            {
                Items = new[]
                {
                    new AuditEntry
                    {
                        AggregateId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                        AggregateType = "Product",
                        Module = "Products",
                        Action = "product.created",
                        Detail = "Producto creado.",
                        PerformedBy = "auditor@plataforma.com",
                        OccurredAtUtc = new DateTime(2026, 3, 18, 12, 0, 0, DateTimeKind.Utc),
                        Source = "Application.Products",
                        Metadata = new Dictionary<string, string>
                        {
                            ["sku"] = "PROD-001"
                        }
                    }
                },
                TotalCount = 1,
                PageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber,
                PageSize = filter.PageSize <= 0 ? 25 : filter.PageSize
            });
        }
    }
}
