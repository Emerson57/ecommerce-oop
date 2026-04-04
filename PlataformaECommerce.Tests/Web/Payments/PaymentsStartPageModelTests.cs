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
using PlataformaECommerce.Application.Interfaces.Services.Orders;
using PlataformaECommerce.Web.Authorization;
using PlataformaECommerce.Web.Pages.Payments;

namespace PlataformaECommerce.Tests.Web.Payments;

[TestFixture]
public class PaymentsStartPageModelTests
{
    [Test]
    public async Task OnGetAsync_SesionDisponible_RedireccionaACheckoutExterno()
    {
        FakeOrderPaymentCheckoutService paymentCheckoutService = new()
        {
            CheckoutSessionResult = Result.Success(new PaymentCheckoutSessionDto
            {
                OrderId = Guid.NewGuid(),
                Provider = "Wompi",
                PaymentReference = "PAY-123",
                CheckoutUrl = "https://checkout.wompi.co/p/?reference=PAY-123"
            })
        };
        StartModel pageModel = CreatePageModel(paymentCheckoutService, Guid.NewGuid());

        IActionResult result = await pageModel.OnGetAsync(Guid.NewGuid(), CancellationToken.None);

        RedirectResult redirectResult = result as RedirectResult
            ?? throw new AssertionException("Se esperaba una redirección al checkout externo.");

        Assert.That(redirectResult.Url, Is.EqualTo("https://checkout.wompi.co/p/?reference=PAY-123"));
    }

    [Test]
    public async Task OnGetAsync_ErrorDePago_RedireccionaADetallePedido()
    {
        Guid orderId = Guid.NewGuid();
        FakeOrderPaymentCheckoutService paymentCheckoutService = new()
        {
            CheckoutSessionResult = Result.Failure<PaymentCheckoutSessionDto>(Error.Conflict("Payments.Disabled", "Pasarela no disponible."))
        };
        StartModel pageModel = CreatePageModel(paymentCheckoutService, Guid.NewGuid());

        IActionResult result = await pageModel.OnGetAsync(orderId, CancellationToken.None);

        RedirectToPageResult redirectResult = result as RedirectToPageResult
            ?? throw new AssertionException("Se esperaba una redirección al detalle del pedido.");

        Assert.That(redirectResult.PageName, Is.EqualTo("/Orders/Details"));
    }

    private static StartModel CreatePageModel(FakeOrderPaymentCheckoutService paymentCheckoutService, Guid? authenticatedUserId)
    {
        ServiceCollection services = new();
        services.AddSingleton<IAuthenticationService, FakeAuthenticationService>();
        DefaultHttpContext httpContext = new()
        {
            RequestServices = services.BuildServiceProvider(),
            User = CreatePrincipal(authenticatedUserId)
        };
        httpContext.Request.Scheme = "https";

        StartModel pageModel = new(paymentCheckoutService)
        {
            PageContext = new PageContext
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
        List<Claim> claims =
        [
            new Claim(ClaimTypes.Name, "Cliente Demo"),
            new Claim(ClaimTypes.Role, "Cliente"),
            new Claim(AuthorizationPolicies.PrimaryRoleClaimType, "Cliente"),
            new Claim(AuthorizationPolicies.SuperUserClaimType, bool.FalseString)
        ];

        if (authenticatedUserId.HasValue)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, authenticatedUserId.Value.ToString()));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, AuthorizationPolicies.CustomerCookieScheme));
    }

    private sealed class FakeOrderPaymentCheckoutService : IOrderPaymentCheckoutService
    {
        public Result<PaymentCheckoutSessionDto> CheckoutSessionResult { get; set; } = Result.Success(new PaymentCheckoutSessionDto());

        public Task<Result<PaymentCheckoutSessionDto>> CreateCheckoutSessionAsync(CreateOrderPaymentSessionCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(CheckoutSessionResult);

        public Task<Result<PaymentReturnResultDto>> ConfirmPaymentReturnAsync(ConfirmOrderPaymentReturnCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class FakeAuthenticationService : IAuthenticationService
    {
        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme) => Task.FromResult(AuthenticateResult.NoResult());
        public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;
        public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;
        public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties) => Task.CompletedTask;
        public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;
    }

    private sealed class FakeUrlHelper : IUrlHelper
    {
        public ActionContext ActionContext { get; } = new();
        public string? Action(Microsoft.AspNetCore.Mvc.Routing.UrlActionContext actionContext) => null;
        public string? Content(string? contentPath) => contentPath;
        public bool IsLocalUrl(string? url) => !string.IsNullOrWhiteSpace(url) && url.StartsWith('/');
        public string? Link(string? routeName, object? values) => null;
        public string? RouteUrl(Microsoft.AspNetCore.Mvc.Routing.UrlRouteContext routeContext) => "https://shop.example.com/payments/confirm?orderId=test";
    }

    private sealed class FakeTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
