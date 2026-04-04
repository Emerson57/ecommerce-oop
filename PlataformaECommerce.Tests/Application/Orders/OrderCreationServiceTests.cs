using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Orders.Commands;
using PlataformaECommerce.Application.Features.Orders.DTOs;
using PlataformaECommerce.Application.Features.Orders.Services;
using PlataformaECommerce.Application.Features.Orders.Validators;
using PlataformaECommerce.Application.Interfaces.Persistence;
using PlataformaECommerce.Application.Interfaces.Repositories.Cart;
using PlataformaECommerce.Application.Interfaces.Repositories.Orders;
using PlataformaECommerce.Application.Interfaces.Repositories.Users;
using PlataformaECommerce.Application.Interfaces.Services.Audit;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Application.Common.Notifications;
using PlataformaECommerce.Domain.Entities.Cart;
using PlataformaECommerce.Domain.Entities.Orders;
using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.ValueObjects;
using PlataformaECommerce.Infrastructure.Services.Products;

namespace PlataformaECommerce.Tests.Application.Orders;

[TestFixture]
public class OrderCreationServiceTests
{
    [Test]
    public async Task CreateOrderFromCartAsync_ConCheckoutValido_ConfirmaPedidoYUsaTransaccion()
    {
        Cliente customer = new("Cliente Checkout", new Email("checkout@plataforma.com"), "hash-seguro-2026-validado-largo");
        CarritoCompra cart = CreatePhysicalCart(customer.Id);
        FakeUnitOfWork unitOfWork = new();
        FakeOrderRepository orderRepository = new();
        FakeEmailNotificationService emailNotificationService = new();
        OrderCreationService service = new(
            orderRepository,
            new FakeCartRepository(cart),
            new FakeUserRepository(customer),
            unitOfWork,
            new FakeAuditTrailService(),
            emailNotificationService,
            new CreateOrderFromCartCommandValidator());

        Result<OrderDetailDto> result = await service.CreateOrderFromCartAsync(new CreateOrderFromCartCommand
        {
            CartId = cart.Id,
            CustomerId = customer.Id,
            PaymentMethod = MetodoPagoPedido.Tarjeta,
            ShippingStreet = "Calle 123 #45-67",
            ShippingCity = "Bogotá",
            ShippingRegion = "Cundinamarca",
            ShippingCountry = "Colombia",
            ShippingPostalCode = "110111",
            RequestedByUserId = customer.Id,
            Source = "Tests.Orders.Checkout",
            RequestedAtUtc = DateTime.UtcNow
        });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Status, Is.EqualTo(EstadoPedido.Confirmado));
        Assert.That(result.Value.PaymentMethod, Is.EqualTo(MetodoPagoPedido.Tarjeta));
        Assert.That(result.Value.ShippingCity, Is.EqualTo("Bogotá"));
        Assert.That(emailNotificationService.LastOrderConfirmationNotification?.ToEmail, Is.EqualTo(customer.CorreoElectronico.Value));
        Assert.That(unitOfWork.BeginTransactionCalls, Is.EqualTo(1));
        Assert.That(unitOfWork.CommitTransactionCalls, Is.EqualTo(1));
        Assert.That(unitOfWork.RollbackTransactionCalls, Is.EqualTo(0));
        Assert.That(unitOfWork.SaveChangesCalls, Is.EqualTo(1));
    }

    [Test]
    public async Task CreateOrderFromCartAsync_ErrorDePersistencia_RevierteTransaccion()
    {
        Cliente customer = new("Cliente Checkout", new Email("checkout@plataforma.com"), "hash-seguro-2026-validado-largo");
        CarritoCompra cart = CreatePhysicalCart(customer.Id);
        FakeUnitOfWork unitOfWork = new();
        FakeOrderRepository orderRepository = new() { ThrowOnAdd = true };
        OrderCreationService service = new(
            orderRepository,
            new FakeCartRepository(cart),
            new FakeUserRepository(customer),
            unitOfWork,
            new FakeAuditTrailService(),
            new FakeEmailNotificationService(),
            new CreateOrderFromCartCommandValidator());

        Assert.ThrowsAsync<InvalidOperationException>(async () => await service.CreateOrderFromCartAsync(new CreateOrderFromCartCommand
        {
            CartId = cart.Id,
            CustomerId = customer.Id,
            PaymentMethod = MetodoPagoPedido.Tarjeta,
            ShippingStreet = "Calle 123 #45-67",
            ShippingCity = "Bogotá",
            ShippingRegion = "Cundinamarca",
            ShippingCountry = "Colombia",
            ShippingPostalCode = "110111",
            RequestedByUserId = customer.Id,
            Source = "Tests.Orders.Checkout",
            RequestedAtUtc = DateTime.UtcNow
        }));

        Assert.That(unitOfWork.BeginTransactionCalls, Is.EqualTo(1));
        Assert.That(unitOfWork.CommitTransactionCalls, Is.EqualTo(0));
        Assert.That(unitOfWork.RollbackTransactionCalls, Is.EqualTo(1));
    }

    private sealed class FakeEmailNotificationService : IEmailNotificationService
    {
        public OrderConfirmationEmailNotification? LastOrderConfirmationNotification { get; private set; }

        public Task<Result> SendAccountEmailConfirmationAsync(AccountEmailConfirmationNotification notification, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());

        public Task<Result> SendPasswordResetEmailAsync(PasswordResetEmailNotification notification, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());

        public Task<Result> SendOrderConfirmationEmailAsync(OrderConfirmationEmailNotification notification, CancellationToken cancellationToken = default)
        {
            LastOrderConfirmationNotification = notification;
            return Task.FromResult(Result.Success());
        }
    }

    private static CarritoCompra CreatePhysicalCart(Guid customerId)
    {
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
        return cart;
    }

    private sealed class FakeOrderRepository : IOrderRepository
    {
        private readonly List<Pedido> _orders = [];

        public bool ThrowOnAdd { get; init; }

        public Task<IReadOnlyCollection<Pedido>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Pedido>>(_orders.ToArray());

        public Task<Pedido?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_orders.FirstOrDefault(order => order.Id == id));

        public Task<IReadOnlyCollection<Pedido>> GetByCustomerIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Pedido>>(_orders.Where(order => order.ClienteId == clienteId).ToArray());

        public Task<IReadOnlyCollection<Pedido>> GetByStatusAsync(EstadoPedido estado, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Pedido>>(_orders.Where(order => order.Estado == estado).ToArray());

        public Task<IReadOnlyCollection<Pedido>> GetByCustomerIdAndStatusAsync(Guid clienteId, EstadoPedido estado, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Pedido>>(_orders.Where(order => order.ClienteId == clienteId && order.Estado == estado).ToArray());

        public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_orders.Any(order => order.Id == id));

        public Task<bool> ExistsByCustomerIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
            => Task.FromResult(_orders.Any(order => order.ClienteId == clienteId));

        public Task AddAsync(Pedido pedido, CancellationToken cancellationToken = default)
        {
            if (ThrowOnAdd)
            {
                throw new InvalidOperationException("Persistencia simulada no disponible.");
            }

            _orders.Add(pedido);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Pedido pedido, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _orders.RemoveAll(order => order.Id == id);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeCartRepository(CarritoCompra cart) : ICartRepository
    {
        public Task<IReadOnlyCollection<CarritoCompra>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<CarritoCompra>>(new[] { cart });

        public Task<CarritoCompra?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(id == cart.Id ? cart : null);

        public Task<CarritoCompra?> GetByCustomerIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
            => Task.FromResult(clienteId == cart.ClienteId ? cart : null);

        public Task<IReadOnlyCollection<CarritoCompra>> GetAllByCustomerIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<CarritoCompra>>(clienteId == cart.ClienteId ? new[] { cart } : Array.Empty<CarritoCompra>());

        public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(id == cart.Id);

        public Task<bool> ExistsByCustomerIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
            => Task.FromResult(clienteId == cart.ClienteId);

        public Task AddAsync(CarritoCompra carrito, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateAsync(CarritoCompra carrito, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeUserRepository(Cliente customer) : IUserRepository
    {
        public Task<IReadOnlyCollection<Usuario>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Usuario>>(new Usuario[] { customer });

        public Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<Usuario?>(id == customer.Id ? customer : null);

        public Task<Usuario?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
            => Task.FromResult<Usuario?>(customer.CorreoElectronico.Equals(email) ? customer : null);

        public Task<IReadOnlyCollection<Usuario>> GetByRoleAsync(RolUsuario rol, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Usuario>>(rol == RolUsuario.Cliente ? new Usuario[] { customer } : Array.Empty<Usuario>());

        public Task<IReadOnlyCollection<Cliente>> GetCustomersAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Cliente>>(new[] { customer });

        public Task<IReadOnlyCollection<Administrador>> GetAdministratorsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Administrador>>(Array.Empty<Administrador>());

        public Task<Cliente?> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(id == customer.Id ? customer : null);

        public Task<Administrador?> GetAdministratorByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<Administrador?>(null);

        public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(id == customer.Id);

        public Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
            => Task.FromResult(customer.CorreoElectronico.Equals(email));

        public Task<bool> ExistsByRoleAsync(RolUsuario rol, CancellationToken cancellationToken = default)
            => Task.FromResult(rol == RolUsuario.Cliente);

        public Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateAsync(Usuario usuario, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int BeginTransactionCalls { get; private set; }
        public int CommitTransactionCalls { get; private set; }
        public int RollbackTransactionCalls { get; private set; }
        public int SaveChangesCalls { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCalls++;
            return Task.FromResult(1);
        }

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
