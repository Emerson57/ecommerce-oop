using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Web.Configuration;
using PlataformaECommerce.Web.Extensions.Startup;

namespace PlataformaECommerce.Tests.Web.Startup;

[TestFixture]
public class BootstrapSuperUserOptionsValidationTests
{
    [Test]
    public void AddInitializationServices_BootstrapDeshabilitado_PermiteConfiguracionVacia()
    {
        ServiceCollection services = new();
        IConfiguration configuration = BuildConfiguration(enabled: false);

        services.AddInitializationServices(configuration);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        BootstrapSuperUserOptions options = serviceProvider.GetRequiredService<IOptions<BootstrapSuperUserOptions>>().Value;

        Assert.That(options.Enabled, Is.False);
    }

    [Test]
    public void AddInitializationServices_BootstrapHabilitadoSinPassword_LanzaOptionsValidationException()
    {
        ServiceCollection services = new();
        IConfiguration configuration = BuildConfiguration(enabled: true, password: " ");

        services.AddInitializationServices(configuration);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        OptionsValidationException exception = Assert.Throws<OptionsValidationException>(() =>
            _ = serviceProvider.GetRequiredService<IOptions<BootstrapSuperUserOptions>>().Value)!;

        Assert.That(exception.Message, Does.Contain(nameof(BootstrapSuperUserOptions.Password)));
        Assert.That(exception.Message, Does.Contain(BootstrapSuperUserOptions.SectionName));
    }

    [Test]
    public void AddInitializationServices_BootstrapHabilitadoConEmailInvalido_LanzaOptionsValidationException()
    {
        ServiceCollection services = new();
        IConfiguration configuration = BuildConfiguration(enabled: true, email: "correo-invalido");

        services.AddInitializationServices(configuration);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        OptionsValidationException exception = Assert.Throws<OptionsValidationException>(() =>
            _ = serviceProvider.GetRequiredService<IOptions<BootstrapSuperUserOptions>>().Value)!;

        Assert.That(exception.Message, Does.Contain(nameof(BootstrapSuperUserOptions.Email)));
        Assert.That(exception.Message, Does.Contain(BootstrapSuperUserOptions.SectionName));
    }

    private static IConfiguration BuildConfiguration(
        bool enabled,
        string name = "Super Admin",
        string tenantId = "tenant-root",
        string email = "root@plataforma.com",
        string password = "Password#2026",
        string area = "Plataforma")
    {
        Dictionary<string, string?> values = new(StringComparer.Ordinal)
        {
            [$"{BootstrapSuperUserOptions.SectionName}:Enabled"] = enabled.ToString(),
            [$"{BootstrapSuperUserOptions.SectionName}:Name"] = name,
            [$"{BootstrapSuperUserOptions.SectionName}:TenantId"] = tenantId,
            [$"{BootstrapSuperUserOptions.SectionName}:Email"] = email,
            [$"{BootstrapSuperUserOptions.SectionName}:Password"] = password,
            [$"{BootstrapSuperUserOptions.SectionName}:Area"] = area
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
