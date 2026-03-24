using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using PlataformaECommerce.Application.Common.Security;
using PlataformaECommerce.Web.Pages.Admin.Users;
using PlataformaECommerce.Web.Authorization;

namespace PlataformaECommerce.Tests.Web.Authorization;

[TestFixture]
public class AuthorizationPoliciesTests
{
    [Test]
    public void IsAdministrativePrincipal_AdministradorValido_RetornaTrue()
    {
        ClaimsPrincipal principal = CreatePrincipal(
            primaryRole: "Administrador",
            roles: ["Administrador"],
            isSuperUser: false);

        bool result = AuthorizationPolicies.IsAdministrativePrincipal(principal);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsSuperUserPrincipal_SuperUsuarioValido_RetornaTrue()
    {
        ClaimsPrincipal principal = CreatePrincipal(
            primaryRole: "SuperUsuario",
            roles: ["SuperUsuario", "Administrador"],
            isSuperUser: true);

        bool result = AuthorizationPolicies.IsSuperUserPrincipal(principal);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsSuperUserPrincipal_RolesInconsistentes_RetornaFalse()
    {
        ClaimsPrincipal principal = CreatePrincipal(
            primaryRole: "Administrador",
            roles: ["SuperUsuario", "Administrador"],
            isSuperUser: true);

        bool result = AuthorizationPolicies.IsSuperUserPrincipal(principal);

        Assert.That(result, Is.False);
    }

    [Test]
    public void IsCustomerPrincipal_ClienteValido_RetornaTrue()
    {
        ClaimsPrincipal principal = new(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, "Cliente Demo"),
            new Claim(ClaimTypes.Email, "cliente@plataforma.com"),
            new Claim(ClaimTypes.Role, "Cliente"),
            new Claim(SecurityClaimTypes.PrimaryRole, "Cliente"),
            new Claim(SecurityClaimTypes.IsSuperUser, bool.FalseString)
        ], AuthorizationPolicies.CustomerCookieScheme));

        bool result = AuthorizationPolicies.IsCustomerPrincipal(principal);

        Assert.That(result, Is.True);
    }

    [Test]
    public void CreateModel_PaginaSensitiva_ExigePoliticaDeSuperUsuario()
    {
        AuthorizeAttribute? attribute = typeof(CreateModel).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .OfType<AuthorizeAttribute>()
            .SingleOrDefault();

        Assert.That(attribute?.Policy, Is.EqualTo(AuthorizationPolicies.SuperUserOnly));
        Assert.That(attribute?.AuthenticationSchemes, Is.EqualTo(AuthorizationPolicies.AdminCookieScheme));
    }

    [Test]
    public void ConfigureAdminCookie_AplicaHardeningEsperado()
    {
        CookieAuthenticationOptions options = new();

        AuthorizationPolicies.ConfigureAdminCookie(options);

        Assert.That(options.Cookie.Name, Is.EqualTo(AuthorizationPolicies.AdminCookieName));
        Assert.That(options.Cookie.IsEssential, Is.True);
        Assert.That(options.Cookie.HttpOnly, Is.True);
        Assert.That(options.Cookie.SameSite, Is.EqualTo(SameSiteMode.Strict));
        Assert.That(options.Cookie.SecurePolicy, Is.EqualTo(CookieSecurePolicy.Always));
        Assert.That(options.ExpireTimeSpan, Is.EqualTo(TimeSpan.FromHours(8)));
        Assert.That(options.SlidingExpiration, Is.True);
    }

    [Test]
    public void ConfigureCustomerCookie_AplicaHardeningEsperado()
    {
        CookieAuthenticationOptions options = new();

        AuthorizationPolicies.ConfigureCustomerCookie(options);

        Assert.That(options.Cookie.Name, Is.EqualTo(AuthorizationPolicies.CustomerCookieName));
        Assert.That(options.Cookie.IsEssential, Is.True);
        Assert.That(options.Cookie.HttpOnly, Is.True);
        Assert.That(options.Cookie.SameSite, Is.EqualTo(SameSiteMode.Strict));
        Assert.That(options.Cookie.SecurePolicy, Is.EqualTo(CookieSecurePolicy.Always));
        Assert.That(options.ExpireTimeSpan, Is.EqualTo(TimeSpan.FromHours(8)));
        Assert.That(options.SlidingExpiration, Is.True);
    }

    [Test]
    public void CustomerAccountIndexModel_PaginaDeCliente_ExigePoliticaDeCliente()
    {
        AuthorizeAttribute? attribute = typeof(PlataformaECommerce.Web.Pages.Account.IndexModel).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .OfType<AuthorizeAttribute>()
            .SingleOrDefault();

        Assert.That(attribute?.Policy, Is.EqualTo(AuthorizationPolicies.CustomerOnly));
        Assert.That(attribute?.AuthenticationSchemes, Is.EqualTo(AuthorizationPolicies.CustomerCookieScheme));
    }

    [Test]
    public void IndexModel_PaginaSensitiva_ExigePoliticaDeSuperUsuario()
    {
        AuthorizeAttribute? attribute = typeof(IndexModel).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .OfType<AuthorizeAttribute>()
            .SingleOrDefault();

        Assert.That(attribute?.Policy, Is.EqualTo(AuthorizationPolicies.SuperUserOnly));
        Assert.That(attribute?.AuthenticationSchemes, Is.EqualTo(AuthorizationPolicies.AdminCookieScheme));
    }

    private static ClaimsPrincipal CreatePrincipal(string primaryRole, string[] roles, bool isSuperUser)
    {
        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, "Admin Demo"),
            new(ClaimTypes.Email, "admin@plataforma.com"),
            new(SecurityClaimTypes.PrimaryRole, primaryRole),
            new(SecurityClaimTypes.AdminArea, "Operaciones"),
            new(SecurityClaimTypes.IsSuperUser, isSuperUser.ToString())
        ];

        foreach (string role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, AuthorizationPolicies.AdminCookieScheme));
    }
}
