using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlataformaECommerce.Application.Features.Orders.Commands;
using PlataformaECommerce.Application.Interfaces.Services.Orders;

namespace PlataformaECommerce.Web.Pages.Payments;

/// <summary>
/// Confirma el retorno de la pasarela externa y redirige al flujo comercial del pedido.
/// </summary>
[AllowAnonymous]
public sealed class ConfirmModel : PageModel
{
    private const string PaymentConfirmSource = "Web.Payments.Confirm";
    private readonly IOrderPaymentCheckoutService _orderPaymentCheckoutService;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="ConfirmModel"/>.
    /// </summary>
    public ConfirmModel(IOrderPaymentCheckoutService orderPaymentCheckoutService)
    {
        _orderPaymentCheckoutService = orderPaymentCheckoutService ?? throw new ArgumentNullException(nameof(orderPaymentCheckoutService));
    }

    /// <summary>
    /// Mensaje temporal mostrado después de procesar el retorno de la pasarela.
    /// </summary>
    [TempData]
    public string? StatusMessage { get; set; }

    /// <summary>
    /// Procesa el retorno de la pasarela y redirige al detalle del pedido o al login.
    /// </summary>
    public async Task<IActionResult> OnGetAsync(Guid orderId, string? id, string? transactionId, CancellationToken cancellationToken)
    {
        if (orderId == Guid.Empty)
        {
            StatusMessage = "No se pudo identificar el pedido asociado al pago retornado.";
            return RedirectToPage("/Orders/Index");
        }

        string gatewayTransactionId = ResolveGatewayTransactionId(id, transactionId);
        if (string.IsNullOrWhiteSpace(gatewayTransactionId))
        {
            StatusMessage = "La pasarela no devolvió una transacción válida para confirmar el pago.";
            return RedirectToOrderDestination(orderId);
        }

        var result = await _orderPaymentCheckoutService.ConfirmPaymentReturnAsync(
            new ConfirmOrderPaymentReturnCommand
            {
                OrderId = orderId,
                GatewayTransactionId = gatewayTransactionId,
                RequestedByUserId = GetAuthenticatedCustomerId(),
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Source = PaymentConfirmSource,
                RequestedAtUtc = DateTime.UtcNow
            },
            cancellationToken);

        StatusMessage = result.IsFailure
            ? result.Error.Message
            : result.Value.UserMessage;

        return RedirectToOrderDestination(orderId);
    }

    private IActionResult RedirectToOrderDestination(Guid orderId)
    {
        string returnUrl = Url.RouteUrl(
            routeName: null,
            values: new
            {
                page = "/Orders/Details",
                id = orderId
            }) ?? $"/Orders/Details?id={orderId}";

        return User.Identity?.IsAuthenticated == true
            ? Redirect(returnUrl)
            : RedirectToPage("/Auth/Login", new { returnUrl });
    }

    private Guid? GetAuthenticatedCustomerId()
    {
        string? rawUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(rawUserId, out Guid userId)
            ? userId
            : null;
    }

    private static string ResolveGatewayTransactionId(string? id, string? transactionId)
    {
        return string.IsNullOrWhiteSpace(transactionId)
            ? id?.Trim() ?? string.Empty
            : transactionId.Trim();
    }
}
