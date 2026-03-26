using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using PlataformaECommerce.Web.Authorization;

namespace PlataformaECommerce.Tests.Web.Authorization;

[TestFixture]
public class AdminCookieAuthenticationEventsTests
{
    [Test]
    public async Task ValidatePrincipal_PrincipalInvalido_RechazaYRevocaLaCookie()
    {
        FakeAuthenticationService authenticationService = new();
        ServiceCollection services = new();
        services.AddSingleton<IAuthenticationService>(authenticationService);
        DefaultHttpContext httpContext = new()
        {
            RequestServices = services.BuildServiceProvider()
        };

        AuthenticationScheme scheme = new(
            AuthorizationPolicies.AdminCookieScheme,
            AuthorizationPolicies.AdminCookieScheme,
            typeof(CookieAuthenticationHandler));
        CookieAuthenticationOptions options = new();
        AuthenticationTicket ticket = new(
            CreateAdministrativePrincipal(),
            new AuthenticationProperties(),
            AuthorizationPolicies.AdminCookieScheme);
        CookieValidatePrincipalContext context = new(httpContext, scheme, options, ticket);
        AdminCookieAuthenticationEvents events = new(
            new AdminCookieSecurityService(new FakeUserRepository()),
            NullLogger<AdminCookieAuthenticationEvents>.Instance);

        await events.ValidatePrincipal(context);

        Assert.That(authenticationService.SignOutCalls, Is.EqualTo(1));
        Assert.That(authenticationService.LastSignOutScheme, Is.EqualTo(AuthorizationPolicies.AdminCookieScheme));
    }

    [Test]
    public async Task RedirectToLogin_SolicitudApi_Retorna401SinRedireccion()
    {
        DefaultHttpContext httpContext = new();
        httpContext.Request.Path = "/api/admin/products";

        RedirectContext<CookieAuthenticationOptions> context = CreateRedirectContext(httpContext);
        AdminCookieAuthenticationEvents events = new(
            new AdminCookieSecurityService(new FakeUserRepository()),
            NullLogger<AdminCookieAuthenticationEvents>.Instance);

        await events.RedirectToLogin(context);

        Assert.That(httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
        Assert.That(httpContext.Response.Headers.Location.ToString(), Is.Empty);
    }

    [Test]
    public async Task RedirectToAccessDenied_SolicitudApi_Retorna403SinRedireccion()
    {
        DefaultHttpContext httpContext = new();
        httpContext.Request.Path = "/api/admin/products";

        RedirectContext<CookieAuthenticationOptions> context = CreateRedirectContext(httpContext);
        AdminCookieAuthenticationEvents events = new(
            new AdminCookieSecurityService(new FakeUserRepository()),
            NullLogger<AdminCookieAuthenticationEvents>.Instance);

        await events.RedirectToAccessDenied(context);

        Assert.That(httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        Assert.That(httpContext.Response.Headers.Location.ToString(), Is.Empty);
    }

    private static ClaimsPrincipal CreateAdministrativePrincipal()
    {
        return new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, "Admin Demo"),
            new Claim(ClaimTypes.Role, "Administrador"),
            new Claim(AuthorizationPolicies.PrimaryRoleClaimType, "Administrador"),
            new Claim(AuthorizationPolicies.SuperUserClaimType, bool.FalseString),
            new Claim(AuthorizationPolicies.AdminAreaClaimType, "Operaciones")
        ], AuthorizationPolicies.AdminCookieScheme));
    }

    private static RedirectContext<CookieAuthenticationOptions> CreateRedirectContext(HttpContext httpContext)
    {
        CookieAuthenticationOptions options = new();
        AuthenticationScheme scheme = new(
            AuthorizationPolicies.AdminCookieScheme,
            AuthorizationPolicies.AdminCookieScheme,
            typeof(CookieAuthenticationHandler));

        return new RedirectContext<CookieAuthenticationOptions>(
            httpContext,
            scheme,
            options,
            new AuthenticationProperties(),
            "/Auth/Login");
    }

    private sealed class FakeAuthenticationService : IAuthenticationService
    {
        public int SignOutCalls { get; private set; }
        public string? LastSignOutScheme { get; private set; }

        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme)
            => Task.FromResult(AuthenticateResult.NoResult());

        public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
        {
            SignOutCalls++;
            LastSignOutScheme = scheme;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUserRepository : PlataformaECommerce.Application.Interfaces.Repositories.Users.IUserRepository
    {
        public Task<IReadOnlyCollection<PlataformaECommerce.Domain.Entities.Users.Usuario>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<PlataformaECommerce.Domain.Entities.Users.Usuario>>(Array.Empty<PlataformaECommerce.Domain.Entities.Users.Usuario>());
        public Task<PlataformaECommerce.Domain.Entities.Users.Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<PlataformaECommerce.Domain.Entities.Users.Usuario?>(null);
        public Task<PlataformaECommerce.Domain.Entities.Users.Usuario?> GetByEmailAsync(PlataformaECommerce.Domain.ValueObjects.Email email, CancellationToken cancellationToken = default) => Task.FromResult<PlataformaECommerce.Domain.Entities.Users.Usuario?>(null);
        public Task<IReadOnlyCollection<PlataformaECommerce.Domain.Entities.Users.Usuario>> GetByRoleAsync(PlataformaECommerce.Domain.Enums.RolUsuario rol, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<PlataformaECommerce.Domain.Entities.Users.Usuario>>(Array.Empty<PlataformaECommerce.Domain.Entities.Users.Usuario>());
        public Task<IReadOnlyCollection<PlataformaECommerce.Domain.Entities.Users.Cliente>> GetCustomersAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<PlataformaECommerce.Domain.Entities.Users.Cliente>>(Array.Empty<PlataformaECommerce.Domain.Entities.Users.Cliente>());
        public Task<IReadOnlyCollection<PlataformaECommerce.Domain.Entities.Users.Administrador>> GetAdministratorsAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<PlataformaECommerce.Domain.Entities.Users.Administrador>>(Array.Empty<PlataformaECommerce.Domain.Entities.Users.Administrador>());
        public Task<PlataformaECommerce.Domain.Entities.Users.Cliente?> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<PlataformaECommerce.Domain.Entities.Users.Cliente?>(null);
        public Task<PlataformaECommerce.Domain.Entities.Users.Administrador?> GetAdministratorByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<PlataformaECommerce.Domain.Entities.Users.Administrador?>(null);
        public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> ExistsByEmailAsync(PlataformaECommerce.Domain.ValueObjects.Email email, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> ExistsByRoleAsync(PlataformaECommerce.Domain.Enums.RolUsuario rol, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task AddAsync(PlataformaECommerce.Domain.Entities.Users.Usuario usuario, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateAsync(PlataformaECommerce.Domain.Entities.Users.Usuario usuario, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
