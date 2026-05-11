using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Tests.Web.Configuration;

[TestFixture]
internal sealed class ProductionSecretsConfigurationGuardTests
{
    [Test]
    public void Validate_DevelopmentSinSecretos_NoLanza()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();

        Assert.DoesNotThrow(() =>
            ProductionSecretsConfigurationGuard.Validate(configuration, new FakeHostEnvironment(Environments.Development)));
    }

    [Test]
    public void Validate_ProductionSinCadenaConexion_LanzaInvalidOperationException()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SigningKey"] = new string('k', 32)
            })
            .Build();

        InvalidOperationException? ex = Assert.Throws<InvalidOperationException>(() =>
            ProductionSecretsConfigurationGuard.Validate(configuration, new FakeHostEnvironment(Environments.Production)));

        Assert.That(ex!.Message, Does.Contain("ConnectionStrings"));
    }

    [Test]
    public void Validate_ProductionConJwtCorto_LanzaInvalidOperationException()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=.;Database=x;Trusted_Connection=True;",
                ["Jwt:SigningKey"] = "short"
            })
            .Build();

        InvalidOperationException? ex = Assert.Throws<InvalidOperationException>(() =>
            ProductionSecretsConfigurationGuard.Validate(configuration, new FakeHostEnvironment(Environments.Production)));

        Assert.That(ex!.Message, Does.Contain("Jwt:SigningKey"));
    }

    [Test]
    public void Validate_ProductionConWompiHabilitadoYPubTest_LanzaInvalidOperationException()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=.;Database=x;Trusted_Connection=True;",
                ["Jwt:SigningKey"] = new string('k', 32),
                ["Payments:Wompi:Enabled"] = bool.TrueString,
                ["Payments:Wompi:PublicKey"] = "pub_test_xxxxxxxx",
                ["Payments:Wompi:IntegritySecret"] = "integrity-real-length-secret-value-ok",
                ["Payments:Wompi:CheckoutBaseUrl"] = "https://checkout.wompi.co/p/",
                ["Payments:Wompi:TransactionsApiBaseUrl"] = "https://production.wompi.co/v1/transactions/"
            })
            .Build();

        InvalidOperationException? ex = Assert.Throws<InvalidOperationException>(() =>
            ProductionSecretsConfigurationGuard.Validate(configuration, new FakeHostEnvironment(Environments.Production)));

        Assert.That(ex!.Message, Does.Contain("pub_test_"));
    }

    [Test]
    public void Validate_ProductionConSecretosMinimosYWompiDeshabilitado_NoLanza()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=.;Database=x;Trusted_Connection=True;",
                ["Jwt:SigningKey"] = new string('k', 32),
                ["Payments:Wompi:Enabled"] = bool.FalseString
            })
            .Build();

        Assert.DoesNotThrow(() =>
            ProductionSecretsConfigurationGuard.Validate(configuration, new FakeHostEnvironment(Environments.Production)));
    }

    private sealed class FakeHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
