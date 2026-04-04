using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Orders.Commands;
using PlataformaECommerce.Application.Features.Orders.DTOs;
using PlataformaECommerce.Application.Interfaces.Services.Orders;
using PlataformaECommerce.Web.Authorization;
using PlataformaECommerce.Web.Pages.Payments;

namespace PlataformaECommerce.Tests.Web.Payments;

[TestFixture]
public class PaymentsConfirmPageModelTests
{
    [Test]
    public async Task OnGetAsync_UsuarioAutenticado_RedireccionaADetallePedido()
    {
        Guid orderId = Guid.NewGuid();
        FakeOrderPaymentCheckoutService paymentCheckoutService = new();
        ConfirmModel pageModel = CreatePageModel(paymentCheckoutService, authenticatedUserId: Guid.NewGuid());

        IActionResult result = await pageModel.OnGetAsync(orderId, id: "tx-123", transactionId: null, CancellationToken.None);

        RedirectResult redirectResult = result as RedirectResult
            ?? throw new AssertionException("Se esperaba una redirección al detalle del pedido.");

        Assert.That(redirectResult.Url, Does.Contain("/Orders/Details"));
    }

    [Test]
    public async Task OnGetAsync_UsuarioAnonimo_RedireccionaALoginConReturnUrl()
    {
        Guid orderId = Guid.NewGuid();
        FakeOrderPaymentCheckoutService paymentCheckoutService = new();
        ConfirmModel pageModel = CreatePageModel(paymentCheckoutService, authenticatedUserId: null);

        IActionResult result = await pageModel.OnGetAsync(orderId, id: "tx-123", transactionId: null, CancellationToken.None);

        RedirectToPageResult redirectResult = result as RedirectToPageResult
            ?? throw new AssertionException("Se esperaba una redirección al login.");

        Assert.That(redirectResult.PageName, Is.EqualTo("/Auth/Login"));
        Assert.That(redirectResult.RouteValues?["returnUrl"], Is.Not.Null);
    }

    private static ConfirmModel CreatePageModel(FakeOrderPaymentCheckoutService paymentCheckoutService, Guid? authenticatedUserId)
    {
        DefaultHttpContext httpContext = new()
        {
            User = CreatePrincipal(authenticatedUserId)
        };

        ConfirmModel pageModel = new(paymentCheckoutService)
        {
            PageContext = new Microsoft.AspNetCore.Mvc.RazorPages.PageContext
            {
                HttpContext = httpContext
            },
            TempData = new TempDataDictionary(httpContext, new FakeTempDataProvider()),
            Url = new FakeUrlHelper()
        };

        return pageModel;
    }

    private static ClaimsPrincipal CreatePrincipal(Guid? authenticatedUserId)
    {
        if (!authenticatedUserId.HasValue)
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }

        List<Claim> claims =
        [
            new Claim(ClaimTypes.Name, "Cliente Demo"),
            new Claim(ClaimTypes.Role, "Cliente"),
            new Claim(AuthorizationPolicies.PrimaryRoleClaimType, "Cliente"),
            new Claim(AuthorizationPolicies.SuperUserClaimType, bool.FalseString),
            new Claim(ClaimTypes.NameIdentifier, authenticatedUserId.Value.ToString())
        ];

        return new ClaimsPrincipal(new ClaimsIdentity(claims, AuthorizationPolicies.CustomerCookieScheme));
    }

    private sealed class FakeOrderPaymentCheckoutService : IOrderPaymentCheckoutService
    {
        public Task<Result<PaymentCheckoutSessionDto>> CreateCheckoutSessionAsync(CreateOrderPaymentSessionCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<PaymentReturnResultDto>> ConfirmPaymentReturnAsync(ConfirmOrderPaymentReturnCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(new PaymentReturnResultDto
            {
                OrderId = command.OrderId,
                Provider = "Wompi",
                GatewayTransactionId = command.GatewayTransactionId,
                IsApproved = true,
                WasAlreadyRegistered = false,
                UserMessage = "Pago confirmado"
            }));
    }

    private sealed class FakeUrlHelper : IUrlHelper
    {
        public ActionContext ActionContext { get; } = new();
        public string? Action(Microsoft.AspNetCore.Mvc.Routing.UrlActionContext actionContext) => null;
        public string? Content(string? contentPath) => contentPath;
        public bool IsLocalUrl(string? url) => !string.IsNullOrWhiteSpace(url) && url.StartsWith('/');
        public string? Link(string? routeName, object? values) => null;
        public string? RouteUrl(Microsoft.AspNetCore.Mvc.Routing.UrlRouteContext routeContext) => "/Orders/Details?id=test";
    }

    private sealed class FakeTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
