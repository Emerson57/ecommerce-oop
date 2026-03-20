using PlataformaECommerce.Application.Features.Users.Commands;
using PlataformaECommerce.Application.Features.Users.Services;
using PlataformaECommerce.Application.Features.Users.Validators;
using PlataformaECommerce.Application.Interfaces.Persistence;
using PlataformaECommerce.Application.Interfaces.Repositories.Users;
using PlataformaECommerce.Application.Interfaces.Services.Audit;
using PlataformaECommerce.Application.Interfaces.Services.Auth;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Tests.Application.Users;

[TestFixture]
public class UserApplicationServiceTests
{
    [Test]
    public async Task RegisterCustomerAsync_OperacionExitosa_RegistraEventoDeAuditoria()
    {
        FakeUserRepository userRepository = new();
        FakeAuditTrailService auditTrailService = new();
        UserApplicationService service = new(
            userRepository,
            new FakeUnitOfWork(),
            new FakePasswordHasher(),
            auditTrailService,
            new RegisterCustomerCommandValidator(),
            new UpdateUserBasicDataCommandValidator());

        await service.RegisterCustomerAsync(new RegisterCustomerCommand
        {
            Name = "Cliente Demo",
            Email = "cliente@plataforma.com",
            Password = "Password#2026",
            ConfirmPassword = "Password#2026",
            Preferences = new[] { "tecnologia" },
            AcceptTermsAndConditions = true,
            AcceptPrivacyPolicy = true
        });

        Assert.That(auditTrailService.RegisteredEvents.Count, Is.EqualTo(1));
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
}
