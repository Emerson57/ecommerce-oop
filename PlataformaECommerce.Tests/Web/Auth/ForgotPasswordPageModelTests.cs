using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
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
    public async Task OnPostAsync_FormularioValido_RedireccionaAConfirmacionYSolicitaRecuperacion()
    {
        FakeAuthApplicationService authApplicationService = new();
        ForgotPasswordModel pageModel = CreatePageModel(authApplicationService, isDevelopment: false, remoteIpAddress: "10.20.30.40");
        pageModel.Input.Email = "  cliente@plataforma.com  ";

        IActionResult result = await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<RedirectToPageResult>());
        RedirectToPageResult redirect = (RedirectToPageResult)result;
        Assert.That(redirect.PageName, Is.EqualTo("/Auth/ForgotPasswordConfirmation"));
        Assert.That(pageModel.StatusMessage, Does.Contain("Si la cuenta existe y está habilitada"));
        Assert.That(authApplicationService.RequestPasswordResetCalls, Is.EqualTo(1));
        Assert.That(authApplicationService.LastRequestPasswordResetCommand?.Email, Is.EqualTo("cliente@plataforma.com"));
        Assert.That(authApplicationService.LastRequestPasswordResetCommand?.Source, Is.EqualTo("Web.Auth.ForgotPassword"));
        Assert.That(authApplicationService.LastRequestPasswordResetCommand?.IpAddress, Is.EqualTo("10.20.30.40"));
    }

    [Test]
    public async Task OnPostAsync_ModeloInvalido_NoInvocaRecuperacion()
    {
        FakeAuthApplicationService authApplicationService = new();
        ForgotPasswordModel pageModel = CreatePageModel(authApplicationService, isDevelopment: false);
        pageModel.ModelState.AddModelError(nameof(ForgotPasswordModel.InputModel.Email), "Correo inválido.");

        IActionResult result = await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(authApplicationService.RequestPasswordResetCalls, Is.EqualTo(0));
    }

    [Test]
    public async Task OnPostAsync_AplicacionRetornaError_PublicaMensajeFuncional()
    {
        FakeAuthApplicationService authApplicationService = new()
        {
            RequestPasswordResetResult = Result.Failure<PasswordResetRequestResultDto>(
                Error.Validation("Auth.InvalidRequest", "La solicitud de recuperación no es válida."))
        };
        ForgotPasswordModel pageModel = CreatePageModel(authApplicationService, isDevelopment: false);
        pageModel.Input.Email = "cliente@plataforma.com";

        IActionResult result = await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(pageModel.ErrorMessage, Is.EqualTo("La solicitud de recuperación no es válida."));
    }

    [Test]
    public async Task OnPostAsync_EntornoDesarrolloConVistaPrevia_GuardaEnlaceTemporal()
    {
        Guid userId = Guid.NewGuid();
        FakeAuthApplicationService authApplicationService = new()
        {
            RequestPasswordResetResult = Result.Success(new PasswordResetRequestResultDto
            {
                UserId = userId,
                ResetToken = "token-temporal-2026",
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(30)
            })
        };
        ForgotPasswordModel pageModel = CreatePageModel(authApplicationService, isDevelopment: true, requestScheme: "https", requestHost: "novashop.test");
        pageModel.Input.Email = "cliente@plataforma.com";

        IActionResult result = await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<RedirectToPageResult>());
        Assert.That(pageModel.DevelopmentResetUrl, Is.EqualTo($"https://novashop.test/Auth/ResetPassword?userId={userId}&token=token-temporal-2026"));
    }

    private static ForgotPasswordModel CreatePageModel(
        FakeAuthApplicationService authApplicationService,
        bool isDevelopment,
        string? remoteIpAddress = null,
        string requestScheme = "https",
        string requestHost = "localhost")
    {
        ForgotPasswordModel pageModel = new(authApplicationService, new FakeWebHostEnvironment(isDevelopment));
        DefaultHttpContext httpContext = new();
        httpContext.Request.Scheme = requestScheme;
        httpContext.Request.Host = new HostString(requestHost);
        httpContext.Request.Headers.UserAgent = "NUnit-Test-Agent";

        if (!string.IsNullOrWhiteSpace(remoteIpAddress))
        {
            httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse(remoteIpAddress);
        }

        pageModel.PageContext = new PageContext
        {
            HttpContext = httpContext
        };
        pageModel.TempData = new TempDataDictionary(httpContext, new FakeTempDataProvider());
        return pageModel;
    }

    private sealed class FakeAuthApplicationService : IAuthApplicationService
    {
        public int RequestPasswordResetCalls { get; private set; }
        public RequestPasswordResetCommand? LastRequestPasswordResetCommand { get; private set; }

        public Result<PasswordResetRequestResultDto> RequestPasswordResetResult { get; set; } = Result.Success(new PasswordResetRequestResultDto());

        public Task<Result<AuthResponseDto>> LoginAsync(LoginCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<PasswordResetRequestResultDto>> RequestPasswordResetAsync(RequestPasswordResetCommand command, CancellationToken cancellationToken = default)
        {
            RequestPasswordResetCalls++;
            LastRequestPasswordResetCommand = command;
            return Task.FromResult(RequestPasswordResetResult);
        }

        public Task<Result> ChangePasswordAsync(ChangePasswordCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result> ResetPasswordAsync(ResetPasswordCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<CurrentUserDto>> GetCurrentUserAsync(GetCurrentUserQuery query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
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

    private sealed class FakeTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}