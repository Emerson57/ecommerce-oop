using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Tests.Web.Configuration;

[TestFixture]
internal sealed class SaaSPlatformProductionGuardTests
{
    [Test]
    public void Validate_ProductionConTenantDemo_LanzaInvalidOperationException()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddJsonStream(OpenEmbeddedJson("""{"SaaS":{"ActiveTenantId":"main-store","ResolveTenantFromHost":true,"Tenants":[{"TenantId":"novashop-default","Enabled":true,"DisplayName":"X","StorefrontName":"X","BackofficeName":"X","StorefrontTagline":"X","LegalCompanyName":"X","SupportEmail":"soporte@midominio.com","SupportPhone":"1","SupportHours":"h","SupportSla":"s","PrimaryColor":"#111","AccentColor":"#222","AdminSidebarStartColor":"#333","AdminSidebarEndColor":"#444","LogoGlyph":"N","Currency":"COP","Country":"CO","Hostnames":["midominio.com"],"Provisioning":{"BootstrapSuperUserEmail":"admin@midominio.com"}}]}}"""))
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ClientExperience:ClientId"] = "main-store" })
            .Build();

        InvalidOperationException? ex = Assert.Throws<InvalidOperationException>(() =>
            SaaSPlatformProductionGuard.Validate(configuration, new FakeHostEnvironment(Environments.Production)));

        Assert.That(ex!.Message, Does.Contain("novashop-default").Or.Contain("demostración"));
    }

    [Test]
    public void Validate_ProductionConConfiguracionPlaceholderValida_NoLanza()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(FindWebProjectDirectory(), "appsettings.SaaS.Production.json"), optional: false)
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ClientExperience:ClientId"] = "main-store" })
            .Build();

        Assert.DoesNotThrow(() =>
            SaaSPlatformProductionGuard.Validate(configuration, new FakeHostEnvironment(Environments.Production)));
    }

    [Test]
    public void Validate_DevelopmentConTenantDemo_NoLanza()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddJsonStream(OpenEmbeddedJson("""{"SaaS":{"ActiveTenantId":"novashop-default","ResolveTenantFromHost":false,"Tenants":[{"TenantId":"novashop-default","Enabled":true,"DisplayName":"X","StorefrontName":"X","BackofficeName":"X","StorefrontTagline":"X","LegalCompanyName":"X","SupportEmail":"support@novashop.example","SupportPhone":"1","SupportHours":"h","SupportSla":"s","PrimaryColor":"#111","AccentColor":"#222","AdminSidebarStartColor":"#333","AdminSidebarEndColor":"#444","LogoGlyph":"N","Currency":"COP","Country":"CO","Hostnames":["novashop.local"],"Provisioning":{"BootstrapSuperUserEmail":"root@novashop.example"}}]}}"""))
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ClientExperience:ClientId"] = "novashop-default" })
            .Build();

        Assert.DoesNotThrow(() =>
            SaaSPlatformProductionGuard.Validate(configuration, new FakeHostEnvironment(Environments.Development)));
    }

    private static Stream OpenEmbeddedJson(string json)
    {
        return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
    }

    private static string FindWebProjectDirectory()
    {
        string dir = AppContext.BaseDirectory;
        for (int i = 0; i < 12; i++)
        {
            string candidate = Path.Combine(dir, "PlataformaECommerce.Web");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            dir = Path.GetFullPath(Path.Combine(dir, ".."));
        }

        throw new DirectoryNotFoundException("No se encontró el directorio PlataformaECommerce.Web desde el ensamblado de pruebas.");
    }

    private sealed class FakeHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
