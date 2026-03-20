using PlataformaECommerce.Application.Features.Orders.Commands;
using PlataformaECommerce.Application.Features.Orders.Services;
using PlataformaECommerce.Application.Features.Orders.Validators;
using PlataformaECommerce.Application.Interfaces.Persistence;
using PlataformaECommerce.Application.Interfaces.Repositories.Cart;
using PlataformaECommerce.Application.Interfaces.Repositories.Orders;
using PlataformaECommerce.Application.Interfaces.Repositories.Users;
using PlataformaECommerce.Application.Interfaces.Services.Audit;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Domain.Entities.Cart;
using PlataformaECommerce.Domain.Entities.Orders;
using PlataformaECommerce.Domain.Entities.Products;
using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Tests.Application.Orders;

[TestFixture]
public class OrderApplicationServiceTests
{
    [Test]
    public async Task CreateOrderFromCartAsync_OperacionExitosa_RegistraEventoDeAuditoria()
    {
        Cliente customer = new("Cliente Demo", new Email("cliente@plataforma.com"), "hash-cliente-seguro-2026");
        CarritoCompra cart = CreateCart(customer.Id);
        FakeAuditTrailService auditTrailService = new();
        OrderApplicationService service = new(
            new FakeOrderRepository(),
            new FakeCartRepository(cart),
            new FakeUserRepository(customer),
            new FakeUnitOfWork(),
            auditTrailService,
            new CreateOrderFromCartCommandValidator(),
            new CancelOrderCommandValidator());

        await service.CreateOrderFromCartAsync(new CreateOrderFromCartCommand
        {
            CartId = cart.Id,
            CustomerId = customer.Id,
            RequestedAtUtc = DateTime.UtcNow
        });

        Assert.That(auditTrailService.RegisteredEvents.Count, Is.EqualTo(1));
    }

    private static CarritoCompra CreateCart(Guid customerId)
    {
        CarritoCompra cart = new(customerId);
        ProductoFisico product = new(
            "Teclado Pro",
            "Teclado mecánico para pruebas.",
            new Sku("TECLADO-001"),
            new Money(250m, "COP"),
            10,
            "teclado-pro",
            null,
            null,
            null,
            null,
            0.8m,
            4m,
            15m,
            45m,
            true);

        product.Activar();
        cart.AgregarProducto(product, 2);
        return cart;
    }

    private sealed class FakeOrderRepository : IOrderRepository
    {
        private readonly List<Pedido> _orders = new();

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
            _orders.Add(pedido);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Pedido pedido, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeCartRepository : ICartRepository
    {
        private readonly CarritoCompra _cart;

        public FakeCartRepository(CarritoCompra cart)
        {
            _cart = cart;
        }

        public Task<IReadOnlyCollection<CarritoCompra>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<CarritoCompra>>(new[] { _cart });

        public Task<CarritoCompra?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(id == _cart.Id ? _cart : null);

        public Task<CarritoCompra?> GetByCustomerIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
            => Task.FromResult(clienteId == _cart.ClienteId ? _cart : null);

        public Task<IReadOnlyCollection<CarritoCompra>> GetAllByCustomerIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<CarritoCompra>>(clienteId == _cart.ClienteId ? new[] { _cart } : Array.Empty<CarritoCompra>());

        public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(id == _cart.Id);
        public Task<bool> ExistsByCustomerIdAsync(Guid clienteId, CancellationToken cancellationToken = default) => Task.FromResult(clienteId == _cart.ClienteId);
        public Task AddAsync(CarritoCompra carrito, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateAsync(CarritoCompra carrito, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly Cliente _customer;

        public FakeUserRepository(Cliente customer)
        {
            _customer = customer;
        }

        public Task<IReadOnlyCollection<Usuario>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Usuario>>(new Usuario[] { _customer });

        public Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<Usuario?>(id == _customer.Id ? _customer : null);

        public Task<Usuario?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
            => Task.FromResult<Usuario?>(email == _customer.CorreoElectronico ? _customer : null);

        public Task<IReadOnlyCollection<Usuario>> GetByRoleAsync(RolUsuario rol, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Usuario>>(rol == RolUsuario.Cliente ? new Usuario[] { _customer } : Array.Empty<Usuario>());

        public Task<IReadOnlyCollection<Cliente>> GetCustomersAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Cliente>>(new[] { _customer });

        public Task<IReadOnlyCollection<Administrador>> GetAdministratorsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Administrador>>(Array.Empty<Administrador>());

        public Task<Cliente?> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(id == _customer.Id ? _customer : null);

        public Task<Administrador?> GetAdministratorByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<Administrador?>(null);

        public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(id == _customer.Id);
        public Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default) => Task.FromResult(email == _customer.CorreoElectronico);
        public Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateAsync(Usuario usuario, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeAuditTrailService : IAuditTrailService
    {
        public List<string> RegisteredEvents { get; } = new();

        public Task RegisterAsync(Guid aggregateId, string aggregateType, string module, string action, string detail, IReadOnlyDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default)
        {
            RegisteredEvents.Add(action);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
        public Task BeginTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task CommitTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
