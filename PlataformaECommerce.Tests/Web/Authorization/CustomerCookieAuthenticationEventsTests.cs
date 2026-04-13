using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Application.Common.Security;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Web.Authorization;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Tests.Web.Authorization;

[TestFixture]
public class CustomerCookieAuthenticationEventsTests
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
            AuthorizationPolicies.CustomerCookieScheme,
            AuthorizationPolicies.CustomerCookieScheme,
            typeof(CookieAuthenticationHandler));
        CookieAuthenticationOptions options = new();
        AuthenticationTicket ticket = new(
            CreateCustomerPrincipal(),
            new AuthenticationProperties(),
            AuthorizationPolicies.CustomerCookieScheme);
        CookieValidatePrincipalContext context = new(httpContext, scheme, options, ticket);
        CustomerCookieAuthenticationEvents events = new(new CustomerCookieSecurityService(new FakeUserRepository(), new FakeTenantContextAccessor("tenant-demo"), Options.Create(new WebAuthenticationCookiesOptions())));

        await events.ValidatePrincipal(context);

        Assert.That(authenticationService.SignOutCalls, Is.EqualTo(1));
        Assert.That(authenticationService.LastSignOutScheme, Is.EqualTo(AuthorizationPolicies.CustomerCookieScheme));
    }

    [Test]
    public async Task RedirectToLogin_SolicitudApi_Retorna401SinRedireccion()
    {
        DefaultHttpContext httpContext = new();
        httpContext.Request.Path = "/api/orders";

        RedirectContext<CookieAuthenticationOptions> context = CreateRedirectContext(httpContext);
        CustomerCookieAuthenticationEvents events = new(new CustomerCookieSecurityService(new FakeUserRepository(), new FakeTenantContextAccessor("tenant-demo"), Options.Create(new WebAuthenticationCookiesOptions())));

        await events.RedirectToLogin(context);

        Assert.That(httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
        Assert.That(httpContext.Response.Headers.Location.ToString(), Is.Empty);
    }

    [Test]
    public async Task RedirectToAccessDenied_SolicitudApi_Retorna403SinRedireccion()
    {
        DefaultHttpContext httpContext = new();
        httpContext.Request.Path = "/api/orders";

        RedirectContext<CookieAuthenticationOptions> context = CreateRedirectContext(httpContext);
        CustomerCookieAuthenticationEvents events = new(new CustomerCookieSecurityService(new FakeUserRepository(), new FakeTenantContextAccessor("tenant-demo"), Options.Create(new WebAuthenticationCookiesOptions())));

        await events.RedirectToAccessDenied(context);

        Assert.That(httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        Assert.That(httpContext.Response.Headers.Location.ToString(), Is.Empty);
    }

    private static System.Security.Claims.ClaimsPrincipal CreateCustomerPrincipal()
    {
        return new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(
        [
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "Cliente Demo"),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "cliente@plataforma.com"),
            new System.Security.Claims.Claim(SecurityClaimTypes.TenantId, "tenant-demo"),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Cliente"),
            new System.Security.Claims.Claim(AuthorizationPolicies.PrimaryRoleClaimType, "Cliente"),
            new System.Security.Claims.Claim(AuthorizationPolicies.SuperUserClaimType, bool.FalseString)
        ], AuthorizationPolicies.CustomerCookieScheme));
    }

    private sealed class FakeTenantContextAccessor(string tenantId) : ITenantContextAccessor
    {
        public string TenantId { get; } = tenantId;
        public bool IsAvailable => !string.IsNullOrWhiteSpace(TenantId);
    }

    private static RedirectContext<CookieAuthenticationOptions> CreateRedirectContext(HttpContext httpContext)
    {
        CookieAuthenticationOptions options = new();
        AuthenticationScheme scheme = new(
            AuthorizationPolicies.CustomerCookieScheme,
            AuthorizationPolicies.CustomerCookieScheme,
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

        public Task SignInAsync(HttpContext context, string? scheme, System.Security.Claims.ClaimsPrincipal principal, AuthenticationProperties? properties)
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
