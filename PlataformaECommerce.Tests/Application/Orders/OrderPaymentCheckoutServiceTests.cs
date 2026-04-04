using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Orders.Commands;
using PlataformaECommerce.Application.Features.Orders.DTOs;
using PlataformaECommerce.Application.Features.Orders.Services;
using PlataformaECommerce.Application.Interfaces.Repositories.Orders;
using PlataformaECommerce.Application.Interfaces.Services.Orders;
using PlataformaECommerce.Domain.Entities.Cart;
using PlataformaECommerce.Domain.Entities.Orders;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.ValueObjects;
using PlataformaECommerce.Infrastructure.Services.Products;

namespace PlataformaECommerce.Tests.Application.Orders;

[TestFixture]
public class OrderPaymentCheckoutServiceTests
{
    [Test]
    public async Task CreateCheckoutSessionAsync_PedidoConfirmadoConPagoEnLinea_RetornaSesionExterna()
    {
        Pedido order = CreateConfirmedOrder(MetodoPagoPedido.Tarjeta);
        FakePaymentGateway paymentGateway = new();
        OrderPaymentCheckoutService service = new(new FakeOrderRepository(order), paymentGateway, new FakePaymentService());

        Result<PaymentCheckoutSessionDto> result = await service.CreateCheckoutSessionAsync(new CreateOrderPaymentSessionCommand
        {
            OrderId = order.Id,
            ExpectedCustomerId = order.ClienteId,
            ReturnUrl = "https://shop.example.com/payments/confirm?orderId=" + order.Id,
            RequestedByUserId = order.ClienteId,
            RequestedAtUtc = DateTime.UtcNow
        });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.OrderId, Is.EqualTo(order.Id));
        Assert.That(paymentGateway.LastCheckoutRequest?.PaymentMethod, Is.EqualTo(MetodoPagoPedido.Tarjeta));
        Assert.That(paymentGateway.LastCheckoutRequest?.PaymentReference, Is.EqualTo($"PAY-{order.Id:N}"));
    }

    [Test]
    public async Task CreateCheckoutSessionAsync_ContraEntrega_RetornaConflicto()
    {
        Pedido order = CreateConfirmedOrder(MetodoPagoPedido.ContraEntrega);
        OrderPaymentCheckoutService service = new(new FakeOrderRepository(order), new FakePaymentGateway(), new FakePaymentService());

        Result<PaymentCheckoutSessionDto> result = await service.CreateCheckoutSessionAsync(new CreateOrderPaymentSessionCommand
        {
            OrderId = order.Id,
            ExpectedCustomerId = order.ClienteId,
            ReturnUrl = "https://shop.example.com/payments/confirm?orderId=" + order.Id
        });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo("Orders.CashOnDelivery"));
    }

    [Test]
    public async Task ConfirmPaymentReturnAsync_Aprobado_RegistraPago()
    {
        Pedido order = CreateConfirmedOrder(MetodoPagoPedido.Pse);
        FakePaymentGateway paymentGateway = new()
        {
            VerifiedTransaction = new PaymentGatewayTransactionDto
            {
                Provider = "Wompi",
                GatewayTransactionId = "tx-123",
                PaymentReference = $"PAY-{order.Id:N}",
                Status = PaymentGatewayTransactionStatus.Approved,
                PaymentMethod = "PSE",
                Amount = order.Total.Amount,
                Currency = order.Total.Currency,
                PaidAtUtc = DateTime.UtcNow
            }
        };
        FakePaymentService paymentService = new();
        OrderPaymentCheckoutService service = new(new FakeOrderRepository(order), paymentGateway, paymentService);

        Result<PaymentReturnResultDto> result = await service.ConfirmPaymentReturnAsync(new ConfirmOrderPaymentReturnCommand
        {
            OrderId = order.Id,
            GatewayTransactionId = "tx-123",
            RequestedByUserId = order.ClienteId,
            RequestedAtUtc = DateTime.UtcNow
        });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.IsApproved, Is.True);
        Assert.That(paymentService.LastCommand?.OrderId, Is.EqualTo(order.Id));
        Assert.That(paymentService.LastCommand?.PaymentReference, Is.EqualTo($"PAY-{order.Id:N}"));
    }

    [Test]
    public async Task ConfirmPaymentReturnAsync_Pendiente_NoRegistraPago()
    {
        Pedido order = CreateConfirmedOrder(MetodoPagoPedido.TransferenciaBancaria);
        FakePaymentGateway paymentGateway = new()
        {
            VerifiedTransaction = new PaymentGatewayTransactionDto
            {
                Provider = "Wompi",
                GatewayTransactionId = "tx-pending",
                PaymentReference = $"PAY-{order.Id:N}",
                Status = PaymentGatewayTransactionStatus.Pending,
                PaymentMethod = "BANK_TRANSFER",
                Amount = order.Total.Amount,
                Currency = order.Total.Currency
            }
        };
        FakePaymentService paymentService = new();
        OrderPaymentCheckoutService service = new(new FakeOrderRepository(order), paymentGateway, paymentService);

        Result<PaymentReturnResultDto> result = await service.ConfirmPaymentReturnAsync(new ConfirmOrderPaymentReturnCommand
        {
            OrderId = order.Id,
            GatewayTransactionId = "tx-pending"
        });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.IsApproved, Is.False);
        Assert.That(paymentService.LastCommand, Is.Null);
    }

    private static Pedido CreateConfirmedOrder(MetodoPagoPedido paymentMethod)
    {
        Guid customerId = Guid.NewGuid();
        CarritoCompra cart = new(customerId);
        var product = FabricaEntidades.CrearProductoFisico(
            "Portátil Pro",
            "Portátil profesional para trabajo intensivo.",
            4500000m,
            10,
            1.8m,
            2m,
            35m,
            24m,
            sku: "PORTATIL-PRO-001");

        product.Activar();
        cart.AgregarProducto(product, 1);

        Pedido order = new(cart);
        order.AsignarDireccionEnvio(new DireccionEnvio("Calle 123", "Bogotá", "Cundinamarca", "Colombia", "110111"));
        order.SeleccionarMetodoPago(paymentMethod);
        order.Confirmar();
        return order;
    }

    private sealed class FakeOrderRepository(Pedido order) : IOrderRepository
    {
        public Task<IReadOnlyCollection<Pedido>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Pedido>>(new[] { order });

        public Task<Pedido?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(id == order.Id ? order : null);

        public Task<IReadOnlyCollection<Pedido>> GetByCustomerIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Pedido>>(clienteId == order.ClienteId ? new[] { order } : Array.Empty<Pedido>());

        public Task<IReadOnlyCollection<Pedido>> GetByStatusAsync(EstadoPedido estado, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Pedido>>(order.Estado == estado ? new[] { order } : Array.Empty<Pedido>());

        public Task<IReadOnlyCollection<Pedido>> GetByCustomerIdAndStatusAsync(Guid clienteId, EstadoPedido estado, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Pedido>>(order.ClienteId == clienteId && order.Estado == estado ? new[] { order } : Array.Empty<Pedido>());

        public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(id == order.Id);

        public Task<bool> ExistsByCustomerIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
            => Task.FromResult(clienteId == order.ClienteId);

        public Task AddAsync(Pedido pedido, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateAsync(Pedido pedido, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakePaymentGateway : IPaymentGateway
    {
        public PaymentGatewayCheckoutRequestDto? LastCheckoutRequest { get; private set; }
        public PaymentGatewayTransactionDto VerifiedTransaction { get; set; } = new()
        {
            Provider = "Wompi",
            GatewayTransactionId = "tx-default",
            PaymentReference = string.Empty,
            Status = PaymentGatewayTransactionStatus.Approved,
            PaymentMethod = "CARD",
            Amount = 0,
            Currency = "COP"
        };

        public Task<Result<PaymentCheckoutSessionDto>> CreateCheckoutSessionAsync(PaymentGatewayCheckoutRequestDto request, CancellationToken cancellationToken = default)
        {
            LastCheckoutRequest = request;
            return Task.FromResult(Result.Success(new PaymentCheckoutSessionDto
            {
                Provider = "Wompi",
                CheckoutUrl = "https://checkout.wompi.co/p/?reference=" + request.PaymentReference,
                PaymentReference = request.PaymentReference,
                OrderId = request.OrderId
            }));
        }

        public Task<Result<PaymentGatewayTransactionDto>> VerifyTransactionAsync(string gatewayTransactionId, CancellationToken cancellationToken = default)
        {
            PaymentGatewayTransactionDto transaction = VerifiedTransaction with { GatewayTransactionId = gatewayTransactionId };
            return Task.FromResult(Result.Success(transaction));
        }
    }

    private sealed class FakePaymentService : IPaymentService
    {
        public RegisterOrderPaymentCommand? LastCommand { get; private set; }

        public Task<Result<OrderDetailDto>> RegisterOrderPaymentAsync(RegisterOrderPaymentCommand command, CancellationToken cancellationToken = default)
        {
            LastCommand = command;
            return Task.FromResult(Result.Success(new OrderDetailDto
            {
                Id = command.OrderId,
                CustomerId = Guid.NewGuid(),
                Status = EstadoPedido.Pagado,
                ItemsCount = 1,
                TotalUnits = 1,
                TotalAmount = command.Amount,
                Currency = command.Currency,
                CreatedAtUtc = DateTime.UtcNow,
                PaidAtUtc = command.PaidAtUtc ?? DateTime.UtcNow
            }));
        }
    }
}
