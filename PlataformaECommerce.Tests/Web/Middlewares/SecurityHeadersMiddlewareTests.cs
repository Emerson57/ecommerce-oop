using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Web.Configuration;
using PlataformaECommerce.Web.Middlewares;

namespace PlataformaECommerce.Tests.Web.Middlewares;

[TestFixture]
public class SecurityHeadersMiddlewareTests
{
    [Test]
    public async Task InvokeAsync_AgregaHeadersDefensivosEsperados()
    {
        SecurityHeadersMiddleware middleware = new(
            context => context.Response.WriteAsync("ok"),
            Options.Create(new WebSecurityHeadersOptions()),
            new FakeWebHostEnvironment("Production"));
        DefaultHttpContext httpContext = new();

        await middleware.InvokeAsync(httpContext);

        Assert.That(httpContext.Response.Headers["X-Content-Type-Options"].ToString(), Is.EqualTo("nosniff"));
        Assert.That(httpContext.Response.Headers["X-Frame-Options"].ToString(), Is.EqualTo("DENY"));
        Assert.That(httpContext.Response.Headers["Content-Security-Policy"].ToString(), Does.Contain("frame-ancestors 'none'"));
    }

    [Test]
    public async Task InvokeAsync_Produccion_IncluyeUpgradeInsecureRequestsEnCsp()
    {
        SecurityHeadersMiddleware middleware = new(
            context => context.Response.WriteAsync("ok"),
            Options.Create(new WebSecurityHeadersOptions
            {
                ContentSecurityPolicy = "default-src 'self'",
                IncludeUpgradeInsecureRequests = true
            }),
            new FakeWebHostEnvironment("Production"));
        DefaultHttpContext httpContext = new();

        await middleware.InvokeAsync(httpContext);

        Assert.That(httpContext.Response.Headers["Content-Security-Policy"].ToString(), Does.Contain("upgrade-insecure-requests"));
    }

    private sealed class FakeWebHostEnvironment(string environmentName) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "PlataformaECommerce.Web";
        public IFileProvider WebRootFileProvider { get; set; } = null!;
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = environmentName;
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}
