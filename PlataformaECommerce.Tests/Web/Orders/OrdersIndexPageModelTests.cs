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
        FakeOrderApplicationService orderApplicationService = new();
        IndexModel pageModel = CreatePageModel(orderApplicationService, Guid.NewGuid());

        IActionResult result = await pageModel.OnGetAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(pageModel.Orders.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task OnGetAsync_ConFiltrosAvanzados_PropagaQueryAlServicio()
    {
        FakeOrderApplicationService orderApplicationService = new();
        IndexModel pageModel = CreatePageModel(orderApplicationService, Guid.NewGuid());
        pageModel.Status = EstadoPedido.Pagado;
        pageModel.CreatedFrom = new DateOnly(2026, 1, 1);
        pageModel.CreatedTo = new DateOnly(2026, 1, 31);
        pageModel.MinTotalAmount = 100m;
        pageModel.MaxTotalAmount = 500m;
        pageModel.Condition = IndexModel.OrderConditionFilter.Active;

        IActionResult result = await pageModel.OnGetAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(orderApplicationService.LastGetOrdersQuery?.Status, Is.EqualTo(EstadoPedido.Pagado));
        Assert.That(orderApplicationService.LastGetOrdersQuery?.CreatedFromUtc, Is.EqualTo(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
        Assert.That(orderApplicationService.LastGetOrdersQuery?.CreatedToUtc, Is.EqualTo(new DateTime(2026, 1, 31, 23, 59, 59, 999, DateTimeKind.Utc).AddTicks(9999)));
        Assert.That(orderApplicationService.LastGetOrdersQuery?.MinTotalAmount, Is.EqualTo(100m));
        Assert.That(orderApplicationService.LastGetOrdersQuery?.MaxTotalAmount, Is.EqualTo(500m));
        Assert.That(orderApplicationService.LastGetOrdersQuery?.OnlyActive, Is.True);
        Assert.That(orderApplicationService.LastGetOrdersQuery?.OnlyFinalized, Is.Null);
        Assert.That(pageModel.HasActiveFilters, Is.True);
    }

    [Test]
    public async Task OnGetAsync_CondicionFinalizada_PropagaFiltroFinalizado()
    {
        FakeOrderApplicationService orderApplicationService = new();
        IndexModel pageModel = CreatePageModel(orderApplicationService, Guid.NewGuid());
        pageModel.Condition = IndexModel.OrderConditionFilter.Finalized;

        IActionResult result = await pageModel.OnGetAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(orderApplicationService.LastGetOrdersQuery?.OnlyFinalized, Is.True);
        Assert.That(orderApplicationService.LastGetOrdersQuery?.OnlyActive, Is.Null);
    }

    [Test]
    public async Task OnGetAsync_RangoInvalido_RetornaPaginaSinConsultarServicio()
    {
        FakeOrderApplicationService orderApplicationService = new();
        IndexModel pageModel = CreatePageModel(orderApplicationService, Guid.NewGuid());
        pageModel.CreatedFrom = new DateOnly(2026, 2, 1);
        pageModel.CreatedTo = new DateOnly(2026, 1, 1);

        IActionResult result = await pageModel.OnGetAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(orderApplicationService.LastGetOrdersQuery, Is.Null);
        Assert.That(pageModel.ModelState[nameof(IndexModel.CreatedFrom)]?.Errors, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task OnGetAsync_SinIdentificadorAutenticado_RedireccionaALoginYRevocaSesion()
    {
        FakeOrderApplicationService orderApplicationService = new();
        FakeAuthenticationService authenticationService = new();
        IndexModel pageModel = CreatePageModel(orderApplicationService, null, authenticationService);

        IActionResult result = await pageModel.OnGetAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<RedirectToPageResult>());
        Assert.That(authenticationService.LastSignOutScheme, Is.EqualTo(AuthorizationPolicies.CustomerCookieScheme));
    }

    private static IndexModel CreatePageModel(
        FakeOrderApplicationService orderApplicationService,
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

    private sealed class FakeOrderApplicationService : IOrderApplicationService
    {
        public GetOrdersByCustomerIdQuery? LastGetOrdersQuery { get; private set; }

        public Task<Result<OrderDetailDto>> CreateOrderFromCartAsync(CreateOrderFromCartCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<OrderDetailDto>> ConfirmOrderAsync(ConfirmOrderCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<OrderDetailDto>> RegisterOrderPaymentAsync(RegisterOrderPaymentCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<OrderDetailDto>> ProcessOrderAsync(ProcessOrderCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<OrderDetailDto>> ShipOrderAsync(ShipOrderCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<OrderDetailDto>> DeliverOrderAsync(DeliverOrderCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<OrderDetailDto>> CancelOrderAsync(CancelOrderCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<OrderDetailDto>> GetOrderByIdAsync(GetOrderByIdQuery query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<IReadOnlyCollection<OrderDto>>> GetOrdersByCustomerIdAsync(GetOrdersByCustomerIdQuery query, CancellationToken cancellationToken = default)
        {
            LastGetOrdersQuery = query;

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
