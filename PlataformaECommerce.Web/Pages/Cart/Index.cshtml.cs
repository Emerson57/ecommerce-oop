using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlataformaECommerce.Application.Features.Cart.Commands;
using PlataformaECommerce.Application.Features.Cart.DTOs;
using PlataformaECommerce.Application.Features.Cart.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Cart;
using PlataformaECommerce.Web.Authorization;

namespace PlataformaECommerce.Web.Pages.Cart;

/// <summary>
/// Proporciona la experiencia de carrito para el cliente autenticado.
/// </summary>
/// <remarks>
/// Esta página reutiliza los casos de uso existentes del módulo de carrito para mostrar,
/// crear y modificar el carrito activo del cliente sin exponer lógica de dominio en la UI.
/// </remarks>
[Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
public sealed class IndexModel : PageModel
{
    private const string CartSource = "Web.Cart.Index";
    private readonly ICartApplicationService _cartApplicationService;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="IndexModel"/>.
    /// </summary>
    public IndexModel(ICartApplicationService cartApplicationService)
    {
        _cartApplicationService = cartApplicationService ?? throw new ArgumentNullException(nameof(cartApplicationService));
    }

    /// <summary>
    /// Carrito proyectado para la UI del cliente autenticado.
    /// </summary>
    public CartViewModel Cart { get; private set; } = new();

    /// <summary>
    /// Mensaje funcional asociado a la operación actual.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Mensaje temporal mostrado al finalizar una operación exitosa.
    /// </summary>
    [TempData]
    public string? StatusMessage { get; set; }

    /// <summary>
    /// Carga el carrito activo del cliente autenticado.
    /// </summary>
    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        Guid? customerId = GetAuthenticatedCustomerId();
        if (!customerId.HasValue)
        {
            await InvalidateCustomerSessionAsync().ConfigureAwait(false);
            return RedirectToPage("/Auth/Login");
        }

        await LoadCartAsync(customerId.Value, cancellationToken).ConfigureAwait(false);
        return Page();
    }

    /// <summary>
    /// Agrega un producto al carrito activo del cliente autenticado.
    /// </summary>
    public async Task<IActionResult> OnPostAddItemAsync(Guid productId, int quantity = 1, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        Guid? customerId = GetAuthenticatedCustomerId();
        if (!customerId.HasValue)
        {
            await InvalidateCustomerSessionAsync().ConfigureAwait(false);
            return RedirectToPage("/Auth/Login");
        }

        CartDto? cart = await EnsureCartAsync(customerId.Value, cancellationToken).ConfigureAwait(false);
        if (cart is null)
        {
            return RedirectToPage();
        }

        var result = await _cartApplicationService.AddProductToCartAsync(
            new AddProductToCartCommand
            {
                CartId = cart.Id,
                ProductId = productId,
                Quantity = quantity,
                RequestedByUserId = customerId.Value,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Source = CartSource,
                ExternalReference = "Web.Cart.AddItem",
                Reason = "Agregado de producto desde el canal web del cliente."
            },
            cancellationToken);

        StatusMessage = result.IsSuccess
            ? "El producto fue agregado correctamente al carrito."
            : result.Error.Message;

        return ResolveRedirect(returnUrl);
    }

    /// <summary>
    /// Actualiza la cantidad de un ítem existente dentro del carrito activo.
    /// </summary>
    public async Task<IActionResult> OnPostUpdateQuantityAsync(Guid cartItemId, Guid productId, int quantity, CancellationToken cancellationToken)
    {
        Guid? customerId = GetAuthenticatedCustomerId();
        if (!customerId.HasValue)
        {
            await InvalidateCustomerSessionAsync().ConfigureAwait(false);
            return RedirectToPage("/Auth/Login");
        }

        CartDto? cart = await EnsureCartAsync(customerId.Value, cancellationToken).ConfigureAwait(false);
        if (cart is null)
        {
            return RedirectToPage();
        }

        var result = await _cartApplicationService.UpdateCartItemQuantityAsync(
            new UpdateCartItemQuantityCommand
            {
                CartId = cart.Id,
                CartItemId = cartItemId,
                ProductId = productId,
                NewQuantity = quantity,
                RequestedByUserId = customerId.Value,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Source = CartSource,
                ExternalReference = "Web.Cart.UpdateQuantity",
                Reason = "Actualización de cantidad desde el carrito del cliente."
            },
            cancellationToken);

        if (result.IsFailure)
        {
            ErrorMessage = result.Error.Message;
        }

        await LoadCartAsync(customerId.Value, cancellationToken).ConfigureAwait(false);
        return Page();
    }

    /// <summary>
    /// Remueve un producto del carrito activo del cliente autenticado.
    /// </summary>
    public async Task<IActionResult> OnPostRemoveItemAsync(Guid productId, Guid? cartItemId, CancellationToken cancellationToken)
    {
        Guid? customerId = GetAuthenticatedCustomerId();
        if (!customerId.HasValue)
        {
            await InvalidateCustomerSessionAsync().ConfigureAwait(false);
            return RedirectToPage("/Auth/Login");
        }

        CartDto? cart = await EnsureCartAsync(customerId.Value, cancellationToken).ConfigureAwait(false);
        if (cart is null)
        {
            return RedirectToPage();
        }

        var result = await _cartApplicationService.RemoveProductFromCartAsync(
            new RemoveProductFromCartCommand
            {
                CartId = cart.Id,
                ProductId = productId,
                CartItemId = cartItemId,
                RequestedByUserId = customerId.Value,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Source = CartSource,
                ExternalReference = "Web.Cart.RemoveItem",
                Reason = "Remoción de producto desde el carrito del cliente."
            },
            cancellationToken);

        if (result.IsFailure)
        {
            ErrorMessage = result.Error.Message;
        }

        await LoadCartAsync(customerId.Value, cancellationToken).ConfigureAwait(false);
        return Page();
    }

    /// <summary>
    /// Vacía completamente el carrito activo del cliente autenticado.
    /// </summary>
    public async Task<IActionResult> OnPostClearAsync(CancellationToken cancellationToken)
    {
        Guid? customerId = GetAuthenticatedCustomerId();
        if (!customerId.HasValue)
        {
            await InvalidateCustomerSessionAsync().ConfigureAwait(false);
            return RedirectToPage("/Auth/Login");
        }

        CartDto? cart = await EnsureCartAsync(customerId.Value, cancellationToken).ConfigureAwait(false);
        if (cart is null)
        {
            return RedirectToPage();
        }

        var result = await _cartApplicationService.ClearCartAsync(
            new ClearCartCommand
            {
                CartId = cart.Id,
                RequestedByUserId = customerId.Value,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Source = CartSource,
                ExternalReference = "Web.Cart.Clear",
                Reason = "Vaciado voluntario del carrito desde la web."
            },
            cancellationToken);

        if (result.IsFailure)
        {
            ErrorMessage = result.Error.Message;
        }
        else
        {
            StatusMessage = "El carrito fue vaciado correctamente.";
        }

        await LoadCartAsync(customerId.Value, cancellationToken).ConfigureAwait(false);
        return Page();
    }

    private async Task LoadCartAsync(Guid customerId, CancellationToken cancellationToken)
    {
        CartDto? cart = await EnsureCartAsync(customerId, cancellationToken).ConfigureAwait(false);
        Cart = cart is null ? new CartViewModel() : Map(cart);
    }

    private async Task<CartDto?> EnsureCartAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var queryResult = await _cartApplicationService.GetCartByCustomerIdAsync(
            new GetCartByCustomerIdQuery(customerId)
            {
                RequestedByUserId = customerId,
                ExternalReference = CartSource
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
                Source = CartSource,
                ExternalReference = "Web.Cart.Create",
                Reason = "Inicialización automática del carrito del cliente."
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

    private IActionResult ResolveRedirect(string? returnUrl)
    {
        return IsSafeLocalReturnUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToPage();
    }

    private static bool IsSafeLocalReturnUrl(string? returnUrl)
    {
        return !string.IsNullOrWhiteSpace(returnUrl)
            && returnUrl.StartsWith("/", StringComparison.Ordinal)
            && !returnUrl.StartsWith("//", StringComparison.Ordinal)
            && !returnUrl.StartsWith("/\\", StringComparison.Ordinal);
    }

    private static CartViewModel Map(CartDto cart)
    {
        return new CartViewModel
        {
            Id = cart.Id,
            ItemsCount = cart.ItemsCount,
            TotalUnits = cart.TotalUnits,
            TotalAmount = cart.TotalAmount,
            Currency = cart.Currency,
            IsReadyForCheckout = cart.IsReadyForCheckout,
            Items = cart.Items
                .Select(item => new CartItemViewModel
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    ProductSku = item.ProductSku,
                    MainImageUrl = item.MainImageUrl,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Subtotal = item.Subtotal,
                    Currency = item.Currency,
                    ItemTypeLabel = item.IsDigitalProduct ? "Digital" : "Físico"
                })
                .ToArray()
        };
    }

    /// <summary>
    /// Proyección del carrito mostrada en la UI del cliente.
    /// </summary>
    public sealed class CartViewModel
    {
        public Guid Id { get; init; }
        public IReadOnlyCollection<CartItemViewModel> Items { get; init; } = Array.Empty<CartItemViewModel>();
        public int ItemsCount { get; init; }
        public int TotalUnits { get; init; }
        public decimal TotalAmount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public bool IsReadyForCheckout { get; init; }
        public bool HasItems => ItemsCount > 0;
    }

    /// <summary>
    /// Proyección de una línea del carrito mostrada en la UI del cliente.
    /// </summary>
    public sealed class CartItemViewModel
    {
        public Guid Id { get; init; }
        public Guid ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public string ProductSku { get; init; } = string.Empty;
        public string? MainImageUrl { get; init; }
        public int Quantity { get; init; }
        public decimal UnitPrice { get; init; }
        public decimal Subtotal { get; init; }
        public string Currency { get; init; } = string.Empty;
        public string ItemTypeLabel { get; init; } = string.Empty;
    }
}
