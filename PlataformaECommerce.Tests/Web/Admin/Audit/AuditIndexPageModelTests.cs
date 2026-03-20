using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Audit.DTOs;
using PlataformaECommerce.Application.Features.Audit.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Audit;
using PlataformaECommerce.Web.Pages.Admin.Audit;

namespace PlataformaECommerce.Tests.Web.Admin.Audit;

[TestFixture]
public class AuditIndexPageModelTests
{
    [Test]
    public async Task OnGetAsync_FiltroValido_CargaEventos()
    {
        FakeAuditApplicationService service = new();
        IndexModel pageModel = new(service)
        {
            Module = "Products",
            PageSize = 20
        };

        await pageModel.OnGetAsync(CancellationToken.None);

        Assert.That(pageModel.AuditItems.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task OnGetAsync_AggregateIdInvalido_RegistraError()
    {
        FakeAuditApplicationService service = new();
        IndexModel pageModel = new(service)
        {
            AggregateId = "no-es-guid"
        };

        await pageModel.OnGetAsync(CancellationToken.None);

        Assert.That(pageModel.ErrorMessage, Is.EqualTo("El identificador del agregado debe ser un GUID válido."));
    }

    private sealed class FakeAuditApplicationService : IAuditApplicationService
    {
        public Task<Result<AuditQueryResultDto>> GetAuditTrailAsync(GetAuditTrailQuery query, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result.Success(new AuditQueryResultDto
            {
                Items = new[]
                {
                    new AuditEntryDto
                    {
                        AggregateId = Guid.NewGuid(),
                        AggregateType = "Product",
                        Module = "Products",
                        Action = "product.created",
                        Detail = "Producto creado.",
                        PerformedBy = "auditor@plataforma.com",
                        OccurredAtUtc = DateTime.UtcNow,
                        Metadata = new Dictionary<string, string>()
                    }
                },
                TotalCount = 1,
                ReturnedCount = 1,
                PageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber,
                PageSize = query.PageSize <= 0 ? 25 : query.PageSize,
                TotalPages = 1,
                HasPreviousPage = false,
                HasNextPage = false
            }));
        }
    }
}
