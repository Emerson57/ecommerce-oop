using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Orders.Commands;
using PlataformaECommerce.Application.Features.Orders.DTOs;
using PlataformaECommerce.Application.Features.Orders.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Orders;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Web.Authorization;
using PlataformaECommerce.Web.Pages.Orders;

namespace PlataformaECommerce.Tests.Web.Orders;

[TestFixture]
public class OrdersDetailsPageModelTests
{
    [Test]
    public async Task OnGetAsync_PedidoPropio_CargaDetalleYRetornaPagina()
    {
        FakeOrderApplicationService orderApplicationService = new();
        DetailsModel pageModel = CreatePageModel(orderApplicationService, Guid.NewGuid());

        IActionResult result = await pageModel.OnGetAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(pageModel.Order.Items.Count, Is.EqualTo(1));
        Assert.That(pageModel.Order.ShippingStreet, Is.EqualTo("Calle 10 #20-30"));
    }

    [Test]
    public async Task OnGetAsync_PedidoInvalido_RedireccionaAlHistorial()
    {
        FakeOrderApplicationService orderApplicationService = new();
        DetailsModel pageModel = CreatePageModel(orderApplicationService, Guid.NewGuid());

        IActionResult result = await pageModel.OnGetAsync(Guid.Empty, CancellationToken.None);

        Assert.That(result, Is.TypeOf<RedirectToPageResult>());
        Assert.That(pageModel.StatusMessage, Does.Contain("pedido válido"));
    }

    [Test]
    public async Task OnGetAsync_PedidoDigitalPuro_ComunicaModalidadSinEnvioFisico()
    {
        FakeOrderApplicationService orderApplicationService = new()
        {
            ReturnsDigitalOnlyOrder = true
        };
        DetailsModel pageModel = CreatePageModel(orderApplicationService, Guid.NewGuid());

        IActionResult result = await pageModel.OnGetAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(pageModel.Order.HasShippingAddress, Is.False);
        Assert.That(pageModel.Order.IsDigitalOnly, Is.True);
        Assert.That(pageModel.Order.FulfillmentLabel, Is.EqualTo("Pedido digital"));
    }

    [Test]
    public async Task OnPostCancelAsync_PedidoElegible_ConsumeCancelacionYRedirigeAlMismoDetalle()
    {
        FakeOrderApplicationService orderApplicationService = new();
        DetailsModel pageModel = CreatePageModel(orderApplicationService, Guid.NewGuid());
        Guid orderId = Guid.NewGuid();
        pageModel.Cancellation = new DetailsModel.CancelOrderInputModel
        {
            Reason = "Necesito cambiar la compra"
        };

        IActionResult result = await pageModel.OnPostCancelAsync(orderId, CancellationToken.None);

        Assert.That(result, Is.TypeOf<RedirectToPageResult>());
        Assert.That(orderApplicationService.LastCancelOrderCommand?.OrderId, Is.EqualTo(orderId));
        Assert.That(orderApplicationService.LastCancelOrderCommand?.Reason, Is.EqualTo("Necesito cambiar la compra"));
        Assert.That(orderApplicationService.LastCancelOrderCommand?.RequestedByCustomer, Is.True);
        Assert.That(pageModel.StatusMessage, Does.Contain("cancelado correctamente"));
    }

    [Test]
    public async Task OnPostCancelAsync_MotivoInvalido_NoInvocaCancelacion()
    {
        FakeOrderApplicationService orderApplicationService = new();
        DetailsModel pageModel = CreatePageModel(orderApplicationService, Guid.NewGuid());
        pageModel.Cancellation = new DetailsModel.CancelOrderInputModel
        {
            Reason = "abc"
        };

        IActionResult result = await pageModel.OnPostCancelAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(orderApplicationService.LastCancelOrderCommand, Is.Null);
        Assert.That(pageModel.ModelState[$"{nameof(DetailsModel.Cancellation)}.{nameof(DetailsModel.CancelOrderInputModel.Reason)}"]?.Errors, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task OnPostCancelAsync_PedidoNoElegible_RetornaPaginaConError()
    {
        FakeOrderApplicationService orderApplicationService = new()
        {
            CurrentStatus = EstadoPedido.Enviado
        };
        DetailsModel pageModel = CreatePageModel(orderApplicationService, Guid.NewGuid());
        pageModel.Cancellation = new DetailsModel.CancelOrderInputModel
        {
            Reason = "Necesito cambiar la compra"
        };

        IActionResult result = await pageModel.OnPostCancelAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(orderApplicationService.LastCancelOrderCommand, Is.Null);
        Assert.That(pageModel.ErrorMessage, Does.Contain("ya no puede cancelarse"));
    }

    private static DetailsModel CreatePageModel(
        FakeOrderApplicationService orderApplicationService,
        Guid? authenticatedUserId,
        FakeAuthenticationService? authenticationService = null)
    {
        authenticationService ??= new FakeAuthenticationService();
        DetailsModel pageModel = new(orderApplicationService);

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

    private sealed class FakeOrderApplicationService : IOrderApplicationService
    {
        public bool ReturnsDigitalOnlyOrder { get; set; }
        public EstadoPedido CurrentStatus { get; set; } = EstadoPedido.EnProceso;
        public CancelOrderCommand? LastCancelOrderCommand { get; private set; }

        public Task<Result<OrderDetailDto>> CreateOrderFromCartAsync(CreateOrderFromCartCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

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
        {
            LastCancelOrderCommand = command;
            CurrentStatus = EstadoPedido.Cancelado;

            return Task.FromResult(Result.Success(new OrderDetailDto
            {
                Id = command.OrderId,
                CustomerId = command.RequestedByUserId ?? Guid.NewGuid(),
                Status = EstadoPedido.Cancelado,
                ItemsCount = 1,
                TotalUnits = 1,
                TotalAmount = 99900m,
                Currency = "COP",
                CreatedAtUtc = DateTime.UtcNow.AddDays(-4),
                CancelledAtUtc = DateTime.UtcNow,
                CancellationReason = command.Reason.Trim(),
                ContainsPhysicalProducts = true,
                Items =
                [
                    new OrderItemDto
                    {
                        Id = Guid.NewGuid(),
                        OrderId = command.OrderId,
                        ProductId = Guid.NewGuid(),
                        ProductName = "Producto demo",
                        ProductSku = "SKU-001",
                        ProductType = TipoProducto.Fisico,
                        Quantity = 1,
                        UnitPrice = 99900m,
                        Currency = "COP",
                        Subtotal = 99900m,
                        CreatedAtUtc = DateTime.UtcNow.AddDays(-4)
                    }
                ]
            }));
        }

        public Task<Result<OrderDetailDto>> GetOrderByIdAsync(GetOrderByIdQuery query, CancellationToken cancellationToken = default)
        {
            TipoProducto productType = ReturnsDigitalOnlyOrder ? TipoProducto.Digital : TipoProducto.Fisico;

            return Task.FromResult(Result.Success(new OrderDetailDto
            {
                Id = query.OrderId,
                CustomerId = query.ExpectedCustomerId ?? Guid.NewGuid(),
                Status = CurrentStatus,
                ItemsCount = 1,
                TotalUnits = 1,
                TotalAmount = 99900m,
                Currency = "COP",
                CreatedAtUtc = DateTime.UtcNow.AddDays(-4),
                ConfirmedAtUtc = CurrentStatus is EstadoPedido.Pendiente ? null : DateTime.UtcNow.AddDays(-4),
                PaidAtUtc = CurrentStatus is EstadoPedido.Pagado or EstadoPedido.EnProceso or EstadoPedido.Enviado or EstadoPedido.Entregado ? DateTime.UtcNow.AddDays(-4) : null,
                ShippedAtUtc = ReturnsDigitalOnlyOrder || CurrentStatus is not (EstadoPedido.Enviado or EstadoPedido.Entregado) ? null : DateTime.UtcNow.AddDays(-3),
                CancelledAtUtc = CurrentStatus == EstadoPedido.Cancelado ? DateTime.UtcNow.AddMinutes(-5) : null,
                CancellationReason = CurrentStatus == EstadoPedido.Cancelado ? "Cancelado por el cliente" : null,
                ShippingStreet = ReturnsDigitalOnlyOrder ? null : "Calle 10 #20-30",
                ShippingCity = ReturnsDigitalOnlyOrder ? null : "Bogotá",
                ShippingDepartment = ReturnsDigitalOnlyOrder ? null : "Cundinamarca",
                ShippingCountry = ReturnsDigitalOnlyOrder ? null : "Colombia",
                ShippingPostalCode = ReturnsDigitalOnlyOrder ? null : "110111",
                ContainsPhysicalProducts = !ReturnsDigitalOnlyOrder,
                ContainsDigitalProducts = ReturnsDigitalOnlyOrder,
                Items =
                [
                    new OrderItemDto
                    {
                        Id = Guid.NewGuid(),
                        OrderId = query.OrderId,
                        ProductId = Guid.NewGuid(),
                        ProductName = "Producto demo",
                        ProductSku = "SKU-001",
                        ProductType = productType,
                        Quantity = 1,
                        UnitPrice = 99900m,
                        Currency = "COP",
                        Subtotal = 99900m,
                        CreatedAtUtc = DateTime.UtcNow.AddDays(-4)
                    }
                ]
            }));
        }

        public Task<Result<IReadOnlyCollection<OrderDto>>> GetOrdersByCustomerIdAsync(GetOrdersByCustomerIdQuery query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class FakeAuthenticationService : IAuthenticationService
    {
        public string? LastSignOutScheme { get; private set; }

        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme)
            => Task.FromResult(AuthenticateResult.NoResult());

        public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
        {
            LastSignOutScheme = scheme;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
