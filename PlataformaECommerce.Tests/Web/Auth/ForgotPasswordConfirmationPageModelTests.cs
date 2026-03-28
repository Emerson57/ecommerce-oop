using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using PlataformaECommerce.Web.Pages.Auth;

namespace PlataformaECommerce.Tests.Web.Auth;

[TestFixture]
public class ForgotPasswordConfirmationPageModelTests
{
    [Test]
    public void VisibleDevelopmentResetUrl_EntornoDesarrollo_ExponeEnlaceTemporal()
    {
        ForgotPasswordConfirmationModel pageModel = new(new FakeWebHostEnvironment(isDevelopment: true))
        {
            DevelopmentResetUrl = "https://novashop.test/Auth/ResetPassword?userId=1&token=abc"
        };

        Assert.That(pageModel.VisibleDevelopmentResetUrl, Is.EqualTo("https://novashop.test/Auth/ResetPassword?userId=1&token=abc"));
    }

    [Test]
    public void VisibleDevelopmentResetUrl_EntornoNoControlado_OcultaEnlaceTemporal()
    {
        ForgotPasswordConfirmationModel pageModel = new(new FakeWebHostEnvironment(isDevelopment: false))
        {
            DevelopmentResetUrl = "https://novashop.test/Auth/ResetPassword?userId=1&token=abc"
        };

        Assert.That(pageModel.VisibleDevelopmentResetUrl, Is.Null);
    }

    private sealed class FakeWebHostEnvironment : IWebHostEnvironment
    {
        public FakeWebHostEnvironment(bool isDevelopment)
        {
            EnvironmentName = isDevelopment ? Environments.Development : Environments.Production;
        }

        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; } = "PlataformaECommerce.Web.Tests";
        public string WebRootPath { get; set; } = string.Empty;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}