using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Web.Configuration;
using PlataformaECommerce.Web.Middlewares;
using PlataformaECommerce.Web.Pages.Admin.Operations;

namespace PlataformaECommerce.Tests.Web.Admin.Operations;

[TestFixture]
public class AdminOperationsPageModelTests
{
    [Test]
    public void OnGet_CargaConfiguracionOperativaYCorrelacionActual()
    {
        IndexModel pageModel = new(
            Options.Create(new ClientExperienceOptions
            {
                ClientId = "cliente-demo",
                StorefrontName = "Tienda Demo",
                BackofficeName = "Backoffice Demo",
                SupportEmail = "support@demo.example",
                SupportPhone = "+57 310 000 0000",
                SupportHours = "Lunes a viernes, 09:00 a 17:00 UTC-5",
                SupportSla = "Respuesta inicial en menos de 4 horas hábiles."
            }),
            Options.Create(new RequestCorrelationOptions
            {
                CorrelationHeaderName = "X-Correlation-ID"
            }),
            new FakeHostEnvironment());

        DefaultHttpContext httpContext = new();
        httpContext.TraceIdentifier = "trace-001";
        httpContext.Items[RequestCorrelationMiddleware.CorrelationIdItemKey] = "corr-123";
        pageModel.PageContext = new PageContext
        {
            HttpContext = httpContext
        };

        pageModel.OnGet();

        Assert.That(pageModel.ClientId, Is.EqualTo("cliente-demo"));
        Assert.That(pageModel.StorefrontName, Is.EqualTo("Tienda Demo"));
        Assert.That(pageModel.CurrentCorrelationId, Is.EqualTo("corr-123"));
        Assert.That(pageModel.SupportEmail, Is.EqualTo("support@demo.example"));
        Assert.That(pageModel.SupportDocuments.Count, Is.EqualTo(4));
    }

    private sealed class FakeHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Testing";
        public string ApplicationName { get; set; } = "PlataformaECommerce.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
