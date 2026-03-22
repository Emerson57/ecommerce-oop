using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Admin.Commands;
using PlataformaECommerce.Application.Features.Admin.DTOs;
using PlataformaECommerce.Application.Features.Admin.Queries;
using PlataformaECommerce.Application.Interfaces.Repositories.Users;
using PlataformaECommerce.Application.Interfaces.Services.Admin;
using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.ValueObjects;
using PlataformaECommerce.Web.Configuration;
using PlataformaECommerce.Web.Initialization;

namespace PlataformaECommerce.Tests.Web.Initialization;

[TestFixture]
public class SuperUserBootstrapServiceTests
{
    [Test]
    public async Task BootstrapAsync_BootstrapDeshabilitado_NoRegistraAdministrador()
    {
        FakeUserRepository userRepository = new();
        FakeAdminApplicationService adminApplicationService = new();
        SuperUserBootstrapService service = CreateService(
            new BootstrapSuperUserOptions { Enabled = false },
            userRepository,
            adminApplicationService);

        await service.BootstrapAsync(CancellationToken.None);

        Assert.That(adminApplicationService.RegisterCalls, Is.EqualTo(0));
    }

    [Test]
    public async Task BootstrapAsync_SuperUsuarioExistente_NoRegistraAdministrador()
    {
        FakeUserRepository userRepository = new FakeUserRepository { SuperUsersExist = true };
        FakeAdminApplicationService adminApplicationService = new();
        SuperUserBootstrapService service = CreateService(
            CreateEnabledOptions(),
            userRepository,
            adminApplicationService);

        await service.BootstrapAsync(CancellationToken.None);

        Assert.That(adminApplicationService.RegisterCalls, Is.EqualTo(0));
    }

    [Test]
    public async Task BootstrapAsync_AdministradorExistenteSinSuperUsuario_RegistraSuperUsuarioBootstrap()
    {
        FakeUserRepository userRepository = new FakeUserRepository { AdministratorsExist = true, SuperUsersExist = false };
        FakeAdminApplicationService adminApplicationService = new();
        SuperUserBootstrapService service = CreateService(
            CreateEnabledOptions(),
            userRepository,
            adminApplicationService);

        await service.BootstrapAsync(CancellationToken.None);

        Assert.That(adminApplicationService.RegisterCalls, Is.EqualTo(1));
        Assert.That(adminApplicationService.LastCommand?.Role, Is.EqualTo(RolUsuario.SuperUsuario));
    }

    [Test]
    public async Task BootstrapAsync_SinUsuariosAdministrativos_RegistraSuperUsuarioBootstrap()
    {
        FakeUserRepository userRepository = new();
        FakeAdminApplicationService adminApplicationService = new();
        SuperUserBootstrapService service = CreateService(
            CreateEnabledOptions(),
            userRepository,
            adminApplicationService);

        await service.BootstrapAsync(CancellationToken.None);

        Assert.That(
            adminApplicationService.LastCommand is
            {
                Role: RolUsuario.SuperUsuario,
                IsBootstrap: true,
                IsActive: true,
                IsEmailConfirmed: true,
                Source: "Web.Startup.Bootstrap",
                Reason: "Bootstrap seguro del primer super usuario.",
                Name: "Super Admin",
                Email: "root@plataforma.com",
                Area: "Plataforma",
                Password: "Password#2026",
                ConfirmPassword: "Password#2026"
            },
            Is.True);
    }

    [Test]
    public void BootstrapAsync_RegistroAdministrativoFallido_LanzaInvalidOperationException()
    {
        FakeUserRepository userRepository = new();
        FakeAdminApplicationService adminApplicationService = new FakeAdminApplicationService
        {
            RegisterResult = Result.Failure<AdminDto>(Error.Failure("Admin.BootstrapFailed", "No fue posible crear el super usuario inicial."))
        };
        SuperUserBootstrapService service = CreateService(
            CreateEnabledOptions(),
            userRepository,
            adminApplicationService);

        AsyncTestDelegate action = async () => await service.BootstrapAsync(CancellationToken.None);

        Assert.That(action, Throws.InvalidOperationException.With.Message.Contains("Admin.BootstrapFailed"));
    }

    private static SuperUserBootstrapService CreateService(
        BootstrapSuperUserOptions options,
        FakeUserRepository userRepository,
        FakeAdminApplicationService adminApplicationService)
    {
        return new SuperUserBootstrapService(
            Options.Create(options),
            userRepository,
            adminApplicationService,
            NullLogger<SuperUserBootstrapService>.Instance);
    }

    private static BootstrapSuperUserOptions CreateEnabledOptions()
    {
        return new BootstrapSuperUserOptions
        {
            Enabled = true,
            Name = " Super Admin ",
            Email = " root@plataforma.com ",
            Password = "Password#2026",
            Area = " Plataforma "
        };
    }

    private sealed class FakeAdminApplicationService : IAdminApplicationService
    {
        public int RegisterCalls { get; private set; }

        public RegisterAdminCommand? LastCommand { get; private set; }

        public Result<AdminDto> RegisterResult { get; set; } = PlataformaECommerce.Application.Common.Results.Result.Success(new AdminDto());

        public Task<Result<AdminDto>> RegisterAdminAsync(RegisterAdminCommand command, CancellationToken cancellationToken = default)
        {
            RegisterCalls++;
            LastCommand = command;
            return Task.FromResult(RegisterResult);
        }

        public Task<Result<AdminRegistrationDefinitionDto>> GetAdminRegistrationDefinitionAsync(GetAdminRegistrationDefinitionQuery query, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Result<AdminDashboardDto>> GetDashboardAsync(GetAdminDashboardQuery query, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Result<AdminUsersBackofficeDto>> GetUsersAsync(GetAdminUsersQuery query, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Result<AdminBackofficeUserDto>> ResetUserPasswordAsync(ResetUserPasswordCommand command, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public bool AdministratorsExist { get; init; }

        public bool SuperUsersExist { get; init; }

        public Task<IReadOnlyCollection<Usuario>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Usuario>>(Array.Empty<Usuario>());

        public Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<Usuario?>(null);

        public Task<Usuario?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
            => Task.FromResult<Usuario?>(null);

        public Task<IReadOnlyCollection<Usuario>> GetByRoleAsync(RolUsuario rol, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Usuario>>(Array.Empty<Usuario>());

        public Task<IReadOnlyCollection<Cliente>> GetCustomersAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Cliente>>(Array.Empty<Cliente>());

        public Task<IReadOnlyCollection<Administrador>> GetAdministratorsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Administrador>>(Array.Empty<Administrador>());

        public Task<Cliente?> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<Cliente?>(null);

        public Task<Administrador?> GetAdministratorByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<Administrador?>(null);

        public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<bool> ExistsByRoleAsync(RolUsuario rol, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(rol switch
            {
                RolUsuario.Administrador => AdministratorsExist,
                RolUsuario.SuperUsuario => SuperUsersExist,
                _ => false
            });
        }

        public Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task UpdateAsync(Usuario usuario, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
