using PlataformaECommerce.Application.Features.Cart.Commands;
using PlataformaECommerce.Application.Features.Cart.Services;
using PlataformaECommerce.Application.Features.Cart.Validators;
using PlataformaECommerce.Application.Interfaces.Persistence;
using PlataformaECommerce.Application.Interfaces.Repositories.Cart;
using PlataformaECommerce.Application.Interfaces.Repositories.Products;
using PlataformaECommerce.Application.Interfaces.Repositories.Users;
using PlataformaECommerce.Application.Interfaces.Services.Audit;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Domain.Entities.Cart;
using PlataformaECommerce.Domain.Entities.Products;
using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Tests.Application.Cart;

[TestFixture]
public class CartApplicationServiceTests
{
    [Test]
    public async Task CreateCartAsync_OperacionExitosa_RegistraEventoDeAuditoria()
    {
        Cliente customer = new("Cliente Demo", new Email("cliente@plataforma.com"), "hash-cliente-seguro-2026");
        FakeAuditTrailService auditTrailService = new();
        CartApplicationService service = new(
            new FakeCartRepository(),
            new FakeProductRepository(),
            new FakeUserRepository(customer),
            new FakeUnitOfWork(),
            auditTrailService,
            new AddProductToCartCommandValidator(),
            new UpdateCartItemQuantityCommandValidator());

        await service.CreateCartAsync(new CreateCartCommand
        {
            CustomerId = customer.Id,
            IsActive = true
        });

        Assert.That(auditTrailService.RegisteredEvents.Count, Is.EqualTo(1));
    }

    private sealed class FakeCartRepository : ICartRepository
    {
        private readonly List<CarritoCompra> _carts = new();

        public Task<IReadOnlyCollection<CarritoCompra>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<CarritoCompra>>(_carts.ToArray());

        public Task<CarritoCompra?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_carts.FirstOrDefault(cart => cart.Id == id));

        public Task<CarritoCompra?> GetByCustomerIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
            => Task.FromResult(_carts.FirstOrDefault(cart => cart.ClienteId == clienteId && cart.Activo));

        public Task<IReadOnlyCollection<CarritoCompra>> GetAllByCustomerIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<CarritoCompra>>(_carts.Where(cart => cart.ClienteId == clienteId).ToArray());

        public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_carts.Any(cart => cart.Id == id));

        public Task<bool> ExistsByCustomerIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
            => Task.FromResult(_carts.Any(cart => cart.ClienteId == clienteId));

        public Task AddAsync(CarritoCompra carrito, CancellationToken cancellationToken = default)
        {
            _carts.Add(carrito);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(CarritoCompra carrito, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _carts.RemoveAll(cart => cart.Id == id);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeProductRepository : IProductRepository
    {
        public Task<IReadOnlyCollection<Producto>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Producto>>(Array.Empty<Producto>());

        public Task<Producto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<Producto?>(null);

        public Task<Producto?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
            => Task.FromResult<Producto?>(null);

        public Task<IReadOnlyCollection<Producto>> GetActiveProductsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Producto>>(Array.Empty<Producto>());

        public Task<IReadOnlyCollection<Producto>> GetFeaturedProductsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Producto>>(Array.Empty<Producto>());

        public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> ExistsBySkuAsync(string sku, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task AddAsync(Producto producto, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateAsync(Producto producto, CancellationToken cancellationToken = default) => Task.CompletedTask;
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

        public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(id == _customer.Id);

        public Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
            => Task.FromResult(email == _customer.CorreoElectronico);

        public Task<bool> ExistsByRoleAsync(RolUsuario rol, CancellationToken cancellationToken = default)
            => Task.FromResult(rol == RolUsuario.Cliente);

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
