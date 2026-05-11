using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Tests.Web.Configuration;

[TestFixture]
internal sealed class EntityFrameworkProductionGuardTests
{
    [Test]
    public void Validate_DevelopmentConMigracionAutomaticaHabilitada_NoLanza()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{EntityFrameworkProductionGuard.SectionName}:ApplyEfMigrationsOnStartup"] = bool.TrueString
            })
            .Build();

        Assert.DoesNotThrow(() =>
            EntityFrameworkProductionGuard.Validate(configuration, new FakeHostEnvironment(Environments.Development)));
    }

    [Test]
    public void Validate_ProductionConMigracionAutomaticaHabilitada_LanzaInvalidOperationException()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{EntityFrameworkProductionGuard.SectionName}:ApplyEfMigrationsOnStartup"] = bool.TrueString
            })
            .Build();

        InvalidOperationException? ex = Assert.Throws<InvalidOperationException>(() =>
            EntityFrameworkProductionGuard.Validate(configuration, new FakeHostEnvironment(Environments.Production)));

        Assert.That(ex!.Message, Does.Contain("ApplyEfMigrationsOnStartup"));
    }

    [Test]
    public void Validate_ProductionSinOpcionPeligrosa_NoLanza()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();

        Assert.DoesNotThrow(() =>
            EntityFrameworkProductionGuard.Validate(configuration, new FakeHostEnvironment(Environments.Production)));
    }

    private sealed class FakeHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
