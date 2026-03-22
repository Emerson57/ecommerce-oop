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
/// Proporciona el detalle de un pedido perteneciente al cliente autenticado.
/// </summary>
/// <remarks>
/// Esta página consulta el pedido desde Application reforzando la pertenencia del pedido
/// al cliente autenticado para evitar exposición cruzada de información sensible.
/// </remarks>
[Authorize(
    Policy = AuthorizationPolicies.CustomerOnly,
    AuthenticationSchemes = AuthorizationPolicies.CustomerCookieScheme)]
public sealed class DetailsModel : PageModel
{
    private const string OrderDetailsSource = "Web.Orders.Details";
    private readonly IOrderApplicationService _orderApplicationService;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="DetailsModel"/>.
    /// </summary>
    public DetailsModel(IOrderApplicationService orderApplicationService)
    {
        _orderApplicationService = orderApplicationService ?? throw new ArgumentNullException(nameof(orderApplicationService));
    }

    /// <summary>
    /// Pedido actualmente proyectado en pantalla.
    /// </summary>
    public OrderDetailsViewModel Order { get; private set; } = new();

    /// <summary>
    /// Mensaje funcional publicado cuando el detalle no puede recuperarse.
    /// </summary>
    [TempData]
    public string? StatusMessage { get; set; }

    /// <summary>
    /// Carga el detalle de un pedido perteneciente al cliente autenticado.
    /// </summary>
    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        Guid? customerId = GetAuthenticatedCustomerId();
        if (!customerId.HasValue)
        {
            await InvalidateCustomerSessionAsync().ConfigureAwait(false);
            return RedirectToPage("/Auth/Login");
        }

        if (id == Guid.Empty)
        {
            StatusMessage = "Debes seleccionar un pedido válido para consultar su detalle.";
            return RedirectToPage("/Orders/Index");
        }

        var result = await _orderApplicationService.GetOrderByIdAsync(
            new GetOrderByIdQuery(id)
            {
                ExpectedCustomerId = customerId.Value,
                RequestedByUserId = customerId.Value,
                ExternalReference = OrderDetailsSource
            },
            cancellationToken);

        if (result.IsFailure)
        {
            StatusMessage = result.Error.Message;
            return RedirectToPage("/Orders/Index");
        }

        Order = Map(result.Value);
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

    private static OrderDetailsViewModel Map(OrderDetailDto order)
    {
        return new OrderDetailsViewModel
        {
            Id = order.Id,
            Status = order.Status,
            StatusLabel = ResolveStatusLabel(order.Status),
            ItemsCount = order.ItemsCount,
            TotalUnits = order.TotalUnits,
            TotalAmount = order.TotalAmount,
            Currency = order.Currency,
            CreatedAtUtc = order.CreatedAtUtc,
            ConfirmedAtUtc = order.ConfirmedAtUtc,
            PaidAtUtc = order.PaidAtUtc,
            ShippedAtUtc = order.ShippedAtUtc,
            DeliveredAtUtc = order.DeliveredAtUtc,
            CancelledAtUtc = order.CancelledAtUtc,
            CancellationReason = order.CancellationReason,
            Items = order.Items
                .Select(item => new OrderItemViewModel
                {
                    ProductName = item.ProductName,
                    ProductSku = item.ProductSku,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Subtotal = item.Subtotal,
                    Currency = item.Currency,
                    IsDigitalProduct = item.IsDigitalProduct
                })
                .ToArray()
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
    /// Representa la proyección del detalle del pedido para la UI del cliente.
    /// </summary>
    public sealed class OrderDetailsViewModel
    {
        public Guid Id { get; init; }
        public EstadoPedido Status { get; init; }
        public string StatusLabel { get; init; } = string.Empty;
        public int ItemsCount { get; init; }
        public int TotalUnits { get; init; }
        public decimal TotalAmount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public DateTime CreatedAtUtc { get; init; }
        public DateTime? ConfirmedAtUtc { get; init; }
        public DateTime? PaidAtUtc { get; init; }
        public DateTime? ShippedAtUtc { get; init; }
        public DateTime? DeliveredAtUtc { get; init; }
        public DateTime? CancelledAtUtc { get; init; }
        public string? CancellationReason { get; init; }
        public IReadOnlyCollection<OrderItemViewModel> Items { get; init; } = Array.Empty<OrderItemViewModel>();
    }

    /// <summary>
    /// Representa una línea del pedido proyectada en el detalle mostrado al cliente.
    /// </summary>
    public sealed class OrderItemViewModel
    {
        public string ProductName { get; init; } = string.Empty;
        public string ProductSku { get; init; } = string.Empty;
        public int Quantity { get; init; }
        public decimal UnitPrice { get; init; }
        public decimal Subtotal { get; init; }
        public string Currency { get; init; } = string.Empty;
        public bool IsDigitalProduct { get; init; }
    }
}
