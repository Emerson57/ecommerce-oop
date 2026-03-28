using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlataformaECommerce.Application.Features.Orders.Commands;
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
    private const string OrderCancellationSource = "Web.Orders.Details.Cancel";
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
    /// Modelo de entrada para la cancelación autoservicio del pedido.
    /// </summary>
    [BindProperty]
    public CancelOrderInputModel Cancellation { get; set; } = new();

    /// <summary>
    /// Mensaje funcional de error asociado a la operación actual.
    /// </summary>
    public string? ErrorMessage { get; private set; }

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
        Guid? customerId = await TryResolveAuthenticatedCustomerIdAsync().ConfigureAwait(false);
        if (!customerId.HasValue)
        {
            return RedirectToPage("/Auth/Login");
        }

        if (!await TryLoadOwnedOrderAsync(id, customerId.Value, OrderDetailsSource, cancellationToken).ConfigureAwait(false))
        {
            return RedirectToPage("/Orders/Index");
        }

        return Page();
    }

    /// <summary>
    /// Procesa la cancelación autoservicio del pedido cuando el estado actual lo permite.
    /// </summary>
    public async Task<IActionResult> OnPostCancelAsync(Guid id, CancellationToken cancellationToken)
    {
        Guid? customerId = await TryResolveAuthenticatedCustomerIdAsync().ConfigureAwait(false);
        if (!customerId.HasValue)
        {
            return RedirectToPage("/Auth/Login");
        }

        if (!await TryLoadOwnedOrderAsync(id, customerId.Value, OrderDetailsSource, cancellationToken).ConfigureAwait(false))
        {
            return RedirectToPage("/Orders/Index");
        }

        if (!Order.CanBeCancelled)
        {
            ErrorMessage = "El pedido ya no puede cancelarse desde autoservicio debido a su estado actual.";
            return Page();
        }

        if (!ModelState.IsValid || !ValidateInputModel(Cancellation, nameof(Cancellation)))
        {
            return Page();
        }

        var result = await _orderApplicationService.CancelOrderAsync(
            new CancelOrderCommand
            {
                OrderId = Order.Id,
                Reason = Cancellation.Reason.Trim(),
                RequestedByCustomer = true,
                RequestedByUserId = customerId.Value,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Source = OrderCancellationSource,
                ExternalReference = OrderCancellationSource,
                RequestedAtUtc = DateTime.UtcNow
            },
            cancellationToken);

        if (result.IsFailure)
        {
            ErrorMessage = result.Error.Message;
            return Page();
        }

        StatusMessage = "El pedido fue cancelado correctamente.";
        return RedirectToPage("/Orders/Details", new { id = result.Value.Id });
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

    private async Task<Guid?> TryResolveAuthenticatedCustomerIdAsync()
    {
        Guid? customerId = GetAuthenticatedCustomerId();
        if (customerId.HasValue)
        {
            return customerId;
        }

        await InvalidateCustomerSessionAsync().ConfigureAwait(false);
        return null;
    }

    private async Task<bool> TryLoadOwnedOrderAsync(Guid id, Guid customerId, string externalReference, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            StatusMessage = "Debes seleccionar un pedido válido para consultar su detalle.";
            return false;
        }

        var result = await _orderApplicationService.GetOrderByIdAsync(
            new GetOrderByIdQuery(id)
            {
                ExpectedCustomerId = customerId,
                RequestedByUserId = customerId,
                ExternalReference = externalReference
            },
            cancellationToken);

        if (result.IsFailure)
        {
            StatusMessage = result.Error.Message;
            return false;
        }

        Order = Map(result.Value);
        return true;
    }

    private bool ValidateInputModel(object model, string prefix)
    {
        ArgumentNullException.ThrowIfNull(model);

        ValidationContext validationContext = new(model);
        List<ValidationResult> validationResults = [];
        bool isValid = Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);

        foreach (ValidationResult validationResult in validationResults)
        {
            if (validationResult.MemberNames.Any())
            {
                foreach (string memberName in validationResult.MemberNames)
                {
                    ModelState.AddModelError($"{prefix}.{memberName}", validationResult.ErrorMessage ?? "El valor informado no es válido.");
                }

                continue;
            }

            ModelState.AddModelError(prefix, validationResult.ErrorMessage ?? "El valor informado no es válido.");
        }

        return isValid;
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
            ShippingStreet = order.ShippingStreet,
            ShippingCity = order.ShippingCity,
            ShippingDepartment = order.ShippingDepartment,
            ShippingCountry = order.ShippingCountry,
            ShippingPostalCode = order.ShippingPostalCode,
            ContainsPhysicalProducts = order.ContainsPhysicalProducts,
            ContainsDigitalProducts = order.ContainsDigitalProducts,
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
        public string? ShippingStreet { get; init; }
        public string? ShippingCity { get; init; }
        public string? ShippingDepartment { get; init; }
        public string? ShippingCountry { get; init; }
        public string? ShippingPostalCode { get; init; }
        public bool ContainsPhysicalProducts { get; init; }
        public bool ContainsDigitalProducts { get; init; }
        public bool CanBeCancelled => Status is EstadoPedido.Pendiente or EstadoPedido.Confirmado or EstadoPedido.Pagado or EstadoPedido.EnProceso;
        public bool HasShippingAddress =>
            !string.IsNullOrWhiteSpace(ShippingStreet) &&
            !string.IsNullOrWhiteSpace(ShippingCity) &&
            !string.IsNullOrWhiteSpace(ShippingDepartment) &&
            !string.IsNullOrWhiteSpace(ShippingCountry) &&
            !string.IsNullOrWhiteSpace(ShippingPostalCode);
        public bool IsDigitalOnly => ContainsDigitalProducts && !ContainsPhysicalProducts;
        public bool IsMixedOrder => ContainsDigitalProducts && ContainsPhysicalProducts;
        public string FulfillmentLabel => IsDigitalOnly
            ? "Pedido digital"
            : IsMixedOrder
                ? "Pedido mixto"
                : "Pedido físico";
        public string FulfillmentDescription => IsDigitalOnly
            ? "El contenido se entrega por canales digitales y no requiere despacho físico."
            : IsMixedOrder
                ? "El pedido combina artículos digitales y físicos. La dirección registrada aplica a los productos físicos."
                : "El pedido requiere despacho físico hacia la dirección registrada.";
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

    /// <summary>
    /// Captura el motivo requerido para cancelar un pedido desde autoservicio.
    /// </summary>
    public sealed class CancelOrderInputModel
    {
        [Display(Name = "Motivo de cancelación")]
        [Required(ErrorMessage = "El motivo de cancelación es obligatorio.")]
        [MinLength(5, ErrorMessage = "El motivo de cancelación debe tener al menos 5 caracteres.")]
        [StringLength(300, ErrorMessage = "El motivo de cancelación no puede superar los 300 caracteres.")]
        public string Reason { get; set; } = string.Empty;
    }
}
