using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Auth.Commands;
using PlataformaECommerce.Application.Features.Auth.DTOs;
using PlataformaECommerce.Application.Features.Auth.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Auth;
using PlataformaECommerce.Web.Pages.Auth;

namespace PlataformaECommerce.Tests.Web.Auth;

[TestFixture]
public class ForgotPasswordPageModelTests
{
    [Test]
    public async Task OnPostAsync_FormularioValido_DelegaSolicitudYRedirigeAConfirmacion()
    {
        FakeAuthApplicationService service = new();
        ForgotPasswordModel pageModel = CreatePageModel(service, environmentName: "Development", remoteIpAddress: "10.20.30.40");
        pageModel.Input.Email = "cliente@plataforma.com";

        IActionResult result = await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<RedirectToPageResult>());
        Assert.That(service.LastCommand?.Email, Is.EqualTo("cliente@plataforma.com"));
        Assert.That(service.LastCommand?.IpAddress, Is.EqualTo("10.20.30.40"));
        Assert.That(service.LastCommand?.ResetPasswordUrl, Does.Contain("userId=%7BuserId%7D"));
        Assert.That(pageModel.StatusMessage, Does.Contain("Si la cuenta existe"));
        Assert.That(pageModel.DevelopmentResetUrl, Does.Contain("/Auth/ResetPassword"));
    }

    [Test]
    public async Task OnPostAsync_Produccion_NoExponeUrlTemporalDeRecuperacion()
    {
        FakeAuthApplicationService service = new();
        ForgotPasswordModel pageModel = CreatePageModel(service, environmentName: "Production");
        pageModel.Input.Email = "cliente@plataforma.com";

        IActionResult result = await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<RedirectToPageResult>());
        Assert.That(pageModel.DevelopmentResetUrl, Is.Null);
    }

    [Test]
    public async Task OnPostAsync_AplicacionRetornaError_PublicaMensajeFuncional()
    {
        FakeAuthApplicationService service = new()
        {
            RequestPasswordResetResult = Result.Failure<PasswordResetRequestResultDto>(Error.Validation("Auth.PasswordResetUnavailable", "No fue posible procesar la recuperación."))
        };
        ForgotPasswordModel pageModel = CreatePageModel(service);
        pageModel.Input.Email = "cliente@plataforma.com";

        IActionResult result = await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(pageModel.ErrorMessage, Is.EqualTo("No fue posible procesar la recuperación."));
    }

    private static ForgotPasswordModel CreatePageModel(
        FakeAuthApplicationService authApplicationService,
        string environmentName = "Development",
        string? remoteIpAddress = null)
    {
        ForgotPasswordModel pageModel = new(authApplicationService, new FakeWebHostEnvironment(environmentName), new FakeLinkGenerator());
        DefaultHttpContext httpContext = new();
        RouteData routeData = new();
        Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor actionDescriptor = new();

        if (!string.IsNullOrWhiteSpace(remoteIpAddress))
        {
            httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse(remoteIpAddress);
        }

        pageModel.PageContext = new PageContext(new Microsoft.AspNetCore.Mvc.ActionContext(httpContext, routeData, actionDescriptor));
        pageModel.TempData = new TempDataDictionary(httpContext, new FakeTempDataProvider());
        httpContext.Request.Scheme = "https";
        return pageModel;
    }

    private sealed class FakeLinkGenerator : LinkGenerator
    {
        public override string? GetPathByAddress<TAddress>(HttpContext httpContext, TAddress address, RouteValueDictionary values, RouteValueDictionary? ambientValues = null, PathString? pathBase = null, FragmentString fragment = default, LinkOptions? options = null)
            => "/Auth/ResetPassword";

        public override string? GetPathByAddress<TAddress>(TAddress address, RouteValueDictionary values, PathString pathBase = default, FragmentString fragment = default, LinkOptions? options = null)
            => "/Auth/ResetPassword";

        public override string? GetUriByAddress<TAddress>(HttpContext httpContext, TAddress address, RouteValueDictionary values, RouteValueDictionary? ambientValues = null, string? scheme = null, HostString? host = null, PathString? pathBase = null, FragmentString fragment = default, LinkOptions? options = null)
        {
            string? userId = values["userId"]?.ToString();
            string? token = values["token"]?.ToString();
            return $"https://localhost/Auth/ResetPassword?userId={Uri.EscapeDataString(userId ?? string.Empty)}&token={Uri.EscapeDataString(token ?? string.Empty)}";
        }

        public override string? GetUriByAddress<TAddress>(TAddress address, RouteValueDictionary values, string scheme, HostString host, PathString pathBase = default, FragmentString fragment = default, LinkOptions? options = null)
        {
            string? userId = values["userId"]?.ToString();
            string? token = values["token"]?.ToString();
            return $"{scheme}://{host}/Auth/ResetPassword?userId={Uri.EscapeDataString(userId ?? string.Empty)}&token={Uri.EscapeDataString(token ?? string.Empty)}";
        }
    }

    private sealed class FakeAuthApplicationService : IAuthApplicationService
    {
        public RequestPasswordResetCommand? LastCommand { get; private set; }

        public Result<PasswordResetRequestResultDto> RequestPasswordResetResult { get; set; } = Result.Success(new PasswordResetRequestResultDto
        {
            UserId = Guid.NewGuid(),
            ResetToken = "token-seguro-2026",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(30)
        });

        public Task<Result<AuthResponseDto>> LoginAsync(LoginCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<CurrentUserDto>> GetCurrentUserAsync(GetCurrentUserQuery query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<PasswordResetRequestResultDto>> RequestPasswordResetAsync(RequestPasswordResetCommand command, CancellationToken cancellationToken = default)
        {
            LastCommand = command;
            return Task.FromResult(RequestPasswordResetResult);
        }

        public Task<Result> ChangePasswordAsync(ChangePasswordCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result> ResetPasswordAsync(ResetPasswordCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class FakeTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }

    private sealed class FakeWebHostEnvironment(string environmentName) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "PlataformaECommerce.Web";
        public Microsoft.Extensions.FileProviders.IFileProvider WebRootFileProvider { get; set; } = null!;
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = environmentName;
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}
