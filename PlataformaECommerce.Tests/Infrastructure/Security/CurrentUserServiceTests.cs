using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PlataformaECommerce.Application.Common.Security;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Infrastructure.Services.Common;

namespace PlataformaECommerce.Tests.Infrastructure.Security;

[TestFixture]
public class CurrentUserServiceTests
{
    [Test]
    public void UserId_UsuarioAutenticado_RetornaIdentificadorActual()
    {
        Guid userId = Guid.NewGuid();
        DefaultHttpContext httpContext = new();
        httpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) },
                authenticationType: "Bearer"));

        HttpContextAccessor accessor = new() { HttpContext = httpContext };
        CurrentUserService service = new(accessor);

        Assert.That(service.UserId, Is.EqualTo(userId));
    }

    [Test]
    public void Email_UsuarioAutenticado_RetornaCorreoActual()
    {
        DefaultHttpContext httpContext = new();
        httpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Email, "cliente@plataforma.com") },
                authenticationType: "Bearer"));

        HttpContextAccessor accessor = new() { HttpContext = httpContext };
        CurrentUserService service = new(accessor);

        Assert.That(service.Email, Is.EqualTo("cliente@plataforma.com"));
    }

    [Test]
    public void Role_SuperUsuarioConRolPrimario_RetornaSuperUsuario()
    {
        DefaultHttpContext httpContext = new();
        httpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(
                [
                    new Claim(SecurityClaimTypes.PrimaryRole, RolUsuario.SuperUsuario.ToString()),
                    new Claim(ClaimTypes.Role, RolUsuario.SuperUsuario.ToString()),
                    new Claim(ClaimTypes.Role, RolUsuario.Administrador.ToString())
                ],
                authenticationType: "Bearer"));

        HttpContextAccessor accessor = new() { HttpContext = httpContext };
        CurrentUserService service = new(accessor);

        Assert.That(service.Role, Is.EqualTo(RolUsuario.SuperUsuario.ToString()));
    }
}
