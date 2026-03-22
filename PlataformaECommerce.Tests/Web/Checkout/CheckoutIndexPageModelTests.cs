using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Cart.Commands;
using PlataformaECommerce.Application.Features.Cart.DTOs;
using PlataformaECommerce.Application.Features.Cart.Queries;
using PlataformaECommerce.Application.Features.Orders.Commands;
using PlataformaECommerce.Application.Features.Orders.DTOs;
using PlataformaECommerce.Application.Features.Orders.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Cart;
using PlataformaECommerce.Application.Interfaces.Services.Orders;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Web.Authorization;
using PlataformaECommerce.Web.Pages.Checkout;

namespace PlataformaECommerce.Tests.Web.Checkout;

[TestFixture]
public class CheckoutIndexPageModelTests
{
    [Test]
    public async Task OnGetAsync_CarritoConItems_RetornaPagina()
    {
        FakeCartApplicationService cartApplicationService = new();
        IndexModel pageModel = CreatePageModel(cartApplicationService, new FakeOrderApplicationService(), Guid.NewGuid());

        IActionResult result = await pageModel.OnGetAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(pageModel.Cart.ItemsCount, Is.EqualTo(1));
    }

    [Test]
    public async Task OnPostPlaceOrderAsync_Confirmado_RedireccionaADetallePedido()
    {
        FakeCartApplicationService cartApplicationService = new();
        FakeOrderApplicationService orderApplicationService = new();
        IndexModel pageModel = CreatePageModel(cartApplicationService, orderApplicationService, Guid.NewGuid());
        pageModel.Input = new IndexModel.CheckoutInputModel
        {
            ConfirmOrderCreation = true,
            Notes = "Entrega prioritaria"
        };

        IActionResult result = await pageModel.OnPostPlaceOrderAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<RedirectToPageResult>());
        Assert.That(orderApplicationService.LastCreateOrderCommand?.Notes, Is.EqualTo("Entrega prioritaria"));
    }

    private static IndexModel CreatePageModel(
        FakeCartApplicationService cartApplicationService,
        FakeOrderApplicationService orderApplicationService,
        Guid? authenticatedUserId)
    {
        FakeAuthenticationService authenticationService = new();
        IndexModel pageModel = new(cartApplicationService, orderApplicationService);

        ServiceCollection services = new();
        services.AddSingleton<IAuthenticationService>(authenticationService);
        DefaultHttpContext httpContext = new()
        {
            RequestServices = services.BuildServiceProvider(),
            User = CreatePrincipal(authenticatedUserId)
        };

        pageModel.PageContext = new PageContext
        {
            HttpContext = httpContext
        };
        pageModel.TempData = new TempDataDictionary(httpContext, new FakeTempDataProvider());

        return pageModel;
    }

    private static ClaimsPrincipal CreatePrincipal(Guid? authenticatedUserId)
    {
        List<Claim> claims =
        [
            new Claim(ClaimTypes.Name, "Cliente Demo"),
            new Claim(ClaimTypes.Role, RolUsuario.Cliente.ToString()),
            new Claim(AuthorizationPolicies.PrimaryRoleClaimType, RolUsuario.Cliente.ToString()),
            new Claim(AuthorizationPolicies.SuperUserClaimType, bool.FalseString)
        ];

        if (authenticatedUserId.HasValue)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, authenticatedUserId.Value.ToString()));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, AuthorizationPolicies.CustomerCookieScheme));
    }

    private sealed class FakeCartApplicationService : ICartApplicationService
    {
        public Task<Result<CartDto>> CreateCartAsync(CreateCartCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(CreateCart(command.CustomerId)));

        public Task<Result<CartDto>> AddProductToCartAsync(AddProductToCartCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<CartDto>> UpdateCartItemQuantityAsync(UpdateCartItemQuantityCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<CartDto>> RemoveProductFromCartAsync(RemoveProductFromCartCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<CartDto>> ClearCartAsync(ClearCartCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<CartDto>> GetCartByCustomerIdAsync(GetCartByCustomerIdQuery query, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(CreateCart(query.CustomerId)));

        private static CartDto CreateCart(Guid customerId)
        {
            return new CartDto
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                IsActive = true,
                Items =
                [
                    new CartItemDto
                    {
                        Id = Guid.NewGuid(),
                        ProductId = Guid.NewGuid(),
                        ProductName = "Producto demo",
                        ProductSku = "SKU-001",
                        ProductType = TipoProducto.Fisico,
                        Quantity = 1,
                        UnitPrice = 99900m,
                        Currency = "COP",
                        Subtotal = 99900m
                    }
                ],
                ItemsCount = 1,
                TotalUnits = 1,
                TotalAmount = 99900m,
                Currency = "COP"
            };
        }
    }

    private sealed class FakeOrderApplicationService : IOrderApplicationService
    {
        public CreateOrderFromCartCommand? LastCreateOrderCommand { get; private set; }

        public Task<Result<OrderDetailDto>> CreateOrderFromCartAsync(CreateOrderFromCartCommand command, CancellationToken cancellationToken = default)
        {
            LastCreateOrderCommand = command;
            return Task.FromResult(Result.Success(new OrderDetailDto
            {
                Id = Guid.NewGuid(),
                CustomerId = command.CustomerId,
                Status = EstadoPedido.Confirmado,
                ItemsCount = 1,
                TotalUnits = 1,
                TotalAmount = 99900m,
                Currency = "COP",
                CreatedAtUtc = DateTime.UtcNow
            }));
        }

        public Task<Result<OrderDetailDto>> ConfirmOrderAsync(ConfirmOrderCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
        public Task<Result<OrderDetailDto>> RegisterOrderPaymentAsync(RegisterOrderPaymentCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
        public Task<Result<OrderDetailDto>> ProcessOrderAsync(ProcessOrderCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
        public Task<Result<OrderDetailDto>> ShipOrderAsync(ShipOrderCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
        public Task<Result<OrderDetailDto>> DeliverOrderAsync(DeliverOrderCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
        public Task<Result<OrderDetailDto>> CancelOrderAsync(CancelOrderCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
        public Task<Result<OrderDetailDto>> GetOrderByIdAsync(GetOrderByIdQuery query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
        public Task<Result<IReadOnlyCollection<OrderDto>>> GetOrdersByCustomerIdAsync(GetOrdersByCustomerIdQuery query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class FakeAuthenticationService : IAuthenticationService
    {
        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme)
            => Task.FromResult(AuthenticateResult.NoResult());

        public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;
    }

    private sealed class FakeTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
