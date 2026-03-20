using PlataformaECommerce.Application.Features.Admin.Commands;
using PlataformaECommerce.Application.Common.Results;
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
public class AdminApplicationServiceTests
{
    [Test]
    public async Task RegisterAdminAsync_OperacionExitosa_RegistraEventoDeAuditoria()
    {
        FakeUserRepository userRepository = new();
        FakeAuditTrailService auditTrailService = new();
        AdminApplicationService service = new(
            new FakeProductRepository(),
            new FakeOrderRepository(),
            userRepository,
            new FakeCartRepository(),
            new FakeAuditRepository(),
            new FakeUnitOfWork(),
            new FakePasswordHasher(),
            auditTrailService,
            new FakeCurrentUserService(),
            new RegisterAdminCommandValidator());

        await service.RegisterAdminAsync(new RegisterAdminCommand
        {
            Name = "Admin Demo",
            Email = "admin@plataforma.com",
            Password = "Password#2026",
            ConfirmPassword = "Password#2026",
            Area = "Operaciones",
            IsActive = true,
            IsEmailConfirmed = true
        });

        Assert.That(auditTrailService.RegisteredEvents.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task GetDashboardAsync_ConsultaValida_RetornaResumenAdministrativo()
    {
        FakeUserRepository userRepository = new();
        AdminApplicationService service = new(
            new FakeProductRepository(),
            new FakeOrderRepository(),
            userRepository,
            new FakeCartRepository(),
            new FakeAuditRepository(),
            new FakeUnitOfWork(),
            new FakePasswordHasher(),
            new FakeAuditTrailService(),
            new FakeCurrentUserService(),
            new RegisterAdminCommandValidator());

        var result = await service.GetDashboardAsync(new GetAdminDashboardQuery(), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.WindowInDays, Is.EqualTo(30));
    }

    private sealed class FakeProductRepository : IProductRepository
    {
        public Task<IReadOnlyCollection<Producto>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Producto>>(Array.Empty<Producto>());
        public Task<Producto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Producto?>(null);
        public Task<Producto?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default) => Task.FromResult<Producto?>(null);
        public Task<IReadOnlyCollection<Producto>> GetActiveProductsAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Producto>>(Array.Empty<Producto>());
        public Task<IReadOnlyCollection<Producto>> GetFeaturedProductsAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Producto>>(Array.Empty<Producto>());
        public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> ExistsBySkuAsync(string sku, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task AddAsync(Producto producto, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateAsync(Producto producto, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeOrderRepository : IOrderRepository
    {
        public Task<IReadOnlyCollection<Pedido>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Pedido>>(Array.Empty<Pedido>());
        public Task<Pedido?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Pedido?>(null);
        public Task<IReadOnlyCollection<Pedido>> GetByCustomerIdAsync(Guid clienteId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Pedido>>(Array.Empty<Pedido>());
        public Task<IReadOnlyCollection<Pedido>> GetByStatusAsync(EstadoPedido estado, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Pedido>>(Array.Empty<Pedido>());
        public Task<IReadOnlyCollection<Pedido>> GetByCustomerIdAndStatusAsync(Guid clienteId, EstadoPedido estado, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Pedido>>(Array.Empty<Pedido>());
        public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> ExistsByCustomerIdAsync(Guid clienteId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task AddAsync(Pedido pedido, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateAsync(Pedido pedido, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeCartRepository : ICartRepository
    {
        public Task<IReadOnlyCollection<CarritoCompra>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<CarritoCompra>>(Array.Empty<CarritoCompra>());
        public Task<CarritoCompra?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<CarritoCompra?>(null);
        public Task<CarritoCompra?> GetByCustomerIdAsync(Guid clienteId, CancellationToken cancellationToken = default) => Task.FromResult<CarritoCompra?>(null);
        public Task<IReadOnlyCollection<CarritoCompra>> GetAllByCustomerIdAsync(Guid clienteId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<CarritoCompra>>(Array.Empty<CarritoCompra>());
        public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> ExistsByCustomerIdAsync(Guid clienteId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task AddAsync(CarritoCompra carrito, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateAsync(CarritoCompra carrito, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeAuditRepository : IAuditRepository
    {
        public Task RegisterEventAsync(AuditEntry entry, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IReadOnlyCollection<AuditEntry>> GetHistoryAsync(Guid aggregateId, string aggregateType, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<AuditEntry>>(Array.Empty<AuditEntry>());
        public Task<AuditSearchResult> SearchAsync(AuditSearchFilter filter, CancellationToken cancellationToken = default) => Task.FromResult(new AuditSearchResult { Items = Array.Empty<AuditEntry>(), TotalCount = 0, PageNumber = 1, PageSize = filter.PageSize <= 0 ? 25 : filter.PageSize });
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly List<Usuario> _users = new();

        public Task<IReadOnlyCollection<Usuario>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Usuario>>(_users.ToArray());

        public Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_users.FirstOrDefault(user => user.Id == id));

        public Task<Usuario?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
            => Task.FromResult(_users.FirstOrDefault(user => user.CorreoElectronico == email));

        public Task<IReadOnlyCollection<Usuario>> GetByRoleAsync(RolUsuario rol, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Usuario>>(_users.Where(user => user.Rol == rol).ToArray());

        public Task<IReadOnlyCollection<Cliente>> GetCustomersAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Cliente>>(_users.OfType<Cliente>().ToArray());

        public Task<IReadOnlyCollection<Administrador>> GetAdministratorsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Administrador>>(_users.OfType<Administrador>().ToArray());

        public Task<Cliente?> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_users.OfType<Cliente>().FirstOrDefault(user => user.Id == id));

        public Task<Administrador?> GetAdministratorByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_users.OfType<Administrador>().FirstOrDefault(user => user.Id == id));

        public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_users.Any(user => user.Id == id));

        public Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
            => Task.FromResult(_users.Any(user => user.CorreoElectronico == email));

        public Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default)
        {
            _users.Add(usuario);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Usuario usuario, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _users.RemoveAll(user => user.Id == id);
            return Task.CompletedTask;
        }
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password) => $"hash-{password}-seguro-2026";
        public bool VerifyPassword(string password, string passwordHash) => passwordHash == HashPassword(password);
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
}
