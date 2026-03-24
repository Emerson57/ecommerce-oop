using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
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
        JwtSettings settings = serviceProvider.GetRequiredService<IOptions<JwtSettings>>().Value;
        ITokenService tokenService = serviceProvider.GetRequiredService<ITokenService>();

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

    private static IConfiguration BuildConfiguration(string signingKey)
    {
        Dictionary<string, string?> values = new(StringComparer.Ordinal)
        {
            ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=PlataformaECommerceTests;Trusted_Connection=True;TrustServerCertificate=True;",
            ["MongoDb:ConnectionString"] = "mongodb://localhost:27017",
            ["MongoDb:DatabaseName"] = "PlataformaECommerceAuditDb",
            ["MongoDb:AuditCollectionName"] = "audit_trail",
            ["MongoDb:EnsureIndexesOnStartup"] = bool.TrueString,
            ["Jwt:Issuer"] = "PlataformaECommerce.Web",
            ["Jwt:Audience"] = "PlataformaECommerce.Clients",
            ["Jwt:SigningKey"] = signingKey,
            ["Jwt:AccessTokenExpirationMinutes"] = "60",
            ["Jwt:RefreshTokenExpirationDays"] = "7",
            ["Jwt:RequireHttpsMetadata"] = bool.TrueString
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
