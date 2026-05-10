using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Tests.Web.Configuration;

[TestFixture]
internal sealed class AdministratorCreationConfigurationGuardTests
{
    [Test]
    public void Validate_ProductionConCreacionUiHabilitada_LanzaInvalidOperationException()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Backoffice:Users:EnableAdministratorCreationUi"] = bool.TrueString
            })
            .Build();
        IHostEnvironment environment = new FakeHostEnvironment(Environments.Production);

        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() =>
            AdministratorCreationConfigurationGuard.Validate(configuration, environment));

        Assert.That(exception!.Message, Does.Contain("EnableAdministratorCreationUi"));
    }

    [Test]
    public void Validate_StagingConCreacionUiHabilitada_LanzaInvalidOperationException()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Backoffice:Users:EnableAdministratorCreationUi"] = "true"
            })
            .Build();
        IHostEnvironment environment = new FakeHostEnvironment(Environments.Staging);

        Assert.Throws<InvalidOperationException>(() =>
            AdministratorCreationConfigurationGuard.Validate(configuration, environment));
    }

    [Test]
    public void Validate_ProductionConCreacionUiDeshabilitada_NoLanza()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Backoffice:Users:EnableAdministratorCreationUi"] = bool.FalseString
            })
            .Build();
        IHostEnvironment environment = new FakeHostEnvironment(Environments.Production);

        Assert.DoesNotThrow(() => AdministratorCreationConfigurationGuard.Validate(configuration, environment));
    }

    [Test]
    public void Validate_DevelopmentConCreacionUiHabilitada_NoLanza()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Backoffice:Users:EnableAdministratorCreationUi"] = bool.TrueString
            })
            .Build();
        IHostEnvironment environment = new FakeHostEnvironment(Environments.Development);

        Assert.DoesNotThrow(() => AdministratorCreationConfigurationGuard.Validate(configuration, environment));
    }

    private sealed class FakeHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
