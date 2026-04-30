using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using PlataformaECommerce.Application.Interfaces.Services.Auth;
using PlataformaECommerce.Infrastructure.Configurations;
using PlataformaECommerce.Infrastructure.DependencyInjection;

namespace PlataformaECommerce.Tests.Infrastructure.DependencyInjection;

[TestFixture]
public class InfrastructureServiceRegistrationTests
{
    [Test]
    public void AddInfrastructure_DevelopmentSinClaveJwtPersistida_ConfiguraClaveTemporalValida()
    {
        ServiceCollection services = new();
        IConfiguration configuration = BuildConfiguration(signingKey: string.Empty);
        FakeHostEnvironment hostEnvironment = new(Environments.Development);

        services.AddInfrastructure(configuration, hostEnvironment);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        DataProtectionKeyManagementSettings dataProtectionSettings = serviceProvider.GetRequiredService<IOptions<DataProtectionKeyManagementSettings>>().Value;
        DataProtectionOptions runtimeDataProtectionOptions = serviceProvider.GetRequiredService<IOptions<DataProtectionOptions>>().Value;
        KeyManagementOptions keyManagementOptions = serviceProvider.GetRequiredService<IOptions<KeyManagementOptions>>().Value;
        JwtSettings settings = serviceProvider.GetRequiredService<IOptions<JwtSettings>>().Value;
        ITokenService tokenService = serviceProvider.GetRequiredService<ITokenService>();

        Assert.That(dataProtectionSettings.ApplicationName, Is.EqualTo("PlataformaECommerce.Tests"));
        Assert.That(runtimeDataProtectionOptions.ApplicationDiscriminator, Is.EqualTo("PlataformaECommerce.Tests"));
        Assert.That(keyManagementOptions.NewKeyLifetime, Is.EqualTo(TimeSpan.FromDays(30)));
        Assert.That(settings.SigningKey, Is.Not.Null.And.Not.Empty);
        Assert.That(settings.SigningKey.Length, Is.GreaterThanOrEqualTo(32));
        Assert.That(tokenService, Is.Not.Null);
    }

    [Test]
    public void AddInfrastructure_ProductionSinClaveJwtPersistida_LanzaInvalidOperationException()
    {
        ServiceCollection services = new();
        IConfiguration configuration = BuildConfiguration(signingKey: string.Empty);
        FakeHostEnvironment hostEnvironment = new(Environments.Production);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddInfrastructure(configuration, hostEnvironment))!;

        Assert.That(exception.Message, Does.Contain("Jwt:SigningKey"));
    }

    [Test]
    public void AddInfrastructure_ProductionSinPublicKeyWompi_LanzaOptionsValidationException()
    {
        ServiceCollection services = new();
        IConfiguration configuration = BuildConfiguration(signingKey: new string('x', 32), wompiPublicKey: string.Empty);
        FakeHostEnvironment hostEnvironment = new(Environments.Production);

        services.AddInfrastructure(configuration, hostEnvironment);

        OptionsValidationException exception = Assert.Throws<OptionsValidationException>(() =>
        {
            using ServiceProvider provider = services.BuildServiceProvider(validateScopes: true);
            _ = provider.GetRequiredService<IOptions<WompiPaymentGatewaySettings>>().Value;
        })!;

        Assert.That(exception.Message, Does.Contain("Payments:Wompi"));
        Assert.That(exception.Message, Does.Contain("PublicKey"));
    }

    [Test]
    public void AddInfrastructure_ProductionSinPasswordSmtp_LanzaOptionsValidationException()
    {
        ServiceCollection services = new();
        IConfiguration configuration = BuildConfiguration(signingKey: new string('x', 32), smtpPassword: string.Empty);
        FakeHostEnvironment hostEnvironment = new(Environments.Production);

        services.AddInfrastructure(configuration, hostEnvironment);

        OptionsValidationException exception = Assert.Throws<OptionsValidationException>(() =>
        {
            using ServiceProvider provider = services.BuildServiceProvider(validateScopes: true);
            _ = provider.GetRequiredService<IOptions<SmtpEmailSettings>>().Value;
        })!;

        Assert.That(exception.Message, Does.Contain("Notifications:Smtp"));
        Assert.That(exception.Message, Does.Contain("Password"));
    }

    private static IConfiguration BuildConfiguration(
        string signingKey,
        string wompiPublicKey = "pub_test_123",
        string wompiIntegritySecret = "int_test_456",
        string smtpHost = "smtp.integration.internal",
        string smtpUserName = "smtp-user",
        string smtpPassword = "smtp-password",
        string smtpFromAddress = "noreply@integration.internal")
    {
        Dictionary<string, string?> values = new(StringComparer.Ordinal)
        {
            ["ConnectionStrings:DefaultConnection"] = "Server=tcp:sql.integration.internal,1433;Database=PlataformaECommerceTests;Encrypt=True;TrustServerCertificate=True;",
            ["DataProtection:ApplicationName"] = "PlataformaECommerce.Tests",
            ["DataProtection:KeyLifetimeDays"] = "30",
            ["Jwt:Issuer"] = "PlataformaECommerce.Web",
            ["Jwt:Audience"] = "PlataformaECommerce.Clients",
            ["Jwt:SigningKey"] = signingKey,
            ["Jwt:AccessTokenExpirationMinutes"] = "60",
            ["Jwt:RefreshTokenExpirationDays"] = "7",
            ["Jwt:RequireHttpsMetadata"] = bool.TrueString,
            ["Payments:Wompi:Enabled"] = bool.FalseString,
            ["Payments:Wompi:PublicKey"] = wompiPublicKey,
            ["Payments:Wompi:IntegritySecret"] = wompiIntegritySecret,
            ["Payments:Wompi:CheckoutBaseUrl"] = "https://checkout.wompi.co/p/",
            ["Payments:Wompi:TransactionsApiBaseUrl"] = "https://production.wompi.co/v1/transactions/",
            ["Notifications:Smtp:Enabled"] = bool.FalseString,
            ["Notifications:Smtp:Host"] = smtpHost,
            ["Notifications:Smtp:UserName"] = smtpUserName,
            ["Notifications:Smtp:Password"] = smtpPassword,
            ["Notifications:Smtp:FromAddress"] = smtpFromAddress
        };

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
