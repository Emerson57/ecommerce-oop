using PlataformaECommerce.Application.Common.Notifications;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Users.Commands;
using PlataformaECommerce.Application.Features.Users.DTOs;
using PlataformaECommerce.Application.Features.Users.Services;
using PlataformaECommerce.Application.Features.Users.Validators;
using PlataformaECommerce.Application.Interfaces.Persistence;
using PlataformaECommerce.Application.Interfaces.Repositories.Users;
using PlataformaECommerce.Application.Interfaces.Services.Audit;
using PlataformaECommerce.Application.Interfaces.Services.Auth;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Application.Interfaces.Services.Users;
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
        FakeEmailNotificationService emailNotificationService = new();
        UserApplicationService service = new(
            userRepository,
            new FakeUnitOfWork(),
            new FakePasswordHasher(),
            auditTrailService,
            new FakeEmailConfirmationTokenService(),
            emailNotificationService,
            new RegisterCustomerCommandValidator(),
            new UpdateUserBasicDataCommandValidator(),
            new ResendUserEmailConfirmationCommandValidator());

        await service.RegisterCustomerAsync(new RegisterCustomerCommand
        {
            Name = "Cliente Demo",
            Email = "cliente@plataforma.com",
            Password = "Password#2026",
            ConfirmPassword = "Password#2026",
            Preferences = new[] { "tecnologia" },
            EmailConfirmationUrl = "https://shop.example.com/Auth/ConfirmEmail?userId={userId}&token={token}",
            AcceptTermsAndConditions = true,
            AcceptPrivacyPolicy = true
        });

        Assert.That(auditTrailService.RegisteredEvents, Does.Contain("user.customer.registered"));
        Assert.That(auditTrailService.RegisteredEvents, Does.Contain("user.email-confirmation.sent"));
        Assert.That(emailNotificationService.LastAccountEmailConfirmationNotification?.ToEmail, Is.EqualTo("cliente@plataforma.com"));
    }

    [Test]
    public async Task ConfirmUserEmailAsync_TokenValido_ConfirmaCorreoDelUsuario()
    {
        FakeUserRepository userRepository = new();
        Cliente customer = new("Cliente Demo", new Email("cliente@plataforma.com"), "hash-Password#2026-seguro-2026");
        await userRepository.AddAsync(customer);
        UserApplicationService service = new(
            userRepository,
            new FakeUnitOfWork(),
            new FakePasswordHasher(),
            new FakeAuditTrailService(),
            new FakeEmailConfirmationTokenService(customer),
            new FakeEmailNotificationService(),
            new RegisterCustomerCommandValidator(),
            new UpdateUserBasicDataCommandValidator(),
            new ResendUserEmailConfirmationCommandValidator());

        var result = await service.ConfirmUserEmailAsync(new ConfirmUserEmailCommand
        {
            UserId = customer.Id,
            ConfirmationToken = "confirm-token"
        });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.IsEmailConfirmed, Is.True);
    }

    [Test]
    public async Task ResendUserEmailConfirmationAsync_UsuarioNoConfirmado_ReenviaCorreo()
    {
        FakeUserRepository userRepository = new();
        Cliente customer = new("Cliente Demo", new Email("cliente@plataforma.com"), "hash-Password#2026-seguro-2026");
        await userRepository.AddAsync(customer);
        FakeEmailNotificationService emailNotificationService = new();
        UserApplicationService service = new(
            userRepository,
            new FakeUnitOfWork(),
            new FakePasswordHasher(),
            new FakeAuditTrailService(),
            new FakeEmailConfirmationTokenService(customer),
            emailNotificationService,
            new RegisterCustomerCommandValidator(),
            new UpdateUserBasicDataCommandValidator(),
            new ResendUserEmailConfirmationCommandValidator());

        Result result = await service.ResendUserEmailConfirmationAsync(new ResendUserEmailConfirmationCommand
        {
            Email = customer.CorreoElectronico.Value,
            EmailConfirmationUrl = "https://shop.example.com/Auth/ConfirmEmail?userId={userId}&token={token}"
        });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(emailNotificationService.AccountEmailConfirmationSendCount, Is.EqualTo(1));
    }

    [Test]
    public async Task ResendUserEmailConfirmationAsync_UsuarioYaConfirmado_NoReenviaCorreo()
    {
        FakeUserRepository userRepository = new();
        Cliente customer = new("Cliente Demo", new Email("cliente@plataforma.com"), "hash-Password#2026-seguro-2026");
        customer.ConfirmarCorreoElectronico();
        await userRepository.AddAsync(customer);
        FakeEmailNotificationService emailNotificationService = new();
        UserApplicationService service = new(
            userRepository,
            new FakeUnitOfWork(),
            new FakePasswordHasher(),
            new FakeAuditTrailService(),
            new FakeEmailConfirmationTokenService(customer),
            emailNotificationService,
            new RegisterCustomerCommandValidator(),
            new UpdateUserBasicDataCommandValidator(),
            new ResendUserEmailConfirmationCommandValidator());

        Result result = await service.ResendUserEmailConfirmationAsync(new ResendUserEmailConfirmationCommand
        {
            Email = customer.CorreoElectronico.Value,
            EmailConfirmationUrl = "https://shop.example.com/Auth/ConfirmEmail?userId={userId}&token={token}"
        });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(emailNotificationService.AccountEmailConfirmationSendCount, Is.EqualTo(0));
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly List<Usuario> _users = new();

        public Task<IReadOnlyCollection<Usuario>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Usuario>>(_users.ToArray());

        public Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_users.FirstOrDefault(user => user.Id == id));

        public Task<Usuario?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
            => Task.FromResult(_users.FirstOrDefault(user => user.CorreoElectronico.Equals(email)));

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
            => Task.FromResult(_users.Any(user => user.CorreoElectronico.Equals(email)));

        public Task<bool> ExistsByRoleAsync(RolUsuario rol, CancellationToken cancellationToken = default)
            => Task.FromResult(_users.Any(user => user.Rol == rol));

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

    private sealed class FakeEmailConfirmationTokenService : IEmailConfirmationTokenService
    {
        private readonly Cliente? _customer;

        public FakeEmailConfirmationTokenService(Cliente? customer = null)
        {
            _customer = customer;
        }

        public string GenerateToken(Usuario usuario, TimeSpan lifetime) => "confirm-token";

        public EmailConfirmationTokenValidationDto? ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token) || _customer is null)
            {
                return null;
            }

            return new EmailConfirmationTokenValidationDto
            {
                UserId = _customer.Id,
                Email = _customer.CorreoElectronico.Value,
                UserVersionTicks = (_customer.FechaActualizacionUtc ?? _customer.FechaCreacionUtc).Ticks,
                ExpiresAtUtc = DateTime.UtcNow.AddHours(1)
            };
        }
    }

    private sealed class FakeEmailNotificationService : IEmailNotificationService
    {
        public int AccountEmailConfirmationSendCount { get; private set; }
        public AccountEmailConfirmationNotification? LastAccountEmailConfirmationNotification { get; private set; }

        public Task<Result> SendAccountEmailConfirmationAsync(AccountEmailConfirmationNotification notification, CancellationToken cancellationToken = default)
        {
            AccountEmailConfirmationSendCount++;
            LastAccountEmailConfirmationNotification = notification;
            return Task.FromResult(Result.Success());
        }

        public Task<Result> SendPasswordResetEmailAsync(PasswordResetEmailNotification notification, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());

        public Task<Result> SendOrderConfirmationEmailAsync(OrderConfirmationEmailNotification notification, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());
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
