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
    public void OnGetAsync_EnlaceInvalido_RedireccionaALogin()
    {
        ResetPasswordModel pageModel = CreatePageModel(new FakeAuthApplicationService());

        IActionResult result = pageModel.OnGet(Guid.Empty, null);

        Assert.That(result, Is.TypeOf<RedirectToPageResult>());
        Assert.That(pageModel.StatusMessage, Does.Contain("no es válido"));
    }

    [Test]
    public async Task OnPostAsync_FormularioValido_DelegayRedireccionaALogin()
    {
        FakeAuthApplicationService authApplicationService = new();
        ResetPasswordModel pageModel = CreatePageModel(authApplicationService);
        pageModel.Input = new ResetPasswordModel.InputModel
        {
            UserId = Guid.NewGuid(),
            Token = "token-seguro-2026",
            NewPassword = "Password#2027",
            ConfirmPassword = "Password#2027"
        };

        IActionResult result = await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<RedirectToPageResult>());
        Assert.That(authApplicationService.LastResetPasswordCommand?.Token, Is.EqualTo("token-seguro-2026"));
        Assert.That(pageModel.StatusMessage, Does.Contain("restablecida correctamente"));
    }

    private static ResetPasswordModel CreatePageModel(FakeAuthApplicationService authApplicationService)
    {
        DefaultHttpContext httpContext = new();
        httpContext.Request.Scheme = "https";
        ResetPasswordModel pageModel = new(authApplicationService)
        {
            PageContext = new PageContext
            {
                HttpContext = httpContext
            },
            TempData = new TempDataDictionary(httpContext, new FakeTempDataProvider())
        };

        return pageModel;
    }

    private sealed class FakeAuthApplicationService : IAuthApplicationService
    {
        public ResetPasswordCommand? LastResetPasswordCommand { get; private set; }

        public Task<Result<AuthResponseDto>> LoginAsync(LoginCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<CurrentUserDto>> GetCurrentUserAsync(GetCurrentUserQuery query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<PasswordResetRequestResultDto>> RequestPasswordResetAsync(RequestPasswordResetCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result> ChangePasswordAsync(ChangePasswordCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result> ResetPasswordAsync(ResetPasswordCommand command, CancellationToken cancellationToken = default)
        {
            LastResetPasswordCommand = command;
            return Task.FromResult(Result.Success());
        }
    }

    private sealed class FakeTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
