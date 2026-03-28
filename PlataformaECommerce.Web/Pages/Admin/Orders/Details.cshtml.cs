using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlataformaECommerce.Application.Features.Orders.Commands;
using PlataformaECommerce.Application.Features.Orders.DTOs;
using PlataformaECommerce.Application.Features.Orders.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Orders;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Web.Authorization;

namespace PlataformaECommerce.Web.Pages.Admin.Orders;

/// <summary>
/// Proporciona el detalle operativo de un pedido para el backoffice administrativo.
/// </summary>
/// <remarks>
/// Esta página centraliza la visualización completa del pedido y permite ejecutar
/// transiciones del ciclo de vida conforme al estado actual y a la política operativa.
/// </remarks>
[Authorize(
    Policy = AuthorizationPolicies.AdminOnly,
    AuthenticationSchemes = AuthorizationPolicies.AdminCookieScheme)]
public sealed class DetailsModel : PageModel
{
    private const string AdminOrderDetailsSource = "Web.Admin.Orders.Details";
    private readonly IOrderApplicationService _orderApplicationService;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="DetailsModel"/>.
    /// </summary>
    public DetailsModel(IOrderApplicationService orderApplicationService)
    {
        _orderApplicationService = orderApplicationService ?? throw new ArgumentNullException(nameof(orderApplicationService));
    }

    /// <summary>
    /// Pedido actualmente cargado en el backoffice.
    /// </summary>
    public AdminOrderDetailsViewModel Order { get; private set; } = new();

    /// <summary>
    /// URL local opcional para volver al listado con el contexto previo.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    /// <summary>
    /// Captura de la confirmación operativa del pedido.
    /// </summary>
    [BindProperty]
    public ConfirmOrderInputModel ConfirmOrder { get; set; } = new();

    /// <summary>
    /// Captura del registro manual del pago del pedido.
    /// </summary>
    [BindProperty]
    public RegisterPaymentInputModel RegisterPayment { get; set; } = new();

    /// <summary>
    /// Captura del paso a procesamiento operativo.
    /// </summary>
    [BindProperty]
    public ProcessOrderInputModel ProcessOrder { get; set; } = new();

    /// <summary>
    /// Captura del despacho del pedido.
    /// </summary>
    [BindProperty]
    public ShipOrderInputModel ShipOrder { get; set; } = new();

    /// <summary>
    /// Captura del cierre de entrega del pedido.
    /// </summary>
    [BindProperty]
    public DeliverOrderInputModel DeliverOrder { get; set; } = new();

    /// <summary>
    /// Captura de la cancelación administrativa del pedido.
    /// </summary>
    [BindProperty]
    public CancelOrderInputModel CancelOrder { get; set; } = new();

    /// <summary>
    /// Mensaje funcional de error asociado a la operación actual.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Mensaje temporal publicado tras una transición operativa exitosa.
    /// </summary>
    [TempData]
    public string? StatusMessage { get; set; }

    /// <summary>
    /// Carga el detalle administrativo del pedido.
    /// </summary>
    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!await TryLoadOrderAsync(id, cancellationToken).ConfigureAwait(false))
        {
            return RedirectToPage("/Admin/Orders/Index");
        }

        return Page();
    }

    /// <summary>
    /// Confirma el pedido desde el backoffice.
    /// </summary>
    public Task<IActionResult> OnPostConfirmAsync(Guid id, CancellationToken cancellationToken)
    {
        return ExecuteTransitionAsync(
            id,
            order => order.CanConfirm,
            "El pedido no puede confirmarse desde su estado actual.",
            () => _orderApplicationService.ConfirmOrderAsync(
                new ConfirmOrderCommand
                {
                    OrderId = id,
                    Notes = Normalize(ConfirmOrder.Notes),
                    RequestedByUserId = GetRequestedByUserId(),
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Source = "AdminPortal.OrderConfirm",
                    ExternalReference = "Admin.Orders.Confirm",
                    RequestedAtUtc = DateTime.UtcNow
                },
                cancellationToken),
            "El pedido fue confirmado correctamente.",
            cancellationToken);
    }

    /// <summary>
    /// Registra el pago del pedido desde el backoffice.
    /// </summary>
    public async Task<IActionResult> OnPostRegisterPaymentAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!await TryLoadOrderForActionAsync(id, cancellationToken).ConfigureAwait(false))
        {
            return RedirectToPage("/Admin/Orders/Index");
        }

        if (!Order.CanRegisterPayment)
        {
            ErrorMessage = "El pago no puede registrarse desde el estado actual del pedido.";
            return Page();
        }

        if (!ValidateInputModel(RegisterPayment, nameof(RegisterPayment)))
        {
            return Page();
        }

        var result = await _orderApplicationService.RegisterOrderPaymentAsync(
            new RegisterOrderPaymentCommand
            {
                OrderId = id,
                PaymentReference = RegisterPayment.PaymentReference.Trim(),
                PaymentMethod = RegisterPayment.PaymentMethod.Trim(),
                Amount = RegisterPayment.Amount,
                Currency = RegisterPayment.Currency.Trim(),
                PaymentProvider = Normalize(RegisterPayment.PaymentProvider),
                Notes = Normalize(RegisterPayment.Notes),
                RequestedByUserId = GetRequestedByUserId(),
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Source = "AdminPortal.OrderPayment",
                ExternalReference = "Admin.Orders.RegisterPayment",
                RequestedAtUtc = DateTime.UtcNow
            },
            cancellationToken);

        if (result.IsFailure)
        {
            ErrorMessage = result.Error.Message;
            return Page();
        }

        StatusMessage = "El pago del pedido fue registrado correctamente.";
        return RedirectToPage("/Admin/Orders/Details", BuildRedirectRouteValues(result.Value.Id));
    }

    /// <summary>
    /// Marca el pedido como en proceso desde el backoffice.
    /// </summary>
    public async Task<IActionResult> OnPostProcessAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!await TryLoadOrderForActionAsync(id, cancellationToken).ConfigureAwait(false))
        {
            return RedirectToPage("/Admin/Orders/Index");
        }

        if (!Order.CanProcess)
        {
            ErrorMessage = "El pedido no puede pasar a proceso desde su estado actual.";
            return Page();
        }

        if (!ValidateInputModel(ProcessOrder, nameof(ProcessOrder)))
        {
            return Page();
        }

        var result = await _orderApplicationService.ProcessOrderAsync(
            new ProcessOrderCommand
            {
                OrderId = id,
                Reason = Normalize(ProcessOrder.Reason),
                Notes = Normalize(ProcessOrder.Notes),
                RequestedByUserId = GetRequestedByUserId(),
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Source = "AdminPortal.OrderProcess",
                ExternalReference = "Admin.Orders.Process",
                RequestedAtUtc = DateTime.UtcNow
            },
            cancellationToken);

        if (result.IsFailure)
        {
            ErrorMessage = result.Error.Message;
            return Page();
        }

        StatusMessage = "El pedido pasó correctamente a estado en proceso.";
        return RedirectToPage("/Admin/Orders/Details", BuildRedirectRouteValues(result.Value.Id));
    }

    /// <summary>
    /// Despacha el pedido desde el backoffice.
    /// </summary>
    public async Task<IActionResult> OnPostShipAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!await TryLoadOrderForActionAsync(id, cancellationToken).ConfigureAwait(false))
        {
            return RedirectToPage("/Admin/Orders/Index");
        }

        if (!Order.CanShip)
        {
            ErrorMessage = "El pedido no puede enviarse desde su estado actual.";
            return Page();
        }

        if (!ValidateInputModel(ShipOrder, nameof(ShipOrder)))
        {
            return Page();
        }

        var result = await _orderApplicationService.ShipOrderAsync(
            new ShipOrderCommand
            {
                OrderId = id,
                CarrierName = ShipOrder.CarrierName.Trim(),
                TrackingNumber = ShipOrder.TrackingNumber.Trim(),
                TrackingUrl = Normalize(ShipOrder.TrackingUrl),
                Notes = Normalize(ShipOrder.Notes),
                RequestedByUserId = GetRequestedByUserId(),
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Source = "AdminPortal.OrderShip",
                ExternalReference = "Admin.Orders.Ship",
                RequestedAtUtc = DateTime.UtcNow
            },
            cancellationToken);

        if (result.IsFailure)
        {
            ErrorMessage = result.Error.Message;
            return Page();
        }

        StatusMessage = "El pedido fue despachado correctamente.";
        return RedirectToPage("/Admin/Orders/Details", BuildRedirectRouteValues(result.Value.Id));
    }

    /// <summary>
    /// Marca el pedido como entregado desde el backoffice.
    /// </summary>
    public async Task<IActionResult> OnPostDeliverAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!await TryLoadOrderForActionAsync(id, cancellationToken).ConfigureAwait(false))
        {
            return RedirectToPage("/Admin/Orders/Index");
        }

        if (!Order.CanDeliver)
        {
            ErrorMessage = "El pedido no puede entregarse desde su estado actual.";
            return Page();
        }

        if (!ValidateInputModel(DeliverOrder, nameof(DeliverOrder)))
        {
            return Page();
        }

        var result = await _orderApplicationService.DeliverOrderAsync(
            new DeliverOrderCommand
            {
                OrderId = id,
                ReceivedBy = Normalize(DeliverOrder.ReceivedBy),
                DeliveryEvidence = Normalize(DeliverOrder.DeliveryEvidence),
                Notes = Normalize(DeliverOrder.Notes),
                RequestedByUserId = GetRequestedByUserId(),
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Source = "AdminPortal.OrderDeliver",
                ExternalReference = "Admin.Orders.Deliver",
                RequestedAtUtc = DateTime.UtcNow
            },
            cancellationToken);

        if (result.IsFailure)
        {
            ErrorMessage = result.Error.Message;
            return Page();
        }

        StatusMessage = "El pedido fue marcado como entregado correctamente.";
        return RedirectToPage("/Admin/Orders/Details", BuildRedirectRouteValues(result.Value.Id));
    }

    /// <summary>
    /// Cancela el pedido desde el backoffice.
    /// </summary>
    public async Task<IActionResult> OnPostCancelAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!await TryLoadOrderForActionAsync(id, cancellationToken).ConfigureAwait(false))
        {
            return RedirectToPage("/Admin/Orders/Index");
        }

        if (!Order.CanCancel)
        {
            ErrorMessage = "El pedido no puede cancelarse desde su estado actual.";
            return Page();
        }

        if (!ValidateInputModel(CancelOrder, nameof(CancelOrder)))
        {
            return Page();
        }

        var result = await _orderApplicationService.CancelOrderAsync(
            new CancelOrderCommand
            {
                OrderId = id,
                Reason = CancelOrder.Reason.Trim(),
                Notes = Normalize(CancelOrder.Notes),
                RequestedByCustomer = false,
                RequestedByUserId = GetRequestedByUserId(),
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Source = "AdminPortal.OrderCancel",
                ExternalReference = "Admin.Orders.Cancel",
                RequestedAtUtc = DateTime.UtcNow
            },
            cancellationToken);

        if (result.IsFailure)
        {
            ErrorMessage = result.Error.Message;
            return Page();
        }

        StatusMessage = "El pedido fue cancelado correctamente desde el backoffice.";
        return RedirectToPage("/Admin/Orders/Details", BuildRedirectRouteValues(result.Value.Id));
    }

    private async Task<IActionResult> ExecuteTransitionAsync(
        Guid id,
        Func<AdminOrderDetailsViewModel, bool> canExecute,
        string invalidStateMessage,
        Func<Task<Application.Common.Results.Result<OrderDetailDto>>> operation,
        string successMessage,
        CancellationToken cancellationToken)
    {
        if (!await TryLoadOrderForActionAsync(id, cancellationToken).ConfigureAwait(false))
        {
            return RedirectToPage("/Admin/Orders/Index");
        }

        if (!canExecute(Order))
        {
            ErrorMessage = invalidStateMessage;
            return Page();
        }

        var result = await operation().ConfigureAwait(false);
        if (result.IsFailure)
        {
            ErrorMessage = result.Error.Message;
            return Page();
        }

        StatusMessage = successMessage;
        return RedirectToPage("/Admin/Orders/Details", BuildRedirectRouteValues(result.Value.Id));
    }

    private async Task<bool> TryLoadOrderForActionAsync(Guid id, CancellationToken cancellationToken)
    {
        return await TryLoadOrderAsync(id, cancellationToken).ConfigureAwait(false);
    }

    private async Task<bool> TryLoadOrderAsync(Guid id, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            StatusMessage = "Debes seleccionar un pedido válido para operar desde el backoffice.";
            return false;
        }

        var result = await _orderApplicationService.GetOrderByIdAsync(
            new GetOrderByIdQuery(id)
            {
                RequestedByUserId = GetRequestedByUserId(),
                ExternalReference = AdminOrderDetailsSource
            },
            cancellationToken);

        if (result.IsFailure)
        {
            StatusMessage = result.Error.Message;
            return false;
        }

        Order = Map(result.Value);
        ApplyOperationDefaults();
        return true;
    }

    private void ApplyOperationDefaults()
    {
        if (RegisterPayment.Amount <= 0)
        {
            RegisterPayment.Amount = Order.TotalAmount;
        }

        if (string.IsNullOrWhiteSpace(RegisterPayment.Currency))
        {
            RegisterPayment.Currency = Order.Currency;
        }
    }

    private Guid? GetRequestedByUserId()
    {
        string? rawUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(rawUserId, out Guid userId)
            ? userId
            : null;
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

    private RouteValueDictionary BuildRedirectRouteValues(Guid orderId)
    {
        RouteValueDictionary routeValues = new()
        {
            ["id"] = orderId
        };

        if (IsLocalReturnUrl(ReturnUrl))
        {
            routeValues[nameof(ReturnUrl)] = ReturnUrl;
        }

        return routeValues;
    }

    private static bool IsLocalReturnUrl(string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && value.StartsWith('/')
            && !value.StartsWith("//", StringComparison.Ordinal)
            && !value.StartsWith("/\\", StringComparison.Ordinal);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static AdminOrderDetailsViewModel Map(OrderDetailDto order)
    {
        return new AdminOrderDetailsViewModel
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
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
            ContainsPhysicalProducts = order.ContainsPhysicalProducts,
            ContainsDigitalProducts = order.ContainsDigitalProducts,
            ShippingStreet = order.ShippingStreet,
            ShippingCity = order.ShippingCity,
            ShippingDepartment = order.ShippingDepartment,
            ShippingCountry = order.ShippingCountry,
            ShippingPostalCode = order.ShippingPostalCode,
            Items = order.Items
                .Select(item => new AdminOrderItemViewModel
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
    /// Proyección del pedido utilizada en el detalle administrativo.
    /// </summary>
    public sealed class AdminOrderDetailsViewModel
    {
        public Guid Id { get; init; }
        public Guid CustomerId { get; init; }
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
        public bool ContainsPhysicalProducts { get; init; }
        public bool ContainsDigitalProducts { get; init; }
        public string? ShippingStreet { get; init; }
        public string? ShippingCity { get; init; }
        public string? ShippingDepartment { get; init; }
        public string? ShippingCountry { get; init; }
        public string? ShippingPostalCode { get; init; }
        public IReadOnlyCollection<AdminOrderItemViewModel> Items { get; init; } = Array.Empty<AdminOrderItemViewModel>();
        public bool HasShippingAddress =>
            !string.IsNullOrWhiteSpace(ShippingStreet) &&
            !string.IsNullOrWhiteSpace(ShippingCity) &&
            !string.IsNullOrWhiteSpace(ShippingDepartment) &&
            !string.IsNullOrWhiteSpace(ShippingCountry) &&
            !string.IsNullOrWhiteSpace(ShippingPostalCode);
        public bool CanConfirm => Status == EstadoPedido.Pendiente;
        public bool CanRegisterPayment => Status == EstadoPedido.Confirmado;
        public bool CanProcess => Status == EstadoPedido.Pagado;
        public bool CanShip => Status == EstadoPedido.EnProceso;
        public bool CanDeliver => Status == EstadoPedido.Enviado;
        public bool CanCancel => Status is EstadoPedido.Pendiente or EstadoPedido.Confirmado or EstadoPedido.Pagado or EstadoPedido.EnProceso;
        public string FulfillmentLabel => ContainsDigitalProducts && !ContainsPhysicalProducts
            ? "Pedido digital"
            : ContainsDigitalProducts && ContainsPhysicalProducts
                ? "Pedido mixto"
                : "Pedido físico";
    }

    /// <summary>
    /// Proyección de una línea del pedido dentro del detalle administrativo.
    /// </summary>
    public sealed class AdminOrderItemViewModel
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
    /// Captura la observación opcional para la confirmación del pedido.
    /// </summary>
    public sealed class ConfirmOrderInputModel
    {
        [Display(Name = "Observación operativa")]
        [StringLength(300, ErrorMessage = "La observación operativa no puede superar los 300 caracteres.")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Captura la información necesaria para registrar el pago del pedido.
    /// </summary>
    public sealed class RegisterPaymentInputModel
    {
        [Display(Name = "Referencia del pago")]
        [Required(ErrorMessage = "La referencia del pago es obligatoria.")]
        [StringLength(100, ErrorMessage = "La referencia del pago no puede superar los 100 caracteres.")]
        public string PaymentReference { get; set; } = string.Empty;

        [Display(Name = "Método de pago")]
        [Required(ErrorMessage = "El método de pago es obligatorio.")]
        [StringLength(50, ErrorMessage = "El método de pago no puede superar los 50 caracteres.")]
        public string PaymentMethod { get; set; } = string.Empty;

        [Display(Name = "Monto pagado")]
        [Range(0.01d, 999999999d, ErrorMessage = "El monto pagado debe ser mayor que cero.")]
        public decimal Amount { get; set; }

        [Display(Name = "Moneda")]
        [Required(ErrorMessage = "La moneda es obligatoria.")]
        [StringLength(10, ErrorMessage = "La moneda no puede superar los 10 caracteres.")]
        public string Currency { get; set; } = string.Empty;

        [Display(Name = "Proveedor de pago")]
        [StringLength(100, ErrorMessage = "El proveedor de pago no puede superar los 100 caracteres.")]
        public string? PaymentProvider { get; set; }

        [Display(Name = "Notas")]
        [StringLength(300, ErrorMessage = "Las notas no pueden superar los 300 caracteres.")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Captura la observación de paso a proceso operativo.
    /// </summary>
    public sealed class ProcessOrderInputModel
    {
        [Display(Name = "Motivo operativo")]
        [StringLength(300, ErrorMessage = "El motivo operativo no puede superar los 300 caracteres.")]
        public string? Reason { get; set; }

        [Display(Name = "Notas internas")]
        [StringLength(300, ErrorMessage = "Las notas internas no pueden superar los 300 caracteres.")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Captura la información logística del despacho del pedido.
    /// </summary>
    public sealed class ShipOrderInputModel
    {
        [Display(Name = "Transportador")]
        [Required(ErrorMessage = "El transportador es obligatorio.")]
        [StringLength(100, ErrorMessage = "El transportador no puede superar los 100 caracteres.")]
        public string CarrierName { get; set; } = string.Empty;

        [Display(Name = "Número de guía")]
        [Required(ErrorMessage = "El número de guía es obligatorio.")]
        [StringLength(100, ErrorMessage = "El número de guía no puede superar los 100 caracteres.")]
        public string TrackingNumber { get; set; } = string.Empty;

        [Display(Name = "URL de seguimiento")]
        [StringLength(300, ErrorMessage = "La URL de seguimiento no puede superar los 300 caracteres.")]
        [Url(ErrorMessage = "La URL de seguimiento debe tener un formato válido.")]
        public string? TrackingUrl { get; set; }

        [Display(Name = "Notas de despacho")]
        [StringLength(300, ErrorMessage = "Las notas de despacho no pueden superar los 300 caracteres.")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Captura la información funcional de entrega del pedido.
    /// </summary>
    public sealed class DeliverOrderInputModel
    {
        [Display(Name = "Recibido por")]
        [StringLength(100, ErrorMessage = "El nombre del receptor no puede superar los 100 caracteres.")]
        public string? ReceivedBy { get; set; }

        [Display(Name = "Evidencia de entrega")]
        [StringLength(300, ErrorMessage = "La evidencia de entrega no puede superar los 300 caracteres.")]
        public string? DeliveryEvidence { get; set; }

        [Display(Name = "Notas finales")]
        [StringLength(300, ErrorMessage = "Las notas finales no pueden superar los 300 caracteres.")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Captura la cancelación administrativa del pedido.
    /// </summary>
    public sealed class CancelOrderInputModel
    {
        [Display(Name = "Motivo de cancelación")]
        [Required(ErrorMessage = "El motivo de cancelación es obligatorio.")]
        [MinLength(5, ErrorMessage = "El motivo de cancelación debe tener al menos 5 caracteres.")]
        [StringLength(300, ErrorMessage = "El motivo de cancelación no puede superar los 300 caracteres.")]
        public string Reason { get; set; } = string.Empty;

        [Display(Name = "Notas internas")]
        [StringLength(300, ErrorMessage = "Las notas internas no pueden superar los 300 caracteres.")]
        public string? Notes { get; set; }
    }
}