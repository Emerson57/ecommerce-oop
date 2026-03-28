using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Orders.Commands;
using PlataformaECommerce.Application.Features.Orders.DTOs;
using PlataformaECommerce.Application.Features.Orders.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Orders;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Web.Pages.Admin.Orders;

namespace PlataformaECommerce.Tests.Web.Admin.Orders;

[TestFixture]
public class AdminOrdersDetailsPageModelTests
{
    [Test]
    public async Task OnGetAsync_PedidoPendiente_CargaDetalleYExponeConfirmacion()
    {
        FakeOrderApplicationService service = new()
        {
            CurrentStatus = EstadoPedido.Pendiente
        };
        DetailsModel pageModel = CreatePageModel(service);

        IActionResult result = await pageModel.OnGetAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(pageModel.Order.CanConfirm, Is.True);
        Assert.That(pageModel.Order.CanRegisterPayment, Is.False);
    }

    [Test]
    public async Task OnPostConfirmAsync_PedidoElegible_ConsumeConfirmacionYRedirige()
    {
        FakeOrderApplicationService service = new()
        {
            CurrentStatus = EstadoPedido.Pendiente
        };
        DetailsModel pageModel = CreatePageModel(service);
        Guid orderId = Guid.NewGuid();
        pageModel.ReturnUrl = "/Admin/Orders/Index?Status=Pendiente";
        pageModel.ConfirmOrder = new DetailsModel.ConfirmOrderInputModel
        {
            Notes = "Validación administrativa"
        };

        IActionResult result = await pageModel.OnPostConfirmAsync(orderId, CancellationToken.None);

        Assert.That(result, Is.TypeOf<RedirectToPageResult>());
        RedirectToPageResult redirect = (RedirectToPageResult)result;
        Assert.That(redirect.PageName, Is.EqualTo("/Admin/Orders/Details"));
        Assert.That(service.LastConfirmOrderCommand?.OrderId, Is.EqualTo(orderId));
        Assert.That(service.LastConfirmOrderCommand?.Notes, Is.EqualTo("Validación administrativa"));
        Assert.That(redirect.RouteValues?[nameof(DetailsModel.ReturnUrl)], Is.EqualTo("/Admin/Orders/Index?Status=Pendiente"));
    }

    [Test]
    public async Task OnPostRegisterPaymentAsync_PedidoConfirmado_ConsumeRegistroDePago()
    {
        FakeOrderApplicationService service = new()
        {
            CurrentStatus = EstadoPedido.Confirmado
        };
        DetailsModel pageModel = CreatePageModel(service);
        Guid orderId = Guid.NewGuid();
        pageModel.RegisterPayment = new DetailsModel.RegisterPaymentInputModel
        {
            PaymentReference = "PAY-2026-001",
            PaymentMethod = "Transferencia",
            Amount = 249900m,
            Currency = "COP",
            PaymentProvider = "Banco Demo"
        };

        IActionResult result = await pageModel.OnPostRegisterPaymentAsync(orderId, CancellationToken.None);

        Assert.That(result, Is.TypeOf<RedirectToPageResult>());
        Assert.That(service.LastRegisterOrderPaymentCommand?.OrderId, Is.EqualTo(orderId));
        Assert.That(service.LastRegisterOrderPaymentCommand?.PaymentReference, Is.EqualTo("PAY-2026-001"));
        Assert.That(service.LastRegisterOrderPaymentCommand?.PaymentMethod, Is.EqualTo("Transferencia"));
    }

    [Test]
    public async Task OnPostShipAsync_PedidoNoElegible_NoInvocaDespacho()
    {
        FakeOrderApplicationService service = new()
        {
            CurrentStatus = EstadoPedido.Confirmado
        };
        DetailsModel pageModel = CreatePageModel(service);
        pageModel.ShipOrder = new DetailsModel.ShipOrderInputModel
        {
            CarrierName = "Operador Demo",
            TrackingNumber = "TRK-001"
        };

        IActionResult result = await pageModel.OnPostShipAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(service.LastShipOrderCommand, Is.Null);
        Assert.That(pageModel.ErrorMessage, Does.Contain("no puede enviarse"));
    }

    [Test]
    public async Task OnPostCancelAsync_MotivoInvalido_NoInvocaCancelacion()
    {
        FakeOrderApplicationService service = new()
        {
            CurrentStatus = EstadoPedido.EnProceso
        };
        DetailsModel pageModel = CreatePageModel(service);
        pageModel.CancelOrder = new DetailsModel.CancelOrderInputModel
        {
            Reason = "abc"
        };

        IActionResult result = await pageModel.OnPostCancelAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(service.LastCancelOrderCommand, Is.Null);
        Assert.That(pageModel.ModelState[$"{nameof(DetailsModel.CancelOrder)}.{nameof(DetailsModel.CancelOrderInputModel.Reason)}"]?.Errors, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task OnPostDeliverAsync_PedidoEnviado_ConsumeEntrega()
    {
        FakeOrderApplicationService service = new()
        {
            CurrentStatus = EstadoPedido.Enviado
        };
        DetailsModel pageModel = CreatePageModel(service);
        Guid orderId = Guid.NewGuid();
        pageModel.DeliverOrder = new DetailsModel.DeliverOrderInputModel
        {
            ReceivedBy = "Recepción principal",
            DeliveryEvidence = "Firma digital"
        };

        IActionResult result = await pageModel.OnPostDeliverAsync(orderId, CancellationToken.None);

        Assert.That(result, Is.TypeOf<RedirectToPageResult>());
        Assert.That(service.LastDeliverOrderCommand?.OrderId, Is.EqualTo(orderId));
        Assert.That(service.LastDeliverOrderCommand?.ReceivedBy, Is.EqualTo("Recepción principal"));
    }

    private static DetailsModel CreatePageModel(FakeOrderApplicationService orderApplicationService)
    {
        DetailsModel pageModel = new(orderApplicationService);
        ClaimsPrincipal principal = new(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, "Admin Demo"),
            new Claim(ClaimTypes.Email, "admin@plataforma.com"),
            new Claim(ClaimTypes.Role, "Administrador"),
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        ], "AdminCookie"));

        DefaultHttpContext httpContext = new()
        {
            User = principal
        };

        pageModel.PageContext = new PageContext
        {
            HttpContext = httpContext
        };
        pageModel.TempData = new TempDataDictionary(httpContext, new FakeTempDataProvider());
        return pageModel;
    }

    private sealed class FakeOrderApplicationService : IOrderApplicationService
    {
        public EstadoPedido CurrentStatus { get; set; } = EstadoPedido.Pendiente;
        public ConfirmOrderCommand? LastConfirmOrderCommand { get; private set; }
        public RegisterOrderPaymentCommand? LastRegisterOrderPaymentCommand { get; private set; }
        public ProcessOrderCommand? LastProcessOrderCommand { get; private set; }
        public ShipOrderCommand? LastShipOrderCommand { get; private set; }
        public DeliverOrderCommand? LastDeliverOrderCommand { get; private set; }
        public CancelOrderCommand? LastCancelOrderCommand { get; private set; }

        public Task<Result<OrderDetailDto>> CreateOrderFromCartAsync(CreateOrderFromCartCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<OrderDetailDto>> ConfirmOrderAsync(ConfirmOrderCommand command, CancellationToken cancellationToken = default)
        {
            LastConfirmOrderCommand = command;
            CurrentStatus = EstadoPedido.Confirmado;
            return Task.FromResult(Result.Success(BuildOrderDetail(command.OrderId)));
        }

        public Task<Result<OrderDetailDto>> RegisterOrderPaymentAsync(RegisterOrderPaymentCommand command, CancellationToken cancellationToken = default)
        {
            LastRegisterOrderPaymentCommand = command;
            CurrentStatus = EstadoPedido.Pagado;
            return Task.FromResult(Result.Success(BuildOrderDetail(command.OrderId)));
        }

        public Task<Result<OrderDetailDto>> ProcessOrderAsync(ProcessOrderCommand command, CancellationToken cancellationToken = default)
        {
            LastProcessOrderCommand = command;
            CurrentStatus = EstadoPedido.EnProceso;
            return Task.FromResult(Result.Success(BuildOrderDetail(command.OrderId)));
        }

        public Task<Result<OrderDetailDto>> ShipOrderAsync(ShipOrderCommand command, CancellationToken cancellationToken = default)
        {
            LastShipOrderCommand = command;
            CurrentStatus = EstadoPedido.Enviado;
            return Task.FromResult(Result.Success(BuildOrderDetail(command.OrderId)));
        }

        public Task<Result<OrderDetailDto>> DeliverOrderAsync(DeliverOrderCommand command, CancellationToken cancellationToken = default)
        {
            LastDeliverOrderCommand = command;
            CurrentStatus = EstadoPedido.Entregado;
            return Task.FromResult(Result.Success(BuildOrderDetail(command.OrderId)));
        }

        public Task<Result<OrderDetailDto>> CancelOrderAsync(CancelOrderCommand command, CancellationToken cancellationToken = default)
        {
            LastCancelOrderCommand = command;
            CurrentStatus = EstadoPedido.Cancelado;
            return Task.FromResult(Result.Success(BuildOrderDetail(command.OrderId, cancellationReason: command.Reason.Trim())));
        }

        public Task<Result<OrderDetailDto>> GetOrderByIdAsync(GetOrderByIdQuery query, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(BuildOrderDetail(query.OrderId)));

        public Task<Result<IReadOnlyCollection<OrderDto>>> GetOrdersAsync(GetOrdersQuery query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<IReadOnlyCollection<OrderDto>>> GetOrdersByCustomerIdAsync(GetOrdersByCustomerIdQuery query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        private OrderDetailDto BuildOrderDetail(Guid orderId, string? cancellationReason = null)
        {
            return new OrderDetailDto
            {
                Id = orderId,
                CustomerId = Guid.NewGuid(),
                Status = CurrentStatus,
                ItemsCount = 1,
                TotalUnits = 2,
                TotalAmount = 249900m,
                Currency = "COP",
                CreatedAtUtc = DateTime.UtcNow.AddDays(-2),
                ConfirmedAtUtc = CurrentStatus is EstadoPedido.Confirmado or EstadoPedido.Pagado or EstadoPedido.EnProceso or EstadoPedido.Enviado or EstadoPedido.Entregado ? DateTime.UtcNow.AddDays(-2) : null,
                PaidAtUtc = CurrentStatus is EstadoPedido.Pagado or EstadoPedido.EnProceso or EstadoPedido.Enviado or EstadoPedido.Entregado ? DateTime.UtcNow.AddDays(-1) : null,
                ShippedAtUtc = CurrentStatus is EstadoPedido.Enviado or EstadoPedido.Entregado ? DateTime.UtcNow.AddHours(-12) : null,
                DeliveredAtUtc = CurrentStatus == EstadoPedido.Entregado ? DateTime.UtcNow.AddHours(-1) : null,
                CancelledAtUtc = CurrentStatus == EstadoPedido.Cancelado ? DateTime.UtcNow.AddHours(-1) : null,
                CancellationReason = CurrentStatus == EstadoPedido.Cancelado ? cancellationReason ?? "Cancelación administrativa" : null,
                ContainsPhysicalProducts = true,
                ShippingStreet = "Calle 10 #20-30",
                ShippingCity = "Bogotá",
                ShippingDepartment = "Cundinamarca",
                ShippingCountry = "Colombia",
                ShippingPostalCode = "110111",
                Items =
                [
                    new OrderItemDto
                    {
                        Id = Guid.NewGuid(),
                        OrderId = orderId,
                        ProductId = Guid.NewGuid(),
                        ProductName = "Producto demo",
                        ProductSku = "SKU-001",
                        ProductType = TipoProducto.Fisico,
                        Quantity = 2,
                        UnitPrice = 124950m,
                        Currency = "COP",
                        Subtotal = 249900m,
                        CreatedAtUtc = DateTime.UtcNow.AddDays(-2)
                    }
                ]
            };
        }
    }

    private sealed class FakeTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}