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
using PlataformaECommerce.Application.Interfaces.Services.Cart;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Web.Authorization;
using PlataformaECommerce.Web.Pages.Cart;

namespace PlataformaECommerce.Tests.Web.Cart;

[TestFixture]
public class CartIndexPageModelTests
{
    [Test]
    public async Task OnGetAsync_SinCarritoPrevio_CreaCarritoYRetornaPagina()
    {
        FakeCartApplicationService cartApplicationService = new();
        IndexModel pageModel = CreatePageModel(cartApplicationService, Guid.NewGuid());

        IActionResult result = await pageModel.OnGetAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(cartApplicationService.CreateCartCalls, Is.EqualTo(1));
    }

    [Test]
    public async Task OnPostAddItemAsync_ProductoValido_RedireccionaYRegistraComando()
    {
        FakeCartApplicationService cartApplicationService = new();
        IndexModel pageModel = CreatePageModel(cartApplicationService, Guid.NewGuid());

        IActionResult result = await pageModel.OnPostAddItemAsync(Guid.NewGuid(), 2, "/Catalog/Index", CancellationToken.None);

        Assert.That(result, Is.TypeOf<LocalRedirectResult>());
        Assert.That(cartApplicationService.LastAddCommand?.Quantity, Is.EqualTo(2));
    }

    private static IndexModel CreatePageModel(FakeCartApplicationService cartApplicationService, Guid? authenticatedUserId)
    {
        FakeAuthenticationService authenticationService = new();
        IndexModel pageModel = new(cartApplicationService);

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
        public int CreateCartCalls { get; private set; }
        public AddProductToCartCommand? LastAddCommand { get; private set; }
        public CartDto CurrentCart { get; private set; } = CreateCart();
        public bool ReturnNotFoundOnFirstGet { get; set; } = true;

        public Task<Result<CartDto>> CreateCartAsync(CreateCartCommand command, CancellationToken cancellationToken = default)
        {
            CreateCartCalls++;
            CurrentCart = CreateCart(command.CustomerId);
            ReturnNotFoundOnFirstGet = false;
            return Task.FromResult(Result.Success(CurrentCart));
        }

        public Task<Result<CartDto>> AddProductToCartAsync(AddProductToCartCommand command, CancellationToken cancellationToken = default)
        {
            LastAddCommand = command;
            CurrentCart = new CartDto
            {
                Id = CurrentCart.Id,
                CustomerId = CurrentCart.CustomerId,
                IsActive = CurrentCart.IsActive,
                Items =
                [
                    new CartItemDto
                    {
                        Id = Guid.NewGuid(),
                        ProductId = command.ProductId,
                        ProductName = "Producto demo",
                        ProductSku = "SKU-001",
                        ProductType = TipoProducto.Fisico,
                        Quantity = command.Quantity,
                        UnitPrice = 99900m,
                        Currency = "COP",
                        Subtotal = 99900m * command.Quantity
                    }
                ],
                ItemsCount = 1,
                TotalUnits = command.Quantity,
                TotalAmount = 99900m * command.Quantity,
                Currency = "COP"
            };

            return Task.FromResult(Result.Success(CurrentCart));
        }

        public Task<Result<CartDto>> UpdateCartItemQuantityAsync(UpdateCartItemQuantityCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(CurrentCart));

        public Task<Result<CartDto>> RemoveProductFromCartAsync(RemoveProductFromCartCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(CurrentCart));

        public Task<Result<CartDto>> ClearCartAsync(ClearCartCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(CurrentCart));

        public Task<Result<CartDto>> GetCartByCustomerIdAsync(GetCartByCustomerIdQuery query, CancellationToken cancellationToken = default)
        {
            if (ReturnNotFoundOnFirstGet)
            {
                return Task.FromResult(Result.Failure<CartDto>(Error.NotFound("Cart.NotFoundByCustomer", "Carrito no encontrado.")));
            }

            return Task.FromResult(Result.Success(CurrentCart));
        }

        private static CartDto CreateCart(Guid? customerId = null)
        {
            return new CartDto
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId ?? Guid.NewGuid(),
                IsActive = true,
                Currency = "COP"
            };
        }
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
