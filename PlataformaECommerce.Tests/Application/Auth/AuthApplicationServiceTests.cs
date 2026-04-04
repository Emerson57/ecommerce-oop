using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Auth.Commands;
using PlataformaECommerce.Application.Features.Auth.DTOs;
using PlataformaECommerce.Application.Features.Auth.Queries;
using PlataformaECommerce.Application.Features.Auth.Services;
using PlataformaECommerce.Application.Features.Auth.Validators;
using PlataformaECommerce.Application.Interfaces.Persistence;
using PlataformaECommerce.Application.Interfaces.Repositories.Users;
using PlataformaECommerce.Application.Interfaces.Services.Audit;
using PlataformaECommerce.Application.Interfaces.Services.Auth;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Application.Common.Notifications;
using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.ValueObjects;
using System.Security.Claims;

namespace PlataformaECommerce.Tests.Application.Auth;

[TestFixture]
public class AuthApplicationServiceTests
{
    [Test]
    public async Task LoginAsync_AdministradorCreado_PermiteAutenticacionParaBackoffice()
    {
        FakeUserRepository userRepository = new();
        Administrador administrator = new(
            "Admin Acceso",
            new Email("admin.acceso@plataforma.com"),
            FakePasswordHasher.Hash("Password#2026"),
            "Operaciones");
        administrator.ConfirmarCorreoElectronico();
        await userRepository.AddAsync(administrator);
        FakeUnitOfWork unitOfWork = new();

        AuthApplicationService service = new(
            userRepository,
            new FakePasswordHasher(),
            new FakeTokenService(),
            unitOfWork,
            new LoginCommandValidator(),
            new RequestPasswordResetCommandValidator(),
            new ChangePasswordCommandValidator(),
            new ResetPasswordCommandValidator(),
            new FakePasswordResetTokenService(),
            new FakeAuditTrailService(),
            new FakeEmailNotificationService());

        Result<AuthResponseDto> result = await service.LoginAsync(new LoginCommand
        {
            Email = "admin.acceso@plataforma.com",
            Password = "Password#2026",
            ExternalReference = "Tests.Auth.AdminLogin"
        });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.User.Role, Is.EqualTo(RolUsuario.Administrador.ToString()));
        Assert.That(result.Value.User.Area, Is.EqualTo("Operaciones"));
        Assert.That(result.Value.User.Roles.Single(), Is.EqualTo(RolUsuario.Administrador.ToString()));
        Assert.That(unitOfWork.SaveChangesCalls, Is.EqualTo(1));
        Assert.That(userRepository.UpdatedUsers.Single().FechaUltimoAccesoUtc, Is.Not.Null);
    }

    [Test]
    public async Task LoginAsync_AdministradorSinCorreoConfirmado_RetornaErrorDeAutenticacion()
    {
        FakeUserRepository userRepository = new();
        Administrador administrator = new(
            "Admin Pendiente",
            new Email("admin.pendiente@plataforma.com"),
            FakePasswordHasher.Hash("Password#2026"),
            "Operaciones");
        await userRepository.AddAsync(administrator);

        AuthApplicationService service = new(
            userRepository,
            new FakePasswordHasher(),
            new FakeTokenService(),
            new FakeUnitOfWork(),
            new LoginCommandValidator(),
            new RequestPasswordResetCommandValidator(),
            new ChangePasswordCommandValidator(),
            new ResetPasswordCommandValidator(),
            new FakePasswordResetTokenService(),
            new FakeAuditTrailService(),
            new FakeEmailNotificationService());

        Result<AuthResponseDto> result = await service.LoginAsync(new LoginCommand
        {
            Email = "admin.pendiente@plataforma.com",
            Password = "Password#2026"
        });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo("Auth.EmailNotConfirmed"));
    }

    [Test]
    public async Task ChangePasswordAsync_ClienteAutenticado_ActualizaHashYRegistraAuditoria()
    {
        FakeUserRepository userRepository = new();
        Cliente customer = new(
            "Cliente Seguro",
            new Email("cliente.seguro@plataforma.com"),
            FakePasswordHasher.Hash("Password#2026"));
        customer.ConfirmarCorreoElectronico();
        await userRepository.AddAsync(customer);
        FakeUnitOfWork unitOfWork = new();
        FakeAuditTrailService auditTrailService = new();
        FakeEmailNotificationService emailNotificationService = new();

        AuthApplicationService service = new(
            userRepository,
            new FakePasswordHasher(),
            new FakeTokenService(),
            unitOfWork,
            new LoginCommandValidator(),
            new RequestPasswordResetCommandValidator(),
            new ChangePasswordCommandValidator(),
            new ResetPasswordCommandValidator(),
            new FakePasswordResetTokenService(),
            auditTrailService,
            emailNotificationService);

        Result result = await service.ChangePasswordAsync(new ChangePasswordCommand
        {
            UserId = customer.Id,
            CurrentPassword = "Password#2026",
            NewPassword = "Password#2027",
            ConfirmPassword = "Password#2027",
            Source = "Tests.Auth.ChangePassword"
        });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(customer.ContrasenaHash, Is.EqualTo(FakePasswordHasher.Hash("Password#2027")));
        Assert.That(unitOfWork.SaveChangesCalls, Is.EqualTo(1));
        Assert.That(auditTrailService.RegisteredActions, Does.Contain("auth.password-changed"));
    }

    [Test]
    public async Task ChangePasswordAsync_ContrasenaActualIncorrecta_RetornaErrorDeAutorizacion()
    {
        FakeUserRepository userRepository = new();
        Cliente customer = new(
            "Cliente Seguro",
            new Email("cliente.seguro@plataforma.com"),
            FakePasswordHasher.Hash("Password#2026"));
        customer.ConfirmarCorreoElectronico();
        await userRepository.AddAsync(customer);

        AuthApplicationService service = new(
            userRepository,
            new FakePasswordHasher(),
            new FakeTokenService(),
            new FakeUnitOfWork(),
            new LoginCommandValidator(),
            new RequestPasswordResetCommandValidator(),
            new ChangePasswordCommandValidator(),
            new ResetPasswordCommandValidator(),
            new FakePasswordResetTokenService(),
            new FakeAuditTrailService(),
            new FakeEmailNotificationService());

        Result result = await service.ChangePasswordAsync(new ChangePasswordCommand
        {
            UserId = customer.Id,
            CurrentPassword = "Password#Incorrecta",
            NewPassword = "Password#2027",
            ConfirmPassword = "Password#2027",
            Source = "Tests.Auth.ChangePassword"
        });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo("Auth.InvalidCurrentPassword"));
    }

    [Test]
    public async Task RequestPasswordResetAsync_UsuarioHabilitado_EnviaCorreoDeRecuperacion()
    {
        FakeUserRepository userRepository = new();
        Cliente customer = new(
            "Cliente Seguro",
            new Email("cliente.seguro@plataforma.com"),
            FakePasswordHasher.Hash("Password#2026"));
        customer.ConfirmarCorreoElectronico();
        await userRepository.AddAsync(customer);
        FakeEmailNotificationService emailNotificationService = new();

        AuthApplicationService service = new(
            userRepository,
            new FakePasswordHasher(),
            new FakeTokenService(),
            new FakeUnitOfWork(),
            new LoginCommandValidator(),
            new RequestPasswordResetCommandValidator(),
            new ChangePasswordCommandValidator(),
            new ResetPasswordCommandValidator(),
            new FakePasswordResetTokenService(),
            new FakeAuditTrailService(),
            emailNotificationService);

        Result<PasswordResetRequestResultDto> result = await service.RequestPasswordResetAsync(new RequestPasswordResetCommand
        {
            Email = customer.CorreoElectronico.Value,
            ResetPasswordUrl = "https://shop.example.com/Auth/ResetPassword?userId={userId}&token={token}",
            Source = "Tests.Auth.ForgotPassword",
            RequestedAtUtc = DateTime.UtcNow
        });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(emailNotificationService.LastPasswordResetNotification?.ToEmail, Is.EqualTo(customer.CorreoElectronico.Value));
        Assert.That(emailNotificationService.LastPasswordResetNotification?.ResetUrl, Does.Contain(customer.Id.ToString()));
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly List<Usuario> _users = new();

        public List<Usuario> UpdatedUsers { get; } = new();

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
        {
            UpdatedUsers.Add(usuario);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _users.RemoveAll(user => user.Id == id);
            return Task.CompletedTask;
        }
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public static string Hash(string password) => $"hash::{password}::seguro-2026";

        public string HashPassword(string password) => Hash(password);

        public bool VerifyPassword(string password, string passwordHash) => passwordHash == Hash(password);
    }

    private sealed class FakeTokenService : ITokenService
    {
        public string GenerateAccessToken(Usuario usuario) => $"access::{usuario.Id}";
        public string GenerateRefreshToken(Usuario usuario) => $"refresh::{usuario.Id}";
        public DateTime GetAccessTokenExpirationUtc(string accessToken) => DateTime.UtcNow.AddMinutes(30);
        public DateTime GetRefreshTokenExpirationUtc(string refreshToken) => DateTime.UtcNow.AddDays(7);
        public ClaimsPrincipal? GetPrincipalFromAccessToken(string accessToken) => null;
        public ClaimsPrincipal? GetPrincipalFromRefreshToken(string refreshToken) => null;
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCalls { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCalls++;
            return Task.FromResult(1);
        }

        public Task BeginTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task CommitTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed class FakePasswordResetTokenService : IPasswordResetTokenService
    {
        public string GenerateToken(Usuario usuario, TimeSpan lifetime)
        {
            ArgumentNullException.ThrowIfNull(usuario);

            return $"reset::{usuario.Id}::{lifetime.Ticks}";
        }

        public PasswordResetTokenValidationDto? ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            return new PasswordResetTokenValidationDto
            {
                UserId = Guid.NewGuid(),
                Email = "preview@plataforma.com",
                UserVersionTicks = DateTime.UtcNow.Ticks,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(30)
            };
        }
    }

    private sealed class FakeAuditTrailService : IAuditTrailService
    {
        public List<string> RegisteredActions { get; } = new();

        public Task RegisterAsync(
            Guid aggregateId,
            string aggregateType,
            string module,
            string action,
            string detail,
            IReadOnlyDictionary<string, string>? metadata = null,
            CancellationToken cancellationToken = default)
        {
            RegisteredActions.Add(action);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeEmailNotificationService : IEmailNotificationService
    {
        public PasswordResetEmailNotification? LastPasswordResetNotification { get; private set; }

        public Task<Result> SendAccountEmailConfirmationAsync(AccountEmailConfirmationNotification notification, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());

        public Task<Result> SendPasswordResetEmailAsync(PasswordResetEmailNotification notification, CancellationToken cancellationToken = default)
        {
            LastPasswordResetNotification = notification;
            return Task.FromResult(Result.Success());
        }

        public Task<Result> SendOrderConfirmationEmailAsync(OrderConfirmationEmailNotification notification, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());
    }
}
