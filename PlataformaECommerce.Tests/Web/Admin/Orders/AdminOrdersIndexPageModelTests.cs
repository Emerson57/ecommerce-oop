using System.Security.Claims;
using Microsoft.AspNetCore.Http;
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
public class AdminOrdersIndexPageModelTests
{
    [Test]
    public async Task OnGetAsync_SinFiltros_CargaListadoAdministrativo()
    {
        IndexModel pageModel = CreatePageModel(new FakeOrderApplicationService());

        await pageModel.OnGetAsync(CancellationToken.None);

        Assert.That(pageModel.Orders.Count, Is.EqualTo(1));
        Assert.That(pageModel.Orders.First().StatusLabel, Is.EqualTo("En proceso"));
    }

    [Test]
    public async Task OnGetAsync_ConFiltros_PropagaConsultaAdministrativa()
    {
        FakeOrderApplicationService service = new();
        IndexModel pageModel = CreatePageModel(service);
        pageModel.Status = EstadoPedido.EnProceso;
        pageModel.Condition = IndexModel.OrderConditionFilter.Active;
        pageModel.CreatedFrom = new DateOnly(2026, 1, 1);
        pageModel.CreatedTo = new DateOnly(2026, 1, 31);
        pageModel.MinTotalAmount = 100m;
        pageModel.MaxTotalAmount = 500m;

        await pageModel.OnGetAsync(CancellationToken.None);

        Assert.That(service.LastGetOrdersQuery?.Status, Is.EqualTo(EstadoPedido.EnProceso));
        Assert.That(service.LastGetOrdersQuery?.OnlyActive, Is.True);
        Assert.That(service.LastGetOrdersQuery?.OnlyFinalized, Is.Null);
        Assert.That(service.LastGetOrdersQuery?.MinTotalAmount, Is.EqualTo(100m));
        Assert.That(service.LastGetOrdersQuery?.MaxTotalAmount, Is.EqualTo(500m));
    }

    [Test]
    public async Task OnGetAsync_ConPedidoSeleccionado_CargaResumenDelPedido()
    {
        FakeOrderApplicationService service = new();
        Guid selectedOrderId = Guid.NewGuid();
        IndexModel pageModel = CreatePageModel(service);
        pageModel.SelectedOrderId = selectedOrderId;

        await pageModel.OnGetAsync(CancellationToken.None);

        Assert.That(service.LastGetOrderByIdQuery?.OrderId, Is.EqualTo(selectedOrderId));
        Assert.That(pageModel.SelectedOrder, Is.Not.Null);
        Assert.That(pageModel.SelectedOrder!.CustomerId, Is.EqualTo(service.OrderCustomerId));
    }

    [Test]
    public async Task OnGetAsync_RangoInvalido_NoConsultaPedidos()
    {
        FakeOrderApplicationService service = new();
        IndexModel pageModel = CreatePageModel(service);
        pageModel.CreatedFrom = new DateOnly(2026, 2, 1);
        pageModel.CreatedTo = new DateOnly(2026, 1, 1);

        await pageModel.OnGetAsync(CancellationToken.None);

        Assert.That(service.LastGetOrdersQuery, Is.Null);
        Assert.That(pageModel.ModelState[nameof(IndexModel.CreatedFrom)]?.Errors, Has.Count.EqualTo(1));
    }

    private static IndexModel CreatePageModel(FakeOrderApplicationService orderApplicationService)
    {
        IndexModel pageModel = new(orderApplicationService);
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
        public Guid OrderCustomerId { get; } = Guid.NewGuid();
        public GetOrdersQuery? LastGetOrdersQuery { get; private set; }
        public GetOrderByIdQuery? LastGetOrderByIdQuery { get; private set; }

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
            => throw new NotSupportedException();

        public Task<Result<OrderDetailDto>> GetOrderByIdAsync(GetOrderByIdQuery query, CancellationToken cancellationToken = default)
        {
            LastGetOrderByIdQuery = query;
            return Task.FromResult(Result.Success(new OrderDetailDto
            {
                Id = query.OrderId,
                CustomerId = OrderCustomerId,
                Status = EstadoPedido.EnProceso,
                ItemsCount = 2,
                TotalUnits = 3,
                TotalAmount = 249900m,
                Currency = "COP",
                CreatedAtUtc = DateTime.UtcNow.AddDays(-2),
                ConfirmedAtUtc = DateTime.UtcNow.AddDays(-2),
                PaidAtUtc = DateTime.UtcNow.AddDays(-1),
                ContainsPhysicalProducts = true,
                ShippingStreet = "Calle 10 #20-30",
                ShippingCity = "Bogotá",
                ShippingDepartment = "Cundinamarca",
                ShippingCountry = "Colombia",
                ShippingPostalCode = "110111"
            }));
        }

        public Task<Result<IReadOnlyCollection<OrderDto>>> GetOrdersAsync(GetOrdersQuery query, CancellationToken cancellationToken = default)
        {
            LastGetOrdersQuery = query;
            IReadOnlyCollection<OrderDto> orders =
            [
                new OrderDto
                {
                    Id = Guid.NewGuid(),
                    CustomerId = OrderCustomerId,
                    Status = EstadoPedido.EnProceso,
                    ItemsCount = 2,
                    TotalUnits = 3,
                    TotalAmount = 249900m,
                    Currency = "COP",
                    CreatedAtUtc = DateTime.UtcNow.AddDays(-2)
                }
            ];

            return Task.FromResult(Result.Success(orders));
        }

        public Task<Result<IReadOnlyCollection<OrderDto>>> GetOrdersByCustomerIdAsync(GetOrdersByCustomerIdQuery query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class FakeTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}