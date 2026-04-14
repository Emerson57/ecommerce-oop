using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlataformaECommerce.Application.Features.Orders.DTOs;
using PlataformaECommerce.Application.Features.Orders.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Orders;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Web.Authorization;

namespace PlataformaECommerce.Web.Pages.Orders;

/// <summary>
/// Proporciona el historial de pedidos del cliente autenticado.
/// </summary>
/// <remarks>
/// Esta página reutiliza el módulo de pedidos para proyectar un historial autoservicio
/// orientado al cliente, manteniendo aislamiento respecto del backoffice y filtrando
/// únicamente los pedidos pertenecientes a la cuenta autenticada.
/// </remarks>
[Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
public sealed class IndexModel : PageModel
{
    private const string OrdersSource = "Web.Orders.Index";
    private readonly IOrderQueryService _orderQueryService;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="IndexModel"/>.
    /// </summary>
    public IndexModel(IOrderQueryService orderQueryService)
    {
        _orderQueryService = orderQueryService ?? throw new ArgumentNullException(nameof(orderQueryService));
    }

    /// <summary>
    /// Colección de pedidos visibles del cliente autenticado.
    /// </summary>
    public IReadOnlyCollection<OrderListItemViewModel> Orders { get; private set; } = Array.Empty<OrderListItemViewModel>();

    /// <summary>
    /// Estado aplicado al filtro actual cuando exista.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public EstadoPedido? Status { get; set; }

    /// <summary>
    /// Mensaje funcional asociado a la consulta del historial.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Mensaje temporal mostrado cuando una operación redirige al historial.
    /// </summary>
    [TempData]
    public string? StatusMessage { get; set; }

    /// <summary>
    /// Carga el historial de pedidos del cliente autenticado.
    /// </summary>
    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        Guid? customerId = GetAuthenticatedCustomerId();
        if (!customerId.HasValue)
        {
            await InvalidateCustomerSessionAsync().ConfigureAwait(false);
            return RedirectToPage("/Auth/Login");
        }

        var result = await _orderQueryService.GetOrdersByCustomerIdAsync(
            new GetOrdersByCustomerIdQuery(customerId.Value)
            {
                Status = Status,
                IncludeItems = false,
                RequestedByUserId = customerId.Value,
                ExternalReference = OrdersSource
            },
            cancellationToken);

        if (result.IsFailure)
        {
            ErrorMessage = result.Error.Message;
            Orders = Array.Empty<OrderListItemViewModel>();
            return Page();
        }

        Orders = result.Value
            .Select(Map)
            .ToArray();

        return Page();
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

    private static OrderListItemViewModel Map(OrderDto order)
    {
        return new OrderListItemViewModel
        {
            Id = order.Id,
            Status = order.Status,
            ItemsCount = order.ItemsCount,
            TotalUnits = order.TotalUnits,
            TotalAmount = order.TotalAmount,
            Currency = order.Currency,
            CreatedAtUtc = order.CreatedAtUtc,
            UpdatedAtUtc = order.UpdatedAtUtc,
            IsFinalized = order.IsFinalized,
            StatusLabel = ResolveStatusLabel(order.Status),
            PaymentMethod = order.PaymentMethod
        };
    }

    private static string ResolveStatusLabel(EstadoPedido status)
    {
        return status switch
        {
            EstadoPedido.Pendiente => "Pendiente",
            EstadoPedido.Confirmado => "Confirmado",
            EstadoPedido.Pagado => "Pagado",
            EstadoPedido.EnProceso => "En proceso",
            EstadoPedido.Enviado => "Enviado",
            EstadoPedido.Entregado => "Entregado",
            EstadoPedido.Cancelado => "Cancelado",
            _ => status.ToString()
        };
    }

    /// <summary>
    /// Representa un pedido resumido dentro del historial del cliente.
    /// </summary>
    public sealed class OrderListItemViewModel
    {
        public Guid Id { get; init; }
        public EstadoPedido Status { get; init; }
        public string StatusLabel { get; init; } = string.Empty;
        public int ItemsCount { get; init; }
        public int TotalUnits { get; init; }
        public decimal TotalAmount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public DateTime CreatedAtUtc { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public bool IsFinalized { get; init; }
        public MetodoPagoPedido? PaymentMethod { get; init; }
        public bool CanStartOnlinePayment => Status == EstadoPedido.Confirmado && PaymentMethod is MetodoPagoPedido.Tarjeta or MetodoPagoPedido.Pse or MetodoPagoPedido.TransferenciaBancaria;
    }
}
