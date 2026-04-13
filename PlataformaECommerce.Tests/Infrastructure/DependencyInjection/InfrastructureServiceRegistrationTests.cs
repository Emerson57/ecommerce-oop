using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using PlataformaECommerce.Application.Interfaces.Repositories.Audit;
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
    public async Task AddInfrastructure_DevelopmentConMongoDeshabilitado_RegistraAuditoriaNoOperativa()
    {
        ServiceCollection services = new();
        IConfiguration configuration = BuildConfiguration(signingKey: string.Empty, mongoEnabled: false, mongoConnectionString: null);
        FakeHostEnvironment hostEnvironment = new(Environments.Development);

        services.AddLogging();
        services.AddInfrastructure(configuration, hostEnvironment);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IAuditRepository auditRepository = serviceProvider.GetRequiredService<IAuditRepository>();

        AuditSearchResult result = await auditRepository.SearchAsync(new AuditSearchFilter());

        Assert.That(result.TotalCount, Is.Zero);
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
    public void AddInfrastructure_ProductionConMongoDeshabilitado_LanzaInvalidOperationException()
    {
        ServiceCollection services = new();
        IConfiguration configuration = BuildConfiguration(signingKey: new string('x', 32), mongoEnabled: false, mongoConnectionString: null);
        FakeHostEnvironment hostEnvironment = new(Environments.Production);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddInfrastructure(configuration, hostEnvironment))!;

        Assert.That(exception.Message, Does.Contain("auditoría MongoDB solo puede deshabilitarse en Development"));
    }

    private static IConfiguration BuildConfiguration(string signingKey, bool mongoEnabled = true, string? mongoConnectionString = "mongodb://mongo.integration.internal:27017")
    {
        Dictionary<string, string?> values = new(StringComparer.Ordinal)
        {
            ["ConnectionStrings:DefaultConnection"] = "Server=tcp:sql.integration.internal,1433;Database=PlataformaECommerceTests;Encrypt=True;TrustServerCertificate=True;",
            ["DataProtection:ApplicationName"] = "PlataformaECommerce.Tests",
            ["DataProtection:KeyLifetimeDays"] = "30",
            ["MongoDb:Enabled"] = mongoEnabled.ToString(),
            ["MongoDb:ConnectionString"] = mongoConnectionString,
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
