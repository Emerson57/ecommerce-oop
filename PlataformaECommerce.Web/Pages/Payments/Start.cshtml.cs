using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlataformaECommerce.Application.Features.Orders.Commands;
using PlataformaECommerce.Application.Interfaces.Services.Orders;
using PlataformaECommerce.Web.Authorization;

namespace PlataformaECommerce.Web.Pages.Payments;

/// <summary>
/// Inicia una sesión de checkout externa para un pedido del cliente autenticado.
/// </summary>
[Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
public sealed class StartModel : PageModel
{
    private const string PaymentStartSource = "Web.Payments.Start";
    private readonly IOrderPaymentCheckoutService _orderPaymentCheckoutService;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="StartModel"/>.
    /// </summary>
    public StartModel(IOrderPaymentCheckoutService orderPaymentCheckoutService)
    {
        _orderPaymentCheckoutService = orderPaymentCheckoutService ?? throw new ArgumentNullException(nameof(orderPaymentCheckoutService));
    }

    /// <summary>
    /// Mensaje temporal mostrado al cliente tras iniciar o fallar el pago.
    /// </summary>
    [TempData]
    public string? StatusMessage { get; set; }

    /// <summary>
    /// Inicia el checkout externo y redirige al cliente a la pasarela configurada.
    /// </summary>
    public async Task<IActionResult> OnGetAsync(Guid orderId, CancellationToken cancellationToken)
    {
        Guid? customerId = GetAuthenticatedCustomerId();
        if (!customerId.HasValue)
        {
            await InvalidateCustomerSessionAsync().ConfigureAwait(false);
            return RedirectToPage("/Auth/Login");
        }

        if (orderId == Guid.Empty)
        {
            StatusMessage = "Debes seleccionar un pedido válido para iniciar el pago.";
            return RedirectToPage("/Orders/Index");
        }

        string returnUrl = Url.RouteUrl(
            routeName: null,
            values: new
            {
                page = "/Payments/Confirm",
                orderId
            },
            protocol: Request.Scheme) ?? string.Empty;
        var result = await _orderPaymentCheckoutService.CreateCheckoutSessionAsync(
            new CreateOrderPaymentSessionCommand
            {
                OrderId = orderId,
                ExpectedCustomerId = customerId.Value,
                ReturnUrl = returnUrl,
                RequestedByUserId = customerId.Value,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Source = PaymentStartSource,
                RequestedAtUtc = DateTime.UtcNow
            },
            cancellationToken);

        if (result.IsFailure)
        {
            StatusMessage = result.Error.Message;
            return RedirectToPage("/Orders/Details", new { id = orderId });
        }

        return Redirect(result.Value.CheckoutUrl);
    }

    private Guid? GetAuthenticatedCustomerId()
    {
        string? rawUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(rawUserId, out Guid userId)
            ? userId
            : null;
    }

    private Task InvalidateCustomerSessionAsync()
    {
        return HttpContext.SignOutAsync(AuthorizationPolicies.CustomerCookieScheme);
    }
}
