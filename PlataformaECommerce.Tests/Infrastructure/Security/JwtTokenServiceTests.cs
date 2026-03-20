using System.Security.Claims;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.ValueObjects;
using PlataformaECommerce.Infrastructure.Configurations;
using PlataformaECommerce.Infrastructure.Services.Auth;

namespace PlataformaECommerce.Tests.Infrastructure.Security;

[TestFixture]
public class JwtTokenServiceTests
{
    [Test]
    public void GetPrincipalFromAccessToken_TokenEmitidoPorElServicio_RetornaPrincipal()
    {
        JwtTokenService service = CreateService();
        Cliente user = CreateCustomer();
        string token = service.GenerateAccessToken(user);

        ClaimsPrincipal? result = service.GetPrincipalFromAccessToken(token);

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void GetAccessTokenExpirationUtc_TokenEmitidoPorElServicio_RetornaFechaFutura()
    {
        JwtTokenService service = CreateService();
        Cliente user = CreateCustomer();
        string token = service.GenerateAccessToken(user);

        DateTime result = service.GetAccessTokenExpirationUtc(token);

        Assert.That(result, Is.GreaterThan(DateTime.UtcNow));
    }

    private static JwtTokenService CreateService()
    {
        JwtSettings settings = new()
        {
            Issuer = "PlataformaECommerce.Tests",
            Audience = "PlataformaECommerce.Tests.Clients",
            SigningKey = "PlataformaECommerce.Tests.Signing.Key.2026!",
            AccessTokenExpirationMinutes = 30,
            RefreshTokenExpirationDays = 7,
            RequireHttpsMetadata = false
        };

        return new JwtTokenService(Options.Create(settings));
    }

    private static Cliente CreateCustomer()
    {
        return new Cliente(
            "Cliente Seguridad",
            new Email("cliente.seguridad@plataforma.com"),
            "hash-seguro-demo-2026");
    }
}
