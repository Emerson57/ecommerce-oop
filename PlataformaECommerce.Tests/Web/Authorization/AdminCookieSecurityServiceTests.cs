using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using PlataformaECommerce.Application.Common.Security;
using PlataformaECommerce.Application.Interfaces.Repositories.Users;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.ValueObjects;
using PlataformaECommerce.Web.Authorization;

namespace PlataformaECommerce.Tests.Web.Authorization;

[TestFixture]
public class AdminCookieSecurityServiceTests
{
    [Test]
    public async Task IsPrincipalValidAsync_SuperUsuarioPersistidoYSesionVigente_RetornaTrue()
    {
        Administrador superUser = CreateSuperUser();
        FakeUserRepository userRepository = new(superUser);
        AdminCookieSecurityService service = new(userRepository, new FakeTenantContextAccessor("tenant-demo"));

        bool result = await service.IsPrincipalValidAsync(
            CreatePrincipal(superUser),
            CreateAuthenticationProperties(DateTimeOffset.UtcNow.AddHours(1)),
            CancellationToken.None);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task IsPrincipalValidAsync_SesionExpirada_RetornaFalse()
    {
        Administrador superUser = CreateSuperUser();
        FakeUserRepository userRepository = new(superUser);
        AdminCookieSecurityService service = new(userRepository, new FakeTenantContextAccessor("tenant-demo"));

        bool result = await service.IsPrincipalValidAsync(
            CreatePrincipal(superUser),
            CreateAuthenticationProperties(DateTimeOffset.UtcNow.AddMinutes(-1)),
            CancellationToken.None);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task IsPrincipalValidAsync_ClaimsDeSuperUsuarioSinActorPersistido_RetornaFalse()
    {
        AdminCookieSecurityService service = new(new FakeUserRepository(), new FakeTenantContextAccessor("tenant-demo"));

        bool result = await service.IsPrincipalValidAsync(
            CreatePrincipal(CreateSuperUser()),
            CreateAuthenticationProperties(DateTimeOffset.UtcNow.AddHours(1)),
            CancellationToken.None);

        Assert.That(result, Is.False);
    }

    private static Administrador CreateSuperUser()
    {
        Administrador superUser = new(
            "Root Demo",
            new Email($"root-{Guid.NewGuid():N}@plataforma.com"),
            "hash-root-demo-seguro-2026",
            "Plataforma",
            RolUsuario.SuperUsuario);
        superUser.ConfirmarCorreoElectronico();
        return superUser;
    }

    private static ClaimsPrincipal CreatePrincipal(Administrador actor)
    {
        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, actor.Id.ToString()),
            new(ClaimTypes.Name, actor.Nombre),
            new(ClaimTypes.Email, actor.CorreoElectronico.Value),
            new(SecurityClaimTypes.TenantId, "tenant-demo"),
            new(AuthorizationPolicies.PrimaryRoleClaimType, actor.Rol.ToString()),
            new(AuthorizationPolicies.AdminAreaClaimType, actor.Area),
            new(AuthorizationPolicies.SuperUserClaimType, actor.EsSuperUsuario.ToString())
        ];

        foreach (string role in actor.Rol.ObtenerRolesEfectivos())
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, AuthorizationPolicies.AdminCookieScheme));
    }

    private static AuthenticationProperties CreateAuthenticationProperties(DateTimeOffset absoluteExpirationUtc)
    {
        return new AuthenticationProperties
        {
            IssuedUtc = DateTimeOffset.UtcNow.AddMinutes(-5),
            Items =
            {
                ["auth:absolute-expiration-utc"] = absoluteExpirationUtc.ToString("O")
            }
        };
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
