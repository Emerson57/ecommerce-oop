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
[Authorize(
    Policy = AuthorizationPolicies.CustomerOnly,
    AuthenticationSchemes = AuthorizationPolicies.CustomerCookieScheme)]
public sealed class IndexModel : PageModel
{
    private const string OrdersSource = "Web.Orders.Index";
    private readonly IOrderApplicationService _orderApplicationService;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="IndexModel"/>.
    /// </summary>
    public IndexModel(IOrderApplicationService orderApplicationService)
    {
        _orderApplicationService = orderApplicationService ?? throw new ArgumentNullException(nameof(orderApplicationService));
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
    /// Fecha inicial del rango de creación aplicado al historial.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public DateOnly? CreatedFrom { get; set; }

    /// <summary>
    /// Fecha final del rango de creación aplicado al historial.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public DateOnly? CreatedTo { get; set; }

    /// <summary>
    /// Monto mínimo total aplicado al historial.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public decimal? MinTotalAmount { get; set; }

    /// <summary>
    /// Monto máximo total aplicado al historial.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public decimal? MaxTotalAmount { get; set; }

    /// <summary>
    /// Condición operativa seleccionada para el historial.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public OrderConditionFilter? Condition { get; set; }

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
    /// Indica si existe al menos un filtro adicional aplicado sobre el historial.
    /// </summary>
    public bool HasActiveFilters =>
        Status.HasValue ||
        CreatedFrom.HasValue ||
        CreatedTo.HasValue ||
        MinTotalAmount.HasValue ||
        MaxTotalAmount.HasValue ||
        Condition.HasValue;

    /// <summary>
    /// Valores de ruta asociados al conjunto actual de filtros.
    /// </summary>
    public IDictionary<string, string> CurrentFilterRouteValues
    {
        get
        {
            Dictionary<string, string> routeValues = [];

            if (Status.HasValue)
            {
                routeValues[nameof(Status)] = Status.Value.ToString();
            }

            if (CreatedFrom.HasValue)
            {
                routeValues[nameof(CreatedFrom)] = CreatedFrom.Value.ToString("yyyy-MM-dd");
            }

            if (CreatedTo.HasValue)
            {
                routeValues[nameof(CreatedTo)] = CreatedTo.Value.ToString("yyyy-MM-dd");
            }

            if (MinTotalAmount.HasValue)
            {
                routeValues[nameof(MinTotalAmount)] = MinTotalAmount.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            if (MaxTotalAmount.HasValue)
            {
                routeValues[nameof(MaxTotalAmount)] = MaxTotalAmount.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            if (Condition.HasValue)
            {
                routeValues[nameof(Condition)] = Condition.Value.ToString();
            }

            return routeValues;
        }
    }

    /// <summary>
    /// Carga el historial de pedidos del cliente autenticado.
    /// </summary>
    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        ValidateFilters();

        Guid? customerId = GetAuthenticatedCustomerId();
        if (!customerId.HasValue)
        {
            await InvalidateCustomerSessionAsync().ConfigureAwait(false);
            return RedirectToPage("/Auth/Login");
        }

        if (!ModelState.IsValid)
        {
            Orders = Array.Empty<OrderListItemViewModel>();
            return Page();
        }

        var result = await _orderApplicationService.GetOrdersByCustomerIdAsync(
            new GetOrdersByCustomerIdQuery(customerId.Value)
            {
                Status = Status,
                CreatedFromUtc = CreatedFrom.HasValue
                    ? DateTime.SpecifyKind(CreatedFrom.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc)
                    : null,
                CreatedToUtc = CreatedTo.HasValue
                    ? DateTime.SpecifyKind(CreatedTo.Value.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc)
                    : null,
                MinTotalAmount = MinTotalAmount,
                MaxTotalAmount = MaxTotalAmount,
                OnlyActive = Condition == OrderConditionFilter.Active ? true : null,
                OnlyFinalized = Condition == OrderConditionFilter.Finalized ? true : null,
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

    private void ValidateFilters()
    {
        if (CreatedFrom.HasValue && CreatedTo.HasValue && CreatedFrom.Value > CreatedTo.Value)
        {
            ModelState.AddModelError(nameof(CreatedFrom), "La fecha inicial no puede ser posterior a la fecha final.");
        }

        if (MinTotalAmount.HasValue && MinTotalAmount.Value < 0)
        {
            ModelState.AddModelError(nameof(MinTotalAmount), "El monto mínimo no puede ser negativo.");
        }

        if (MaxTotalAmount.HasValue && MaxTotalAmount.Value < 0)
        {
            ModelState.AddModelError(nameof(MaxTotalAmount), "El monto máximo no puede ser negativo.");
        }

        if (MinTotalAmount.HasValue && MaxTotalAmount.HasValue && MinTotalAmount.Value > MaxTotalAmount.Value)
        {
            ModelState.AddModelError(nameof(MinTotalAmount), "El monto mínimo no puede ser mayor que el monto máximo.");
        }
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
            StatusLabel = ResolveStatusLabel(order.Status)
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
    }

    /// <summary>
    /// Define la condición operativa disponible para el historial de pedidos del cliente.
    /// </summary>
    public enum OrderConditionFilter
    {
        Active,
        Finalized
    }
}
