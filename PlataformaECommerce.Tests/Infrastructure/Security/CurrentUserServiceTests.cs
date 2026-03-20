using System.Security.Claims;
using Microsoft.AspNetCore.Http;
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
}
