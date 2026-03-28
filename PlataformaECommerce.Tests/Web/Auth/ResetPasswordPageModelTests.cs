using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Auth.Commands;
using PlataformaECommerce.Application.Features.Auth.DTOs;
using PlataformaECommerce.Application.Features.Auth.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Auth;
using PlataformaECommerce.Web.Pages.Auth;

namespace PlataformaECommerce.Tests.Web.Auth;

[TestFixture]
public class ResetPasswordPageModelTests
{
    [Test]
    public void OnGet_SinContextoDeRecuperacion_PublicaErrorSeguro()
    {
        ResetPasswordModel pageModel = CreatePageModel(new FakeAuthApplicationService());

        pageModel.OnGet();

        Assert.That(pageModel.ErrorMessage, Does.Contain("no es válido o está incompleto"));
    }

    [Test]
    public async Task OnPostAsync_FormularioValido_ConsumeResetYRedirigeALogin()
    {
        FakeAuthApplicationService authApplicationService = new();
        ResetPasswordModel pageModel = CreatePageModel(authApplicationService, remoteIpAddress: "10.20.30.40");
        Guid userId = Guid.NewGuid();
        pageModel.UserId = userId;
        pageModel.Token = " token-valido-2026 ";
        pageModel.Input = new ResetPasswordModel.InputModel
        {
            NewPassword = "Password#2027",
            ConfirmPassword = "Password#2027"
        };

        IActionResult result = await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<RedirectToPageResult>());
        RedirectToPageResult redirect = (RedirectToPageResult)result;
        Assert.That(redirect.PageName, Is.EqualTo("/Auth/Login"));
        Assert.That(pageModel.StatusMessage, Does.Contain("fue restablecida correctamente"));
        Assert.That(authApplicationService.ResetPasswordCalls, Is.EqualTo(1));
        Assert.That(authApplicationService.LastResetPasswordCommand?.UserId, Is.EqualTo(userId));
        Assert.That(authApplicationService.LastResetPasswordCommand?.Token, Is.EqualTo("token-valido-2026"));
        Assert.That(authApplicationService.LastResetPasswordCommand?.Source, Is.EqualTo("Web.Auth.ResetPassword"));
        Assert.That(authApplicationService.LastResetPasswordCommand?.IpAddress, Is.EqualTo("10.20.30.40"));
    }

    [Test]
    public async Task OnPostAsync_ModeloInvalido_NoInvocaRestablecimiento()
    {
        FakeAuthApplicationService authApplicationService = new();
        ResetPasswordModel pageModel = CreatePageModel(authApplicationService);
        pageModel.UserId = Guid.NewGuid();
        pageModel.Token = "token-valido-2026";
        pageModel.ModelState.AddModelError(nameof(ResetPasswordModel.InputModel.NewPassword), "Contraseña inválida.");

        IActionResult result = await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(authApplicationService.ResetPasswordCalls, Is.EqualTo(0));
    }

    [Test]
    public async Task OnPostAsync_SinContextoDeRecuperacion_PublicaErrorSeguro()
    {
        FakeAuthApplicationService authApplicationService = new();
        ResetPasswordModel pageModel = CreatePageModel(authApplicationService);
        pageModel.Input = new ResetPasswordModel.InputModel
        {
            NewPassword = "Password#2027",
            ConfirmPassword = "Password#2027"
        };

        IActionResult result = await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(pageModel.ErrorMessage, Does.Contain("no es válido o está incompleto"));
        Assert.That(authApplicationService.ResetPasswordCalls, Is.EqualTo(0));
    }

    [Test]
    public async Task OnPostAsync_AplicacionRetornaError_PublicaMensajeFuncional()
    {
        FakeAuthApplicationService authApplicationService = new()
        {
            ResetPasswordResult = Result.Failure(
                Error.Unauthorized("Auth.InvalidPasswordResetToken", "El enlace de recuperación no es válido o ya expiró."))
        };
        ResetPasswordModel pageModel = CreatePageModel(authApplicationService);
        pageModel.UserId = Guid.NewGuid();
        pageModel.Token = "token-valido-2026";
        pageModel.Input = new ResetPasswordModel.InputModel
        {
            NewPassword = "Password#2027",
            ConfirmPassword = "Password#2027"
        };

        IActionResult result = await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(pageModel.ErrorMessage, Is.EqualTo("El enlace de recuperación no es válido o ya expiró."));
    }

    private static ResetPasswordModel CreatePageModel(FakeAuthApplicationService authApplicationService, string? remoteIpAddress = null)
    {
        ResetPasswordModel pageModel = new(authApplicationService);
        DefaultHttpContext httpContext = new();
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
        public int ResetPasswordCalls { get; private set; }
        public ResetPasswordCommand? LastResetPasswordCommand { get; private set; }

        public Result ResetPasswordResult { get; set; } = Result.Success();

        public Task<Result<AuthResponseDto>> LoginAsync(LoginCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<PasswordResetRequestResultDto>> RequestPasswordResetAsync(RequestPasswordResetCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result> ChangePasswordAsync(ChangePasswordCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result> ResetPasswordAsync(ResetPasswordCommand command, CancellationToken cancellationToken = default)
        {
            ResetPasswordCalls++;
            LastResetPasswordCommand = command;
            return Task.FromResult(ResetPasswordResult);
        }

        public Task<Result<CurrentUserDto>> GetCurrentUserAsync(GetCurrentUserQuery query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class FakeTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}