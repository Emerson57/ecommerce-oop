using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Audit.DTOs;
using PlataformaECommerce.Application.Features.Audit.Queries;
using PlataformaECommerce.Application.Features.Orders.Commands;
using PlataformaECommerce.Application.Features.Orders.DTOs;
using PlataformaECommerce.Application.Features.Orders.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Audit;
using PlataformaECommerce.Application.Interfaces.Services.Orders;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Web.Authorization;
using PlataformaECommerce.Web.Pages.Orders;

namespace PlataformaECommerce.Tests.Web.Orders;

[TestFixture]
public class OrdersDetailsPageModelTests
{
    [Test]
    public async Task OnGetAsync_PedidoPropio_CargaDetalleYRetornaPagina()
    {
        FakeOrderQueryService orderApplicationService = new();
        DetailsModel pageModel = CreatePageModel(orderApplicationService, new FakeAuditApplicationService(), Guid.NewGuid());

        IActionResult result = await pageModel.OnGetAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(pageModel.Order.Items.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task OnGetAsync_PedidoInvalido_RedireccionaAlHistorial()
    {
        FakeOrderQueryService orderApplicationService = new();
        DetailsModel pageModel = CreatePageModel(orderApplicationService, new FakeAuditApplicationService(), Guid.NewGuid());

        IActionResult result = await pageModel.OnGetAsync(Guid.Empty, CancellationToken.None);

        Assert.That(result, Is.TypeOf<RedirectToPageResult>());
        Assert.That(pageModel.StatusMessage, Does.Contain("pedido válido"));
    }

    [Test]
    public async Task OnGetAsync_PedidoPendienteDePago_ExponeMetodoYPermiteContinuarPago()
    {
        FakeOrderQueryService orderApplicationService = new()
        {
            OrderStatus = EstadoPedido.Confirmado,
            PaymentMethod = MetodoPagoPedido.Tarjeta
        };
        DetailsModel pageModel = CreatePageModel(orderApplicationService, new FakeAuditApplicationService(), Guid.NewGuid());

        await pageModel.OnGetAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(pageModel.Order.PaymentMethodLabel, Is.EqualTo("Tarjeta"));
        Assert.That(pageModel.Order.CanStartOnlinePayment, Is.True);
        Assert.That(pageModel.OperationalHistory.Count, Is.EqualTo(1));
    }

    private static DetailsModel CreatePageModel(
        FakeOrderQueryService orderApplicationService,
        FakeAuditApplicationService auditApplicationService,
        Guid? authenticatedUserId,
        FakeAuthenticationService? authenticationService = null)
    {
        authenticationService ??= new FakeAuthenticationService();
        DetailsModel pageModel = new(orderApplicationService, auditApplicationService);

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
        public EstadoPedido OrderStatus { get; set; } = EstadoPedido.Enviado;
        public MetodoPagoPedido? PaymentMethod { get; set; } = MetodoPagoPedido.TransferenciaBancaria;

        public Task<Result<OrderDetailDto>> GetOrderByIdAsync(GetOrderByIdQuery query, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result.Success(new OrderDetailDto
            {
                Id = query.OrderId,
                CustomerId = query.ExpectedCustomerId ?? Guid.NewGuid(),
                Status = OrderStatus,
                ItemsCount = 1,
                TotalUnits = 1,
                TotalAmount = 99900m,
                Currency = "COP",
                CreatedAtUtc = DateTime.UtcNow.AddDays(-4),
                ConfirmedAtUtc = DateTime.UtcNow.AddDays(-4),
                PaidAtUtc = DateTime.UtcNow.AddDays(-4),
                ShippedAtUtc = DateTime.UtcNow.AddDays(-3),
                PaymentMethod = PaymentMethod,
                Items =
                [
                    new OrderItemDto
                    {
                        Id = Guid.NewGuid(),
                        OrderId = query.OrderId,
                        ProductId = Guid.NewGuid(),
                        ProductName = "Producto demo",
                        ProductSku = "SKU-001",
                        ProductType = TipoProducto.Fisico,
                        Quantity = 1,
                        UnitPrice = 99900m,
                        Currency = "COP",
                        Subtotal = 99900m,
                        CreatedAtUtc = DateTime.UtcNow.AddDays(-4)
                    }
                ]
            }));
        }

        public Task<Result<IReadOnlyCollection<OrderDto>>> GetOrdersByCustomerIdAsync(GetOrdersByCustomerIdQuery query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
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

    private sealed class FakeAuditApplicationService : IAuditApplicationService
    {
        public Task<Result<AuditQueryResultDto>> GetAuditTrailAsync(GetAuditTrailQuery query, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result.Success(new AuditQueryResultDto
            {
                Items =
                [
                    new AuditEntryDto
                    {
                        AggregateId = query.AggregateId ?? Guid.NewGuid(),
                        AggregateType = query.AggregateType ?? "Pedido",
                        Module = "Orders",
                        Action = "order.payment.registered",
                        Detail = "Se registró el pago del pedido.",
                        PerformedBy = "cliente@plataforma.com",
                        OccurredAtUtc = DateTime.UtcNow,
                        Source = "Web.Payments.Confirm",
                        Metadata = new Dictionary<string, string>
                        {
                            ["status"] = "Pagado"
                        }
                    }
                ],
                TotalCount = 1,
                ReturnedCount = 1,
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1
            }));
        }
    }

    private sealed class FakeTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
