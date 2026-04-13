using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Application.Common.Security;
using PlataformaECommerce.Application.Interfaces.Repositories.Users;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.ValueObjects;
using PlataformaECommerce.Web.Authorization;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Tests.Web.Authorization;

[TestFixture]
public class CustomerCookieSecurityServiceTests
{
    [Test]
    public async Task IsPrincipalValidAsync_ClientePersistidoYSesionVigente_RetornaTrue()
    {
        Cliente customer = CreateCustomer();
        FakeUserRepository userRepository = new(customer);
        CustomerCookieSecurityService service = new(userRepository, new FakeTenantContextAccessor("tenant-demo"), Options.Create(new WebAuthenticationCookiesOptions()));

        bool result = await service.IsPrincipalValidAsync(
            CreatePrincipal(customer),
            new AuthenticationProperties
            {
                IssuedUtc = DateTimeOffset.UtcNow.AddMinutes(-5),
                IsPersistent = false
            },
            CancellationToken.None);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task IsPrincipalValidAsync_ClaimsDeClienteSinActorPersistido_RetornaFalse()
    {
        CustomerCookieSecurityService service = new(new FakeUserRepository(), new FakeTenantContextAccessor("tenant-demo"), Options.Create(new WebAuthenticationCookiesOptions()));

        bool result = await service.IsPrincipalValidAsync(
            CreatePrincipal(CreateCustomer()),
            new AuthenticationProperties
            {
                IssuedUtc = DateTimeOffset.UtcNow.AddMinutes(-5),
                IsPersistent = false
            },
            CancellationToken.None);

        Assert.That(result, Is.False);
    }

    private static Cliente CreateCustomer()
    {
        Cliente customer = new(
            "Cliente Demo",
            new Email($"cliente-{Guid.NewGuid():N}@plataforma.com"),
            "hash-cliente-demo-seguro-2026");
        customer.ConfirmarCorreoElectronico();
        return customer;
    }

    private static ClaimsPrincipal CreatePrincipal(Cliente actor)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, actor.Id.ToString()),
            new Claim(ClaimTypes.Name, actor.Nombre),
            new Claim(ClaimTypes.Email, actor.CorreoElectronico.Value),
            new Claim(SecurityClaimTypes.TenantId, "tenant-demo"),
            new Claim(ClaimTypes.Role, RolUsuario.Cliente.ToString()),
            new Claim(AuthorizationPolicies.PrimaryRoleClaimType, RolUsuario.Cliente.ToString()),
            new Claim(AuthorizationPolicies.SuperUserClaimType, bool.FalseString)
        ], AuthorizationPolicies.CustomerCookieScheme));
    }

    private sealed class FakeTenantContextAccessor(string tenantId) : ITenantContextAccessor
    {
        public string TenantId { get; } = tenantId;
        public bool IsAvailable => !string.IsNullOrWhiteSpace(TenantId);
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
}
