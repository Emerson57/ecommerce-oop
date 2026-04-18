using System.Security.Claims;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Application.Common.Security;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.Enums;
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

    [Test]
    public void GetPrincipalFromAccessToken_SuperUsuario_PropagaRolPrimarioYRolesEfectivos()
    {
        JwtTokenService service = CreateService();
        Administrador superUser = CreateSuperUser();
        string token = service.GenerateAccessToken(superUser);

        ClaimsPrincipal? result = service.GetPrincipalFromAccessToken(token);

        Assert.That(result?.FindFirstValue(SecurityClaimTypes.PrimaryRole), Is.EqualTo(RolUsuario.SuperUsuario.ToString()));
        Assert.That(result?.IsInRole(RolUsuario.SuperUsuario.ToString()), Is.True);
        Assert.That(result?.IsInRole(RolUsuario.Administrador.ToString()), Is.True);
        Assert.That(result?.FindFirstValue(SecurityClaimTypes.IsSuperUser), Is.EqualTo(bool.TrueString));
        Assert.That(result?.FindFirstValue(SecurityClaimTypes.TenantId), Is.EqualTo("tenant-security"));
    }

    private static JwtTokenService CreateService()
    {
        JwtSettings settings = new()
        {
            Issuer = "PlataformaECommerce.Tests",
            Audience = "PlataformaECommerce.Tests.Clients",
            // Generate a test-only signing key at runtime to avoid committing secrets
            SigningKey = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(48)),
            AccessTokenExpirationMinutes = 30,
            RefreshTokenExpirationDays = 7,
            RequireHttpsMetadata = false
        };

        return new JwtTokenService(Options.Create(settings), new FakeTenantContextAccessor("tenant-security"));
    }

    private sealed class FakeTenantContextAccessor(string tenantId) : ITenantContextAccessor
    {
        public string TenantId { get; } = tenantId;
        public bool IsAvailable => !string.IsNullOrWhiteSpace(TenantId);
    }

    private static Cliente CreateCustomer()
    {
        return new Cliente(
            "Cliente Seguridad",
            new Email("cliente.seguridad@plataforma.com"),
            "hash-seguro-demo-2026");
    }

    private static Administrador CreateSuperUser()
    {
        return new Administrador(
            "Root Seguridad",
            new Email("root.seguridad@plataforma.com"),
            "hash-seguro-root-2026",
            "Plataforma",
            RolUsuario.SuperUsuario);
    }
}
