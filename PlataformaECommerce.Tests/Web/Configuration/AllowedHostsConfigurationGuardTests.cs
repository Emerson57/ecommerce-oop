using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Tests.Web.Configuration;

[TestFixture]
internal sealed class AllowedHostsConfigurationGuardTests
{
    [Test]
    public void Validate_ProductionSinAllowedHosts_LanzaInvalidOperationException()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder().Build();
        IHostEnvironment environment = new FakeHostEnvironment(Environments.Production);

        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() =>
            AllowedHostsConfigurationGuard.Validate(configuration, environment));

        Assert.That(exception!.Message, Does.Contain("AllowedHosts"));
    }

    [Test]
    public void Validate_ProductionConWildcardAsterisco_LanzaInvalidOperationException()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["AllowedHosts"] = "*" })
            .Build();
        IHostEnvironment environment = new FakeHostEnvironment(Environments.Production);

        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() =>
            AllowedHostsConfigurationGuard.Validate(configuration, environment));

        Assert.That(exception!.Message, Does.Contain("*"));
        Assert.That(exception!.Message, Does.Contain("Production"));
    }

    [Test]
    public void Validate_ProductionConListaExplicita_NoLanza()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["AllowedHosts"] = "midominio.com;www.midominio.com" })
            .Build();
        IHostEnvironment environment = new FakeHostEnvironment(Environments.Production);

        Assert.DoesNotThrow(() => AllowedHostsConfigurationGuard.Validate(configuration, environment));
    }

    [Test]
    public void Validate_DevelopmentConWildcard_NoLanza()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["AllowedHosts"] = "*" })
            .Build();
        IHostEnvironment environment = new FakeHostEnvironment(Environments.Development);

        Assert.DoesNotThrow(() => AllowedHostsConfigurationGuard.Validate(configuration, environment));
    }

    [Test]
    public void ResolveEffectiveAllowedHosts_FormatoSeparadoPorPuntoYComa_DevuelveEntradas()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["AllowedHosts"] = " a.com ; b.com " })
            .Build();

        IReadOnlyList<string> hosts = AllowedHostsConfigurationGuard.ResolveEffectiveAllowedHosts(configuration);

        Assert.That(hosts, Is.EqualTo(new[] { "a.com", "b.com" }).AsCollection);
    }

    [Test]
    public void ResolveEffectiveAllowedHosts_ArregloJson_DevuelveEntradas()
    {
        string json = /*lang=json,strict*/ """
            {"AllowedHosts":["x.test","y.test"]}
            """;
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)))
            .Build();

        IReadOnlyList<string> hosts = AllowedHostsConfigurationGuard.ResolveEffectiveAllowedHosts(configuration);

        Assert.That(hosts, Is.EqualTo(new[] { "x.test", "y.test" }).AsCollection);
    }

    private sealed class FakeHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
