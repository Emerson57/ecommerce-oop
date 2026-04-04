using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Orders.Commands;
using PlataformaECommerce.Application.Features.Orders.DTOs;
using PlataformaECommerce.Application.Features.Orders.Services;
using PlataformaECommerce.Application.Interfaces.Persistence;
using PlataformaECommerce.Application.Interfaces.Repositories.Orders;
using PlataformaECommerce.Application.Interfaces.Repositories.Products;
using PlataformaECommerce.Application.Interfaces.Services.Audit;
using PlataformaECommerce.Domain.Entities.Cart;
using PlataformaECommerce.Domain.Entities.Orders;
using PlataformaECommerce.Domain.Entities.Products;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Infrastructure.Services.Products;

namespace PlataformaECommerce.Tests.Application.Orders;

[TestFixture]
public class PaymentServiceTests
{
    [Test]
    public async Task RegisterOrderPaymentAsync_PagoValido_DescuentaStockYMarcaPedidoComoPagado()
    {
        (Pedido order, Producto product) = CreateConfirmedOrderWithProduct();
        FakeUnitOfWork unitOfWork = new();
        PaymentService service = new(
            new FakeOrderRepository(order),
            new FakeProductRepository(product),
            unitOfWork,
            new FakeAuditTrailService());

        Result<OrderDetailDto> result = await service.RegisterOrderPaymentAsync(new RegisterOrderPaymentCommand
        {
            OrderId = order.Id,
            PaymentReference = "PAY-TEST-001",
            PaymentMethod = "Tarjeta",
            Amount = order.Total.Amount,
            Currency = order.Total.Currency,
            PaidAtUtc = DateTime.UtcNow,
            PaymentProvider = "Wompi"
        });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Status, Is.EqualTo(EstadoPedido.Pagado));
        Assert.That(product.Stock, Is.EqualTo(8));
        Assert.That(unitOfWork.BeginTransactionCalls, Is.EqualTo(1));
        Assert.That(unitOfWork.CommitTransactionCalls, Is.EqualTo(1));
    }

    [Test]
    public async Task RegisterOrderPaymentAsync_StockInsuficiente_RetornaFalloYSinPersistirPago()
    {
        (Pedido order, Producto product) = CreateConfirmedOrderWithProduct(initialStock: 3, orderedQuantity: 2);
        product.DisminuirStock(2);
        FakeUnitOfWork unitOfWork = new();
        PaymentService service = new(
            new FakeOrderRepository(order),
            new FakeProductRepository(product),
            unitOfWork,
            new FakeAuditTrailService());

        Result<OrderDetailDto> result = await service.RegisterOrderPaymentAsync(new RegisterOrderPaymentCommand
        {
            OrderId = order.Id,
            PaymentReference = "PAY-TEST-002",
            PaymentMethod = "Tarjeta",
            Amount = order.Total.Amount,
            Currency = order.Total.Currency,
            PaidAtUtc = DateTime.UtcNow,
            PaymentProvider = "Wompi"
        });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(order.Estado, Is.EqualTo(EstadoPedido.Confirmado));
        Assert.That(product.Stock, Is.EqualTo(1));
        Assert.That(unitOfWork.RollbackTransactionCalls, Is.EqualTo(1));
    }

    private static (Pedido Order, Producto Product) CreateConfirmedOrderWithProduct(int initialStock = 10, int orderedQuantity = 2)
    {
        Guid customerId = Guid.NewGuid();
        CarritoCompra cart = new(customerId);
        var product = FabricaEntidades.CrearProductoFisico(
            "Portátil Pro",
            "Portátil profesional para trabajo intensivo.",
            4500000m,
            initialStock,
            1.8m,
            2m,
            35m,
            24m,
            sku: "PORTATIL-PRO-001");

        product.Activar();
        cart.AgregarProducto(product, orderedQuantity);

        Pedido order = new(cart);
        order.Confirmar();
        return (order, product);
    }

    private sealed class FakeOrderRepository(Pedido order) : IOrderRepository
    {
        public Task<IReadOnlyCollection<Pedido>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Pedido>>(new[] { order });
        public Task<Pedido?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(id == order.Id ? order : null);
        public Task<IReadOnlyCollection<Pedido>> GetByCustomerIdAsync(Guid clienteId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Pedido>>(new[] { order });
        public Task<IReadOnlyCollection<Pedido>> GetByStatusAsync(EstadoPedido estado, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Pedido>>(new[] { order });
        public Task<IReadOnlyCollection<Pedido>> GetByCustomerIdAndStatusAsync(Guid clienteId, EstadoPedido estado, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Pedido>>(new[] { order });
        public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(id == order.Id);
        public Task<bool> ExistsByCustomerIdAsync(Guid clienteId, CancellationToken cancellationToken = default) => Task.FromResult(clienteId == order.ClienteId);
        public Task AddAsync(Pedido pedido, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateAsync(Pedido pedido, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeProductRepository(Producto product) : IProductRepository
    {
        public Task<IReadOnlyCollection<Producto>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Producto>>(new[] { product });
        public Task<Producto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(id == product.Id ? product : null);
        public Task<Producto?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default) => Task.FromResult(string.Equals(product.Sku.Value, sku, StringComparison.OrdinalIgnoreCase) ? product : null);
        public Task<IReadOnlyCollection<Producto>> GetActiveProductsAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Producto>>(new[] { product });
        public Task<IReadOnlyCollection<Producto>> GetFeaturedProductsAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Producto>>(Array.Empty<Producto>());
        public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(id == product.Id);
        public Task<bool> ExistsBySkuAsync(string sku, CancellationToken cancellationToken = default) => Task.FromResult(string.Equals(product.Sku.Value, sku, StringComparison.OrdinalIgnoreCase));
        public Task AddAsync(Producto producto, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateAsync(Producto producto, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int BeginTransactionCalls { get; private set; }
        public int CommitTransactionCalls { get; private set; }
        public int RollbackTransactionCalls { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
        public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            BeginTransactionCalls++;
            return Task.CompletedTask;
        }

        public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            CommitTransactionCalls++;
            return Task.CompletedTask;
        }

        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            RollbackTransactionCalls++;
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed class FakeAuditTrailService : IAuditTrailService
    {
        public Task RegisterAsync(Guid aggregateId, string aggregateType, string module, string action, string detail, IReadOnlyDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
