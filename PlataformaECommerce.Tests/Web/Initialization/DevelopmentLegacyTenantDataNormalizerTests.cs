using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Infrastructure.Persistence.Context;
using PlataformaECommerce.Web.Initialization;

namespace PlataformaECommerce.Tests.Web.Initialization;

[TestFixture]
public sealed class DevelopmentLegacyTenantDataNormalizerTests
{
    [TestCaseSource(nameof(DisabledExecutionConfigurations))]
    public async Task NormalizeAsync_CuandoFlagNoExisteOEstaEnFalse_NoIntentaEjecutarInspeccion(IDictionary<string, string?> configurationValues)
    {
        await using ECommerceDbContext dbContext = CreateContext();
        DevelopmentLegacyTenantDataNormalizer normalizer = new(
            dbContext,
            new FakeTenantContextAccessor("tenant-dev"),
            new ThrowingHostEnvironment(),
            BuildConfiguration(configurationValues),
            NullLogger<DevelopmentLegacyTenantDataNormalizer>.Instance);

        Assert.DoesNotThrowAsync(async () => await normalizer.NormalizeAsync());
    }

    [Test]
    public async Task NormalizeAsync_CuandoFlagTrueYDevelopment_PuedeEjecutarse()
    {
        await using ECommerceDbContext dbContext = CreateContext();
        DevelopmentLegacyTenantDataNormalizer normalizer = new(
            dbContext,
            new FakeTenantContextAccessor("tenant-dev"),
            new FakeHostEnvironment(Environments.Development),
            BuildConfiguration(new Dictionary<string, string?>
            {
                ["Maintenance:LegacyTenantNormalization:Enabled"] = "true"
            }),
            NullLogger<DevelopmentLegacyTenantDataNormalizer>.Instance);

        Assert.DoesNotThrowAsync(async () => await normalizer.NormalizeAsync());
    }

    private static IEnumerable<IDictionary<string, string?>> DisabledExecutionConfigurations()
    {
        yield return new Dictionary<string, string?>();
        yield return new Dictionary<string, string?>
        {
            ["Maintenance:LegacyTenantNormalization:Enabled"] = "false"
        };
    }

    private static ECommerceDbContext CreateContext()
    {
        DbContextOptions<ECommerceDbContext> options = new DbContextOptionsBuilder<ECommerceDbContext>()
            .UseInMemoryDatabase($"legacy-normalizer-tests-{Guid.NewGuid():N}")
            .Options;

        return new ECommerceDbContext(options, new FakeTenantContextAccessor("tenant-dev"));
    }

    private static IConfiguration BuildConfiguration(IDictionary<string, string?> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    private sealed class FakeTenantContextAccessor(string tenantId) : ITenantContextAccessor
    {
        public string TenantId { get; } = tenantId;
        public bool IsAvailable => true;
    }

    private sealed class FakeHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "PlataformaECommerce.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private sealed class ThrowingHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName
        {
            get => throw new InvalidOperationException("No debe evaluarse el ambiente cuando la normalización está deshabilitada.");
            set => throw new InvalidOperationException("Setter no soportado para este doble de pruebas.");
        }

        public string ApplicationName { get; set; } = "PlataformaECommerce.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
