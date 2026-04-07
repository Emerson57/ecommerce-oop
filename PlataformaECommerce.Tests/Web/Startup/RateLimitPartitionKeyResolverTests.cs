using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Web.Extensions.Startup;

namespace PlataformaECommerce.Tests.Web.Startup;

[TestFixture]
public class RateLimitPartitionKeyResolverTests
{
    [Test]
    public void Resolve_UsuarioAutenticadoYTenantDisponible_ConstruyeClaveEstablePorPoliticaTenantActorYSuperficie()
    {
        RateLimitPartitionKeyResolver resolver = new(new FakeTenantContextAccessor("novashop-default", isAvailable: true));
        DefaultHttpContext httpContext = new();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "user-123"),
            new Claim(ClaimTypes.Name, "admin@novashop.example")
        ], authenticationType: "Cookies"));
        httpContext.Request.RouteValues["controller"] = "AdminProducts";

        string partitionKey = resolver.Resolve(httpContext, "administration-api");

        Assert.That(partitionKey, Is.EqualTo("policy:administration-api|tenant:novashop-default|actor:user:user-123|surface:api"));
    }

    [Test]
    public void Resolve_UsuarioAnonimo_NormalizaIpYSuperficieDePagina()
    {
        RateLimitPartitionKeyResolver resolver = new(new FakeTenantContextAccessor(null, isAvailable: false));
        DefaultHttpContext httpContext = new();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("::ffff:10.0.0.24");
        httpContext.Request.RouteValues["page"] = "/Auth/Login";

        string partitionKey = resolver.Resolve(httpContext, "auth-flow");

        Assert.That(partitionKey, Is.EqualTo("policy:auth-flow|tenant:default|actor:ip:10.0.0.24|surface:page"));
    }

    [Test]
    public void Resolve_SinMetadatosDeEndpoint_UsaSuperficieGenerica()
    {
        RateLimitPartitionKeyResolver resolver = new(new FakeTenantContextAccessor("tenant-a", isAvailable: true));
        DefaultHttpContext httpContext = new();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("192.168.10.7");
        httpContext.Request.Path = "/health/ready";

        string partitionKey = resolver.Resolve(httpContext, "sensitive-endpoints");

        Assert.That(partitionKey, Is.EqualTo("policy:sensitive-endpoints|tenant:tenant-a|actor:ip:192.168.10.7|surface:endpoint"));
    }

    private sealed class FakeTenantContextAccessor(string? tenantId, bool isAvailable) : ITenantContextAccessor
    {
        public string TenantId => tenantId ?? string.Empty;

        public bool IsAvailable => isAvailable;
    }
}
