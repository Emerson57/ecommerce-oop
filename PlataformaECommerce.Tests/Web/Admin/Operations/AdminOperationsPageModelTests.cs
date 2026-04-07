using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Application.Common.SaaS;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Web.Configuration;
using PlataformaECommerce.Web.Middlewares;
using PlataformaECommerce.Web.Pages.Admin.Operations;

namespace PlataformaECommerce.Tests.Web.Admin.Operations;

[TestFixture]
public class AdminOperationsPageModelTests
{
    [Test]
    public async Task OnGetAsync_CargaConfiguracionOperativaYCorrelacionActual()
    {
        IndexModel pageModel = new(
            Options.Create(new RequestCorrelationOptions
            {
                CorrelationHeaderName = "X-Correlation-ID"
            }),
            new FakeTenantCatalogService(),
            new FakeHostEnvironment());

        DefaultHttpContext httpContext = new();
        httpContext.TraceIdentifier = "trace-001";
        httpContext.Items[RequestCorrelationMiddleware.CorrelationIdItemKey] = "corr-123";
        pageModel.PageContext = new PageContext
        {
            HttpContext = httpContext
        };

        await pageModel.OnGetAsync();

        Assert.That(pageModel.ClientId, Is.EqualTo("cliente-demo"));
        Assert.That(pageModel.StorefrontName, Is.EqualTo("Tienda Demo"));
        Assert.That(pageModel.CurrentCorrelationId, Is.EqualTo("corr-123"));
        Assert.That(pageModel.SupportEmail, Is.EqualTo("support@demo.example"));
        Assert.That(pageModel.SupportDocuments.Count, Is.EqualTo(5));
        Assert.That(pageModel.DataIsolationMode, Is.EqualTo("SharedDatabaseSharedSchema"));
    }

    private sealed class FakeTenantCatalogService : ITenantCatalogService
    {
        public string DataIsolationMode => "SharedDatabaseSharedSchema";

        public Task<TenantDefinition> GetCurrentTenantAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new TenantDefinition
            {
                TenantId = "cliente-demo",
                DisplayName = "Tenant Demo",
                StorefrontName = "Tienda Demo",
                BackofficeName = "Backoffice Demo",
                SupportEmail = "support@demo.example",
                SupportPhone = "+57 310 000 0000",
                SupportHours = "Lunes a viernes, 09:00 a 17:00 UTC-5",
                SupportSla = "Respuesta inicial en menos de 4 horas hábiles.",
                Subscription = new TenantSubscriptionDefinition { Status = "active", SeatsPurchased = 3 },
                Provisioning = new TenantProvisioningDefinition { BootstrapSuperUserEmail = "root@demo.example" }
            });
        }

        public async Task<IReadOnlyCollection<TenantDefinition>> GetConfiguredTenantsAsync(CancellationToken cancellationToken = default)
            => [await GetCurrentTenantAsync(cancellationToken)];
    }

    private sealed class FakeHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Testing";
        public string ApplicationName { get; set; } = "PlataformaECommerce.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
