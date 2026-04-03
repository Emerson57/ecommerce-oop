using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Orders.Commands;
using PlataformaECommerce.Application.Features.Orders.DTOs;
using PlataformaECommerce.Application.Features.Orders.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Orders;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Web.Authorization;
using PlataformaECommerce.Web.Pages.Orders;

namespace PlataformaECommerce.Tests.Web.Orders;

[TestFixture]
public class OrdersIndexPageModelTests
{
    [Test]
    public async Task OnGetAsync_ClienteAutenticado_CargaHistorialYRetornaPagina()
    {
        FakeOrderQueryService orderApplicationService = new();
        IndexModel pageModel = CreatePageModel(orderApplicationService, Guid.NewGuid());

        IActionResult result = await pageModel.OnGetAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(pageModel.Orders.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task OnGetAsync_SinIdentificadorAutenticado_RedireccionaALoginYRevocaSesion()
    {
        FakeOrderQueryService orderApplicationService = new();
        FakeAuthenticationService authenticationService = new();
        IndexModel pageModel = CreatePageModel(orderApplicationService, null, authenticationService);

        IActionResult result = await pageModel.OnGetAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<RedirectToPageResult>());
        Assert.That(authenticationService.LastSignOutScheme, Is.EqualTo(AuthorizationPolicies.CustomerCookieScheme));
    }

    private static IndexModel CreatePageModel(
        FakeOrderQueryService orderApplicationService,
        Guid? authenticatedUserId,
        FakeAuthenticationService? authenticationService = null)
    {
        authenticationService ??= new FakeAuthenticationService();
        IndexModel pageModel = new(orderApplicationService);

        ServiceCollection services = new();
        services.AddSingleton<IAuthenticationService>(authenticationService);
        DefaultHttpContext httpContext = new()
        {
            RequestServices = services.BuildServiceProvider(),
            User = CreatePrincipal(authenticatedUserId)
        };

        pageModel.PageContext = new PageContext
        {
            HttpContext = httpContext
        };
        pageModel.TempData = new TempDataDictionary(httpContext, new FakeTempDataProvider());

        return pageModel;
    }

    private static ClaimsPrincipal CreatePrincipal(Guid? authenticatedUserId)
    {
        List<Claim> claims =
        [
            new Claim(ClaimTypes.Name, "Cliente Demo"),
            new Claim(ClaimTypes.Role, RolUsuario.Cliente.ToString()),
            new Claim(AuthorizationPolicies.PrimaryRoleClaimType, RolUsuario.Cliente.ToString()),
            new Claim(AuthorizationPolicies.SuperUserClaimType, bool.FalseString)
        ];

        if (authenticatedUserId.HasValue)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, authenticatedUserId.Value.ToString()));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, AuthorizationPolicies.CustomerCookieScheme));
    }

    private sealed class FakeOrderQueryService : IOrderQueryService
    {
        public Task<Result<OrderDetailDto>> GetOrderByIdAsync(GetOrderByIdQuery query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<IReadOnlyCollection<OrderDto>>> GetOrdersByCustomerIdAsync(GetOrdersByCustomerIdQuery query, CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<OrderDto> orders =
            [
                new OrderDto
                {
                    Id = Guid.NewGuid(),
                    CustomerId = query.CustomerId,
                    Status = EstadoPedido.Pagado,
                    ItemsCount = 2,
                    TotalUnits = 3,
                    TotalAmount = 129900m,
                    Currency = "COP",
                    CreatedAtUtc = DateTime.UtcNow.AddDays(-2)
                }
            ];

            return Task.FromResult(Result.Success(orders));
        }
    }

    private sealed class FakeAuthenticationService : IAuthenticationService
    {
        public string? LastSignOutScheme { get; private set; }

        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme)
            => Task.FromResult(AuthenticateResult.NoResult());

        public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
        {
            LastSignOutScheme = scheme;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
