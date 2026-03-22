using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlataformaECommerce.Application.Common.Security;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Admin.Commands;
using PlataformaECommerce.Application.Features.Admin.DTOs;
using PlataformaECommerce.Application.Features.Admin.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Admin;
using PlataformaECommerce.Web.Pages.Admin;

namespace PlataformaECommerce.Tests.Web.Admin.Dashboard;

[TestFixture]
public class AdminDashboardPageModelTests
{
    [Test]
    public async Task OnGetAsync_UsuarioAutenticado_CargaDatosDelAdministradorYMetricaPrincipal()
    {
        IndexModel pageModel = new(new FakeAdminApplicationService());
        ClaimsPrincipal principal = new(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, "Admin Demo"),
            new Claim(ClaimTypes.Email, "admin@plataforma.com"),
            new Claim(ClaimTypes.Role, "Administrador"),
            new Claim(SecurityClaimTypes.AdminArea, "Operaciones")
        ], "AdminCookie"));

        pageModel.PageContext = new PageContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };

        await pageModel.OnGetAsync(CancellationToken.None);

        Assert.That(pageModel.DisplayName, Is.EqualTo("Admin Demo"));
        Assert.That(pageModel.Email, Is.EqualTo("admin@plataforma.com"));
        Assert.That(pageModel.Area, Is.EqualTo("Operaciones"));
        Assert.That(pageModel.IsAuthenticated, Is.True);
        Assert.That(pageModel.Dashboard.TotalProducts, Is.EqualTo(12));
    }

    private sealed class FakeAdminApplicationService : IAdminApplicationService
    {
        public Task<Result<AdminDto>> RegisterAdminAsync(RegisterAdminCommand command, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Result<AdminRegistrationDefinitionDto>> GetAdminRegistrationDefinitionAsync(GetAdminRegistrationDefinitionQuery query, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Result<AdminDashboardDto>> GetDashboardAsync(GetAdminDashboardQuery query, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result.Success(new AdminDashboardDto
            {
                GeneratedAtUtc = DateTime.UtcNow,
                WindowStartUtc = DateTime.UtcNow.AddDays(-7),
                WindowEndUtc = DateTime.UtcNow,
                WindowInDays = 7,
                TotalProducts = 12,
                ActiveProducts = 9,
                FeaturedProducts = 3,
                TotalOrders = 8,
                ActiveOrders = 5,
                TotalUsers = 14,
                TotalCustomers = 12,
                TotalAdministrators = 2,
                ActiveCarts = 4,
                AuditEventsLast24Hours = 7,
                RecentActivities = Array.Empty<AdminDashboardRecentActivityDto>()
            }));
        }

        public Task<Result<AdminUsersBackofficeDto>> GetUsersAsync(GetAdminUsersQuery query, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Result<AdminBackofficeUserDto>> ResetUserPasswordAsync(ResetUserPasswordCommand command, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}
