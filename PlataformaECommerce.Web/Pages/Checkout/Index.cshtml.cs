using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlataformaECommerce.Application.Features.Cart.Commands;
using PlataformaECommerce.Application.Features.Cart.DTOs;
using PlataformaECommerce.Application.Features.Cart.Queries;
using PlataformaECommerce.Application.Features.Orders.Commands;
using PlataformaECommerce.Application.Interfaces.Services.Cart;
using PlataformaECommerce.Application.Interfaces.Services.Orders;
using PlataformaECommerce.Web.Authorization;

namespace PlataformaECommerce.Web.Pages.Checkout;

/// <summary>
/// Proporciona la revisión final del carrito y la creación real del pedido del cliente autenticado.
/// </summary>
/// <remarks>
/// Esta página consolida la experiencia de checkout reutilizando los módulos de carrito y pedidos,
/// permitiendo confirmar la compra desde Razor Pages sin duplicar reglas de negocio en la UI.
/// </remarks>
[Authorize(
    Policy = AuthorizationPolicies.CustomerOnly,
    AuthenticationSchemes = AuthorizationPolicies.CustomerCookieScheme)]
public sealed class IndexModel : PageModel
{
    private const string CheckoutSource = "Web.Checkout.Index";
    private readonly ICartApplicationService _cartApplicationService;
    private readonly IOrderApplicationService _orderApplicationService;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="IndexModel"/>.
    /// </summary>
    public IndexModel(
        ICartApplicationService cartApplicationService,
        IOrderApplicationService orderApplicationService)
    {
        _cartApplicationService = cartApplicationService ?? throw new ArgumentNullException(nameof(cartApplicationService));
        _orderApplicationService = orderApplicationService ?? throw new ArgumentNullException(nameof(orderApplicationService));
    }

    /// <summary>
    /// Carrito en revisión durante el checkout.
    /// </summary>
    public CheckoutCartViewModel Cart { get; private set; } = new();

    /// <summary>
    /// Modelo de confirmación del checkout.
    /// </summary>
    [BindProperty]
    public CheckoutInputModel Input { get; set; } = new();

    /// <summary>
    /// Mensaje funcional asociado al checkout.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Mensaje temporal mostrado al redirigir al detalle del pedido.
    /// </summary>
    [TempData]
    public string? StatusMessage { get; set; }

    /// <summary>
    /// Carga la revisión final del carrito antes de convertirlo en pedido.
    /// </summary>
    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        Guid? customerId = GetAuthenticatedCustomerId();
        if (!customerId.HasValue)
        {
            await InvalidateCustomerSessionAsync().ConfigureAwait(false);
            return RedirectToPage("/Auth/Login");
        }

        bool wasLoaded = await LoadCheckoutCartAsync(customerId.Value, cancellationToken).ConfigureAwait(false);
        return wasLoaded
            ? Page()
            : RedirectToPage("/Cart/Index");
    }

    /// <summary>
    /// Ejecuta la creación real del pedido a partir del carrito en revisión.
    /// </summary>
    public async Task<IActionResult> OnPostPlaceOrderAsync(CancellationToken cancellationToken)
    {
        Guid? customerId = GetAuthenticatedCustomerId();
        if (!customerId.HasValue)
        {
            await InvalidateCustomerSessionAsync().ConfigureAwait(false);
            return RedirectToPage("/Auth/Login");
        }

        bool wasLoaded = await LoadCheckoutCartAsync(customerId.Value, cancellationToken).ConfigureAwait(false);
        if (!wasLoaded)
        {
            return RedirectToPage("/Cart/Index");
        }

        ValidateRequiredConfirmation();

        if (!ModelState.IsValid || !ValidateInputModel(Input, nameof(Input)))
        {
            return Page();
        }

        if (Cart.RequiresShippingAddress && !ValidateInputModel(Input.ShippingAddress, $"{nameof(Input)}.{nameof(CheckoutInputModel.ShippingAddress)}"))
        {
            return Page();
        }

        var result = await _orderApplicationService.CreateOrderFromCartAsync(
            new CreateOrderFromCartCommand
            {
                CartId = Cart.Id,
                CustomerId = customerId.Value,
                Notes = string.IsNullOrWhiteSpace(Input.Notes) ? null : Input.Notes.Trim(),
                ShippingStreet = Cart.RequiresShippingAddress ? Normalize(Input.ShippingAddress.Street) : null,
                ShippingCity = Cart.RequiresShippingAddress ? Normalize(Input.ShippingAddress.City) : null,
                ShippingDepartment = Cart.RequiresShippingAddress ? Normalize(Input.ShippingAddress.Department) : null,
                ShippingCountry = Cart.RequiresShippingAddress ? Normalize(Input.ShippingAddress.Country) : null,
                ShippingPostalCode = Cart.RequiresShippingAddress ? Normalize(Input.ShippingAddress.PostalCode) : null,
                ExternalReference = "Web.Checkout.PlaceOrder",
                RequestedByUserId = customerId.Value,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Source = CheckoutSource,
                RequestedAtUtc = DateTime.UtcNow
            },
            cancellationToken);

        if (result.IsFailure)
        {
            ErrorMessage = result.Error.Message;
            return Page();
        }

        TempData[nameof(StatusMessage)] = "Tu pedido fue creado correctamente a partir del carrito actual.";
        return RedirectToPage("/Orders/Details", new { id = result.Value.Id });
    }

    private async Task<bool> LoadCheckoutCartAsync(Guid customerId, CancellationToken cancellationToken)
    {
        CartDto? cart = await EnsureCartAsync(customerId, cancellationToken).ConfigureAwait(false);
        if (cart is null || !cart.HasItems)
        {
            TempData[nameof(StatusMessage)] = "Necesitas un carrito con productos para continuar al checkout.";
            return false;
        }

        Cart = Map(cart);
        return true;
    }

    private async Task<CartDto?> EnsureCartAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var queryResult = await _cartApplicationService.GetCartByCustomerIdAsync(
            new GetCartByCustomerIdQuery(customerId)
            {
                RequestedByUserId = customerId,
                ExternalReference = CheckoutSource
            },
            cancellationToken);

        if (queryResult.IsSuccess)
        {
            return queryResult.Value;
        }

        if (!string.Equals(queryResult.Error.Code, "Cart.NotFoundByCustomer", StringComparison.Ordinal)
            && !string.Equals(queryResult.Error.Code, "Cart.ActiveCartNotFound", StringComparison.Ordinal))
        {
            ErrorMessage = queryResult.Error.Message;
            return null;
        }

        var createResult = await _cartApplicationService.CreateCartAsync(
            new CreateCartCommand
            {
                CustomerId = customerId,
                RequestedByUserId = customerId,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Source = CheckoutSource,
                ExternalReference = "Web.Checkout.CreateCart",
                Reason = "Inicialización automática del carrito previa al checkout."
            },
            cancellationToken);

        if (createResult.IsFailure)
        {
            ErrorMessage = createResult.Error.Message;
            return null;
        }

        return createResult.Value;
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

    private void ValidateRequiredConfirmation()
    {
        if (!Input.ConfirmOrderCreation)
        {
            ModelState.AddModelError(
                $"{nameof(Input)}.{nameof(CheckoutInputModel.ConfirmOrderCreation)}",
                "Debes confirmar la creación del pedido para continuar.");
        }
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

    private static CheckoutCartViewModel Map(CartDto cart)
    {
        return new CheckoutCartViewModel
        {
            Id = cart.Id,
            ItemsCount = cart.ItemsCount,
            TotalUnits = cart.TotalUnits,
            TotalAmount = cart.TotalAmount,
            Currency = cart.Currency,
            RequiresShippingAddress = cart.Items.Any(item => item.IsPhysicalProduct),
            ContainsDigitalProducts = cart.Items.Any(item => item.IsDigitalProduct),
            Items = cart.Items
                .Select(item => new CheckoutCartItemViewModel
                {
                    ProductName = item.ProductName,
                    ProductSku = item.ProductSku,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Subtotal = item.Subtotal,
                    Currency = item.Currency
                })
                .ToArray()
        };
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <summary>
    /// Proyección del carrito utilizada durante el checkout.
    /// </summary>
    public sealed class CheckoutCartViewModel
    {
        public Guid Id { get; init; }
        public IReadOnlyCollection<CheckoutCartItemViewModel> Items { get; init; } = Array.Empty<CheckoutCartItemViewModel>();
        public int ItemsCount { get; init; }
        public int TotalUnits { get; init; }
        public decimal TotalAmount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public bool RequiresShippingAddress { get; init; }
        public bool ContainsDigitalProducts { get; init; }
        public bool IsDigitalOnly => ContainsDigitalProducts && !RequiresShippingAddress;
        public bool IsMixedOrder => ContainsDigitalProducts && RequiresShippingAddress;
        public string FulfillmentLabel => IsDigitalOnly
            ? "Entrega digital"
            : IsMixedOrder
                ? "Entrega mixta"
                : "Entrega física";
        public string FulfillmentDescription => IsDigitalOnly
            ? "Este checkout corresponde a productos digitales y no requiere dirección de envío."
            : IsMixedOrder
                ? "Tu compra combina productos digitales y físicos. La dirección se aplicará únicamente a los artículos físicos."
                : "Tu compra requiere despacho físico y necesita una dirección de envío válida.";
    }

    /// <summary>
    /// Proyección de una línea del carrito mostrada en el checkout.
    /// </summary>
    public sealed class CheckoutCartItemViewModel
    {
        public string ProductName { get; init; } = string.Empty;
        public string ProductSku { get; init; } = string.Empty;
        public int Quantity { get; init; }
        public decimal UnitPrice { get; init; }
        public decimal Subtotal { get; init; }
        public string Currency { get; init; } = string.Empty;
    }

    /// <summary>
    /// Captura la confirmación final del checkout.
    /// </summary>
    public sealed class CheckoutInputModel
    {
        [Display(Name = "Notas del pedido")]
        [StringLength(300, ErrorMessage = "Las notas del pedido no pueden superar los 300 caracteres.")]
        public string? Notes { get; set; }

        /// <summary>
        /// Dirección de envío requerida cuando el carrito contiene productos físicos.
        /// </summary>
        public ShippingAddressInputModel ShippingAddress { get; set; } = new();

        [Display(Name = "Confirmo que deseo generar el pedido con los productos del carrito")]
        public bool ConfirmOrderCreation { get; set; }
    }

    /// <summary>
    /// Captura la dirección de envío requerida para pedidos con productos físicos.
    /// </summary>
    public sealed class ShippingAddressInputModel
    {
        [Display(Name = "Calle o dirección principal")]
        [Required(ErrorMessage = "La calle o dirección principal es obligatoria.")]
        [StringLength(150, ErrorMessage = "La calle o dirección principal no puede superar los 150 caracteres.")]
        public string Street { get; set; } = string.Empty;

        [Display(Name = "Ciudad")]
        [Required(ErrorMessage = "La ciudad es obligatoria.")]
        [StringLength(150, ErrorMessage = "La ciudad no puede superar los 150 caracteres.")]
        public string City { get; set; } = string.Empty;

        [Display(Name = "Departamento o provincia")]
        [Required(ErrorMessage = "El departamento o provincia es obligatorio.")]
        [StringLength(150, ErrorMessage = "El departamento o provincia no puede superar los 150 caracteres.")]
        public string Department { get; set; } = string.Empty;

        [Display(Name = "País")]
        [Required(ErrorMessage = "El país es obligatorio.")]
        [StringLength(150, ErrorMessage = "El país no puede superar los 150 caracteres.")]
        public string Country { get; set; } = string.Empty;

        [Display(Name = "Código postal")]
        [Required(ErrorMessage = "El código postal es obligatorio.")]
        [StringLength(150, ErrorMessage = "El código postal no puede superar los 150 caracteres.")]
        public string PostalCode { get; set; } = string.Empty;
    }
}
