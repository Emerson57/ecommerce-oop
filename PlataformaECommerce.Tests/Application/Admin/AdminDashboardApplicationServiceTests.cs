using PlataformaECommerce.Application.Features.Admin.DTOs;
using PlataformaECommerce.Application.Features.Admin.Queries;
using PlataformaECommerce.Application.Features.Admin.Services;
using PlataformaECommerce.Application.Features.Admin.Validators;
using PlataformaECommerce.Application.Interfaces.Persistence;
using PlataformaECommerce.Application.Interfaces.Repositories.Audit;
using PlataformaECommerce.Application.Interfaces.Repositories.Cart;
using PlataformaECommerce.Application.Interfaces.Repositories.Orders;
using PlataformaECommerce.Application.Interfaces.Repositories.Products;
using PlataformaECommerce.Application.Interfaces.Repositories.Users;
using PlataformaECommerce.Application.Interfaces.Services.Audit;
using PlataformaECommerce.Application.Interfaces.Services.Auth;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Domain.Entities.Cart;
using PlataformaECommerce.Domain.Entities.Orders;
using PlataformaECommerce.Domain.Entities.Products;
using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Tests.Application.Admin;

[TestFixture]
public class AdminApplicationServiceDashboardTests
{
    [Test]
    public async Task GetDashboardAsync_DatosDisponibles_RetornaMetricasReales()
    {
        ProductoFisico activeProduct = new(
            "Producto Activo",
            "Producto activo para pruebas.",
            new Sku("PROD-001"),
            new Money(100m, "COP"),
            10,
            "producto-activo",
            null,
            null,
            null,
            null,
            1m,
            1m,
            1m,
            1m,
            false);
        activeProduct.Activar();
        activeProduct.MarcarComoDestacado();

        ProductoFisico lowStockProduct = new(
            "Producto Bajo Stock",
            "Producto con bajo stock.",
            new Sku("PROD-002"),
            new Money(120m, "COP"),
            3,
            "producto-bajo-stock",
            null,
            null,
            null,
            null,
            1m,
            1m,
            1m,
            1m,
            false);
        lowStockProduct.Activar();

        Cliente customer = new("Cliente Demo", new Email("cliente@plataforma.com"), "hash-cliente-seguro-2026");
        customer.ConfirmarCorreoElectronico();
        customer.RegistrarAcceso();

        Administrador administrator = new("Admin Demo", new Email("admin@plataforma.com"), "hash-admin-seguro-2026", "Operaciones");
        administrator.ConfirmarCorreoElectronico();

        Pedido order = new(customer.Id);
        CarritoCompra cart = new(customer.Id);

        AdminApplicationService service = new(
            new FakeProductRepository(activeProduct, lowStockProduct),
            new FakeOrderRepository(order),
            new FakeUserRepository(customer, administrator),
            new FakeCartRepository(cart),
            new FakeAuditRepository(),
            new FakeUnitOfWork(),
            new FakePasswordHasher(),
            new FakeAuditTrailService(),
            new FakeCurrentUserService(),
            new RegisterAdminCommandValidator());

        var result = await service.GetDashboardAsync(new GetAdminDashboardQuery());

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.TotalProducts, Is.EqualTo(2));
        Assert.That(result.Value.AuditEventsLast24Hours, Is.EqualTo(3));
        Assert.That(result.Value.RecentActivities.Count, Is.EqualTo(2));
    }

    private sealed class FakeProductRepository : IProductRepository
    {
        private readonly IReadOnlyCollection<Producto> _products;

        public FakeProductRepository(params Producto[] products)
        {
            _products = products;
        }

        public Task<IReadOnlyCollection<Producto>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(_products);
        public Task<Producto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_products.FirstOrDefault(product => product.Id == id));
        public Task<Producto?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default) => Task.FromResult(_products.FirstOrDefault(product => product.Sku.Value == sku));
        public Task<IReadOnlyCollection<Producto>> GetActiveProductsAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Producto>>(_products.Where(product => product.Activo).ToArray());
        public Task<IReadOnlyCollection<Producto>> GetFeaturedProductsAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Producto>>(_products.Where(product => product.Destacado).ToArray());
        public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_products.Any(product => product.Id == id));
        public Task<bool> ExistsBySkuAsync(string sku, CancellationToken cancellationToken = default) => Task.FromResult(_products.Any(product => product.Sku.Value == sku));
        public Task AddAsync(Producto producto, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateAsync(Producto producto, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeOrderRepository : IOrderRepository
    {
        private readonly IReadOnlyCollection<Pedido> _orders;

        public FakeOrderRepository(params Pedido[] orders)
        {
            _orders = orders;
        }

        public Task<IReadOnlyCollection<Pedido>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(_orders);
        public Task<Pedido?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_orders.FirstOrDefault(order => order.Id == id));
        public Task<IReadOnlyCollection<Pedido>> GetByCustomerIdAsync(Guid clienteId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Pedido>>(_orders.Where(order => order.ClienteId == clienteId).ToArray());
        public Task<IReadOnlyCollection<Pedido>> GetByStatusAsync(EstadoPedido estado, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Pedido>>(_orders.Where(order => order.Estado == estado).ToArray());
        public Task<IReadOnlyCollection<Pedido>> GetByCustomerIdAndStatusAsync(Guid clienteId, EstadoPedido estado, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Pedido>>(_orders.Where(order => order.ClienteId == clienteId && order.Estado == estado).ToArray());
        public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_orders.Any(order => order.Id == id));
        public Task<bool> ExistsByCustomerIdAsync(Guid clienteId, CancellationToken cancellationToken = default) => Task.FromResult(_orders.Any(order => order.ClienteId == clienteId));
        public Task AddAsync(Pedido pedido, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateAsync(Pedido pedido, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly IReadOnlyCollection<Usuario> _users;

        public FakeUserRepository(params Usuario[] users)
        {
            _users = users;
        }

        public Task<IReadOnlyCollection<Usuario>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(_users);
        public Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_users.FirstOrDefault(user => user.Id == id));
        public Task<Usuario?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default) => Task.FromResult(_users.FirstOrDefault(user => user.CorreoElectronico.Equals(email)));
        public Task<IReadOnlyCollection<Usuario>> GetByRoleAsync(RolUsuario rol, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Usuario>>(_users.Where(user => user.Rol == rol).ToArray());
        public Task<IReadOnlyCollection<Cliente>> GetCustomersAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Cliente>>(_users.OfType<Cliente>().ToArray());
        public Task<IReadOnlyCollection<Administrador>> GetAdministratorsAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Administrador>>(_users.OfType<Administrador>().ToArray());
        public Task<Cliente?> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_users.OfType<Cliente>().FirstOrDefault(user => user.Id == id));
        public Task<Administrador?> GetAdministratorByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_users.OfType<Administrador>().FirstOrDefault(user => user.Id == id));
        public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_users.Any(user => user.Id == id));
        public Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default) => Task.FromResult(_users.Any(user => user.CorreoElectronico.Equals(email)));
        public Task<bool> ExistsByRoleAsync(RolUsuario rol, CancellationToken cancellationToken = default) => Task.FromResult(_users.Any(user => user.Rol == rol));
        public Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateAsync(Usuario usuario, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeCartRepository : ICartRepository
    {
        private readonly IReadOnlyCollection<CarritoCompra> _carts;

        public FakeCartRepository(params CarritoCompra[] carts)
        {
            _carts = carts;
        }

        public Task<IReadOnlyCollection<CarritoCompra>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(_carts);
        public Task<CarritoCompra?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_carts.FirstOrDefault(cart => cart.Id == id));
        public Task<CarritoCompra?> GetByCustomerIdAsync(Guid clienteId, CancellationToken cancellationToken = default) => Task.FromResult(_carts.FirstOrDefault(cart => cart.ClienteId == clienteId));
        public Task<IReadOnlyCollection<CarritoCompra>> GetAllByCustomerIdAsync(Guid clienteId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<CarritoCompra>>(_carts.Where(cart => cart.ClienteId == clienteId).ToArray());
        public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_carts.Any(cart => cart.Id == id));
        public Task<bool> ExistsByCustomerIdAsync(Guid clienteId, CancellationToken cancellationToken = default) => Task.FromResult(_carts.Any(cart => cart.ClienteId == clienteId));
        public Task AddAsync(CarritoCompra carrito, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateAsync(CarritoCompra carrito, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeAuditRepository : IAuditRepository
    {
        public Task RegisterEventAsync(AuditEntry entry, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IReadOnlyCollection<AuditEntry>> GetHistoryAsync(Guid aggregateId, string aggregateType, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<AuditEntry>>(Array.Empty<AuditEntry>());

        public Task<AuditSearchResult> SearchAsync(AuditSearchFilter filter, CancellationToken cancellationToken = default)
        {
            if (filter.PageSize == 1 && filter.FromUtc.HasValue)
            {
                return Task.FromResult(new AuditSearchResult
                {
                    Items = Array.Empty<AuditEntry>(),
                    TotalCount = 3,
                    PageNumber = 1,
                    PageSize = 1
                });
            }

            return Task.FromResult(new AuditSearchResult
            {
                Items =
                [
                    new AuditEntry
                    {
                        AggregateId = Guid.NewGuid(),
                        AggregateType = "Product",
                        Module = "Products",
                        Action = "product.updated",
                        Detail = "Producto actualizado.",
                        PerformedBy = "admin@plataforma.com",
                        OccurredAtUtc = DateTime.UtcNow.AddMinutes(-15)
                    },
                    new AuditEntry
                    {
                        AggregateId = Guid.NewGuid(),
                        AggregateType = "Order",
                        Module = "Orders",
                        Action = "order.created",
                        Detail = "Pedido creado.",
                        PerformedBy = "admin@plataforma.com",
                        OccurredAtUtc = DateTime.UtcNow.AddMinutes(-5)
                    }
                ],
                TotalCount = 2,
                PageNumber = 1,
                PageSize = 5
            });
        }
    }

    private sealed class FakeCurrentUserService : ICurrentUserService
    {
        public Guid? UserId => Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        public string? UserName => "Admin Demo";
        public string? Email => "admin@plataforma.com";
        public string? Role => "Administrador";
        public bool IsAuthenticated => true;
        public bool IsInRole(string role) => string.Equals(role, Role, StringComparison.OrdinalIgnoreCase);
        public string? GetClaimValue(string claimType) => null;
        public IReadOnlyCollection<string> GetClaimValues(string claimType) => Array.Empty<string>();
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password) => $"hash-{password}-seguro-2026";
        public bool VerifyPassword(string password, string passwordHash) => passwordHash == HashPassword(password);
    }

    private sealed class FakeAuditTrailService : IAuditTrailService
    {
        public Task RegisterAsync(Guid aggregateId, string aggregateType, string module, string action, string detail, IReadOnlyDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default)
        {
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
