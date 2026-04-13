using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Web.Configuration;
using PlataformaECommerce.Web.Extensions.Startup;
using ForwardedHeadersOptions = Microsoft.AspNetCore.Builder.ForwardedHeadersOptions;

namespace PlataformaECommerce.Tests.Web.Startup;

[TestFixture]
public class ForwardedHeadersStartupExtensionsTests
{
    [Test]
    public void AddForwardedHeadersSupport_DevelopmentConRedesLoopback_ConfiguraOpcionesSeguras()
    {
        ServiceCollection services = new();
        IConfiguration configuration = BuildConfiguration(enabled: true, trustedNetworks: ["127.0.0.0/8", "::1/128"]);
        FakeHostEnvironment hostEnvironment = new(Environments.Development);

        services.AddLogging();
        services.AddForwardedHeadersSupport(configuration, hostEnvironment);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        ForwardedHeadersSecurityOptions securityOptions = serviceProvider.GetRequiredService<IOptions<ForwardedHeadersSecurityOptions>>().Value;
        ForwardedHeadersOptions forwardedHeadersOptions = serviceProvider.GetRequiredService<IOptions<ForwardedHeadersOptions>>().Value;

        Assert.That(securityOptions.Enabled, Is.True);
        Assert.That(forwardedHeadersOptions.ForwardedHeaders, Is.EqualTo(ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto));
        Assert.That(forwardedHeadersOptions.KnownNetworks, Has.Count.EqualTo(2));
        Assert.That(forwardedHeadersOptions.ForwardLimit, Is.EqualTo(1));
    }

    [Test]
    public void AddForwardedHeadersSupport_ProductionHabilitadoSinConfianzaExplicita_LanzaOptionsValidationException()
    {
        ServiceCollection services = new();
        IConfiguration configuration = BuildConfiguration(enabled: true);
        FakeHostEnvironment hostEnvironment = new(Environments.Production);

        services.AddLogging();
        services.AddForwardedHeadersSupport(configuration, hostEnvironment);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        OptionsValidationException exception = Assert.Throws<OptionsValidationException>(() =>
            _ = serviceProvider.GetRequiredService<IOptions<ForwardedHeadersSecurityOptions>>().Value)!;

        Assert.That(exception.Message, Does.Contain("ForwardedHeadersSecurity"));
    }

    [Test]
    public void AddForwardedHeadersSupport_ProductionConProxyConfiable_ConfiguraKnownProxies()
    {
        ServiceCollection services = new();
        IConfiguration configuration = BuildConfiguration(
            enabled: true,
            trustedProxies: ["10.10.0.5"],
            trustForwardedHost: true,
            allowedHosts: ["admin.plataforma.com", "shop.plataforma.com"]);
        FakeHostEnvironment hostEnvironment = new(Environments.Production);

        services.AddLogging();
        services.AddForwardedHeadersSupport(configuration, hostEnvironment);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        ForwardedHeadersOptions forwardedHeadersOptions = serviceProvider.GetRequiredService<IOptions<ForwardedHeadersOptions>>().Value;

        Assert.That(forwardedHeadersOptions.KnownProxies.Select(proxy => proxy.ToString()), Is.EquivalentTo(new[] { "10.10.0.5" }));
        Assert.That(forwardedHeadersOptions.KnownNetworks, Is.Empty);
        Assert.That(forwardedHeadersOptions.RequireHeaderSymmetry, Is.True);
        Assert.That(forwardedHeadersOptions.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedHost), Is.True);
        Assert.That(forwardedHeadersOptions.AllowedHosts, Is.EquivalentTo(new[] { "admin.plataforma.com", "shop.plataforma.com" }));
    }

    [Test]
    public void AddForwardedHeadersSupport_ProductionConForwardedHostSinAllowedHosts_LanzaOptionsValidationException()
    {
        ServiceCollection services = new();
        IConfiguration configuration = BuildConfiguration(enabled: true, trustedProxies: ["10.10.0.5"], trustForwardedHost: true);
        FakeHostEnvironment hostEnvironment = new(Environments.Production);

        services.AddLogging();
        services.AddForwardedHeadersSupport(configuration, hostEnvironment);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        OptionsValidationException exception = Assert.Throws<OptionsValidationException>(() =>
            _ = serviceProvider.GetRequiredService<IOptions<ForwardedHeadersSecurityOptions>>().Value)!;

        Assert.That(exception.Message, Does.Contain("AllowedHosts"));
    }

    private static IConfiguration BuildConfiguration(
        bool enabled,
        string[]? trustedProxies = null,
        string[]? trustedNetworks = null,
        string[]? allowedHosts = null,
        int forwardLimit = 1,
        bool requireHeaderSymmetry = true,
        bool trustForwardedHost = false)
    {
        Dictionary<string, string?> values = new(StringComparer.Ordinal)
        {
            [$"{ForwardedHeadersSecurityOptions.SectionName}:Enabled"] = enabled.ToString(),
            [$"{ForwardedHeadersSecurityOptions.SectionName}:ForwardLimit"] = forwardLimit.ToString(),
            [$"{ForwardedHeadersSecurityOptions.SectionName}:RequireHeaderSymmetry"] = requireHeaderSymmetry.ToString(),
            [$"{ForwardedHeadersSecurityOptions.SectionName}:TrustForwardedHost"] = trustForwardedHost.ToString()
        };

        if (trustedProxies is not null)
        {
            for (int index = 0; index < trustedProxies.Length; index++)
            {
                values[$"{ForwardedHeadersSecurityOptions.SectionName}:TrustedProxies:{index}"] = trustedProxies[index];
            }
        }

        if (trustedNetworks is not null)
        {
            for (int index = 0; index < trustedNetworks.Length; index++)
            {
                values[$"{ForwardedHeadersSecurityOptions.SectionName}:TrustedNetworks:{index}"] = trustedNetworks[index];
            }
        }

        if (allowedHosts is not null)
        {
            for (int index = 0; index < allowedHosts.Length; index++)
            {
                values[$"{ForwardedHeadersSecurityOptions.SectionName}:AllowedHosts:{index}"] = allowedHosts[index];
            }
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    private sealed class FakeHostEnvironment : IHostEnvironment
    {
        public FakeHostEnvironment(string environmentName)
        {
            EnvironmentName = environmentName;
            ApplicationName = "PlataformaECommerce.Web";
            ContentRootPath = AppContext.BaseDirectory;
        }

        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; }
        public string ContentRootPath { get; set; }
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}
