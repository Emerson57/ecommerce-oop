using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlataformaECommerce.Application.Features.Orders.DTOs;
using PlataformaECommerce.Application.Features.Orders.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Orders;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Web.Authorization;

namespace PlataformaECommerce.Web.Pages.Admin.Orders;

/// <summary>
/// Proporciona el listado administrativo de pedidos del backoffice.
/// </summary>
/// <remarks>
/// Esta página permite explorar pedidos del sistema con filtros operativos básicos,
/// seleccionar un pedido para ver su resumen y navegar hacia el módulo de auditoría.
/// </remarks>
[Authorize(
    Policy = AuthorizationPolicies.AdminOnly,
    AuthenticationSchemes = AuthorizationPolicies.AdminCookieScheme)]
public sealed class IndexModel : PageModel
{
    private const string AdminOrdersSource = "Web.Admin.Orders.Index";
    private readonly IOrderApplicationService _orderApplicationService;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="IndexModel"/>.
    /// </summary>
    public IndexModel(IOrderApplicationService orderApplicationService)
    {
        _orderApplicationService = orderApplicationService ?? throw new ArgumentNullException(nameof(orderApplicationService));
    }

    [BindProperty(SupportsGet = true)]
    public EstadoPedido? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? CreatedFrom { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? CreatedTo { get; set; }

    [BindProperty(SupportsGet = true)]
    public decimal? MinTotalAmount { get; set; }

    [BindProperty(SupportsGet = true)]
    public decimal? MaxTotalAmount { get; set; }

    [BindProperty(SupportsGet = true)]
    public OrderConditionFilter? Condition { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? SelectedOrderId { get; set; }

    /// <summary>
    /// Pedidos visibles en el listado administrativo.
    /// </summary>
    public IReadOnlyCollection<AdminOrderListItemViewModel> Orders { get; private set; } = Array.Empty<AdminOrderListItemViewModel>();

    /// <summary>
    /// Pedido actualmente seleccionado para resumen operativo.
    /// </summary>
    public AdminOrderSummaryViewModel? SelectedOrder { get; private set; }

    /// <summary>
    /// Mensaje funcional asociado a la vista actual.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Mensaje temporal publicado tras operaciones administrativas relacionadas.
    /// </summary>
    [TempData]
    public string? StatusMessage { get; set; }

    /// <summary>
    /// Indica si existe al menos un filtro activo.
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
                routeValues[nameof(CreatedFrom)] = CreatedFrom.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            if (CreatedTo.HasValue)
            {
                routeValues[nameof(CreatedTo)] = CreatedTo.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            if (MinTotalAmount.HasValue)
            {
                routeValues[nameof(MinTotalAmount)] = MinTotalAmount.Value.ToString(CultureInfo.InvariantCulture);
            }

            if (MaxTotalAmount.HasValue)
            {
                routeValues[nameof(MaxTotalAmount)] = MaxTotalAmount.Value.ToString(CultureInfo.InvariantCulture);
            }

            if (Condition.HasValue)
            {
                routeValues[nameof(Condition)] = Condition.Value.ToString();
            }

            return routeValues;
        }
    }

    /// <summary>
    /// Ejecuta la carga del listado administrativo de pedidos con filtros operativos.
    /// </summary>
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        ValidateFilters();
        if (!ModelState.IsValid)
        {
            Orders = Array.Empty<AdminOrderListItemViewModel>();
            SelectedOrder = null;
            return;
        }

        var result = await _orderApplicationService.GetOrdersAsync(
            new GetOrdersQuery
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
                RequestedByUserId = GetRequestedByUserId(),
                ExternalReference = AdminOrdersSource
            },
            cancellationToken);

        if (result.IsFailure)
        {
            ErrorMessage = result.Error.Message;
            SelectedOrder = null;
            return;
        }

        Orders = result.Value
            .Select(Map)
            .ToArray();

        if (SelectedOrderId.HasValue && SelectedOrderId.Value != Guid.Empty)
        {
            await LoadSelectedOrderAsync(SelectedOrderId.Value, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task LoadSelectedOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var result = await _orderApplicationService.GetOrderByIdAsync(
            new GetOrderByIdQuery(orderId)
            {
                RequestedByUserId = GetRequestedByUserId(),
                ExternalReference = "Web.Admin.Orders.Index.Select"
            },
            cancellationToken);

        if (result.IsFailure)
        {
            ErrorMessage = result.Error.Message;
            SelectedOrder = null;
            return;
        }

        SelectedOrder = MapSummary(result.Value);
    }

    private Guid? GetRequestedByUserId()
    {
        string? rawUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(rawUserId, out Guid userId)
            ? userId
            : null;
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

    private static AdminOrderListItemViewModel Map(OrderDto order)
    {
        return new AdminOrderListItemViewModel
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            Status = order.Status,
            StatusLabel = ResolveStatusLabel(order.Status),
            TotalAmount = order.TotalAmount,
            Currency = order.Currency,
            CreatedAtUtc = order.CreatedAtUtc,
            ItemsCount = order.ItemsCount,
            TotalUnits = order.TotalUnits,
            IsFinalized = order.IsFinalized
        };
    }

    private static AdminOrderSummaryViewModel MapSummary(OrderDetailDto order)
    {
        return new AdminOrderSummaryViewModel
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            StatusLabel = ResolveStatusLabel(order.Status),
            TotalAmount = order.TotalAmount,
            Currency = order.Currency,
            CreatedAtUtc = order.CreatedAtUtc,
            ConfirmedAtUtc = order.ConfirmedAtUtc,
            PaidAtUtc = order.PaidAtUtc,
            ShippedAtUtc = order.ShippedAtUtc,
            DeliveredAtUtc = order.DeliveredAtUtc,
            CancelledAtUtc = order.CancelledAtUtc,
            CancellationReason = order.CancellationReason,
            ContainsPhysicalProducts = order.ContainsPhysicalProducts,
            ContainsDigitalProducts = order.ContainsDigitalProducts,
            ShippingStreet = order.ShippingStreet,
            ShippingCity = order.ShippingCity,
            ShippingDepartment = order.ShippingDepartment,
            ShippingCountry = order.ShippingCountry,
            ShippingPostalCode = order.ShippingPostalCode
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
    /// Representa un pedido resumido dentro del listado administrativo.
    /// </summary>
    public sealed class AdminOrderListItemViewModel
    {
        public Guid Id { get; init; }
        public Guid CustomerId { get; init; }
        public EstadoPedido Status { get; init; }
        public string StatusLabel { get; init; } = string.Empty;
        public decimal TotalAmount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public DateTime CreatedAtUtc { get; init; }
        public int ItemsCount { get; init; }
        public int TotalUnits { get; init; }
        public bool IsFinalized { get; init; }
    }

    /// <summary>
    /// Representa el resumen ampliado de un pedido seleccionado desde el listado administrativo.
    /// </summary>
    public sealed class AdminOrderSummaryViewModel
    {
        public Guid Id { get; init; }
        public Guid CustomerId { get; init; }
        public string StatusLabel { get; init; } = string.Empty;
        public decimal TotalAmount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public DateTime CreatedAtUtc { get; init; }
        public DateTime? ConfirmedAtUtc { get; init; }
        public DateTime? PaidAtUtc { get; init; }
        public DateTime? ShippedAtUtc { get; init; }
        public DateTime? DeliveredAtUtc { get; init; }
        public DateTime? CancelledAtUtc { get; init; }
        public string? CancellationReason { get; init; }
        public bool ContainsPhysicalProducts { get; init; }
        public bool ContainsDigitalProducts { get; init; }
        public string? ShippingStreet { get; init; }
        public string? ShippingCity { get; init; }
        public string? ShippingDepartment { get; init; }
        public string? ShippingCountry { get; init; }
        public string? ShippingPostalCode { get; init; }
        public bool HasShippingAddress =>
            !string.IsNullOrWhiteSpace(ShippingStreet) &&
            !string.IsNullOrWhiteSpace(ShippingCity) &&
            !string.IsNullOrWhiteSpace(ShippingDepartment) &&
            !string.IsNullOrWhiteSpace(ShippingCountry) &&
            !string.IsNullOrWhiteSpace(ShippingPostalCode);
        public string FulfillmentLabel => ContainsDigitalProducts && !ContainsPhysicalProducts
            ? "Pedido digital"
            : ContainsDigitalProducts && ContainsPhysicalProducts
                ? "Pedido mixto"
                : "Pedido físico";
    }

    /// <summary>
    /// Define la condición operativa disponible para el listado administrativo de pedidos.
    /// </summary>
    public enum OrderConditionFilter
    {
        Active,
        Finalized
    }
}