using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Users.Commands;
using PlataformaECommerce.Application.Features.Users.DTOs;
using PlataformaECommerce.Application.Features.Users.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Users;
using PlataformaECommerce.Web.Pages.Auth;

namespace PlataformaECommerce.Tests.Web.Auth;

[TestFixture]
public class ResendEmailConfirmationPageModelTests
{
    [Test]
    public async Task OnPostAsync_FormularioValido_DelegaReenvioYRedirigeALaMismaPagina()
    {
        FakeUserApplicationService service = new();
        ResendEmailConfirmationModel pageModel = CreatePageModel(service, remoteIpAddress: "10.20.30.40");
        pageModel.Input.Email = "cliente@plataforma.com";

        IActionResult result = await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<RedirectToPageResult>());
        Assert.That(service.LastResendCommand?.Email, Is.EqualTo("cliente@plataforma.com"));
        Assert.That(service.LastResendCommand?.IpAddress, Is.EqualTo("10.20.30.40"));
        Assert.That(service.LastResendCommand?.EmailConfirmationUrl, Does.Contain("/Auth/ConfirmEmail"));
        Assert.That(pageModel.StatusMessage, Does.Contain("Si la cuenta existe"));
    }

    [Test]
    public async Task OnPostAsync_AplicacionRetornaError_PublicaMensajeFuncional()
    {
        FakeUserApplicationService service = new()
        {
            ResendResult = Result.Failure(Error.Validation("Users.EmailConfirmationUnavailable", "No fue posible reenviar la confirmación."))
        };
        ResendEmailConfirmationModel pageModel = CreatePageModel(service);
        pageModel.Input.Email = "cliente@plataforma.com";

        IActionResult result = await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(pageModel.ErrorMessage, Is.EqualTo("No fue posible reenviar la confirmación."));
    }

    private static ResendEmailConfirmationModel CreatePageModel(FakeUserApplicationService userApplicationService, string? remoteIpAddress = null)
    {
        ResendEmailConfirmationModel pageModel = new(userApplicationService, new FakeLinkGenerator());
        DefaultHttpContext httpContext = new();
        httpContext.Request.Scheme = "https";

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

    private sealed class FakeLinkGenerator : LinkGenerator
    {
        public override string? GetPathByAddress<TAddress>(HttpContext httpContext, TAddress address, RouteValueDictionary values, RouteValueDictionary? ambientValues = null, PathString? pathBase = null, FragmentString fragment = default, LinkOptions? options = null)
            => "/Auth/ConfirmEmail";

        public override string? GetPathByAddress<TAddress>(TAddress address, RouteValueDictionary values, PathString pathBase = default, FragmentString fragment = default, LinkOptions? options = null)
            => "/Auth/ConfirmEmail";

        public override string? GetUriByAddress<TAddress>(HttpContext httpContext, TAddress address, RouteValueDictionary values, RouteValueDictionary? ambientValues = null, string? scheme = null, HostString? host = null, PathString? pathBase = null, FragmentString fragment = default, LinkOptions? options = null)
            => $"https://shop.example.com/Auth/ConfirmEmail?userId={Uri.EscapeDataString(values["userId"]?.ToString() ?? string.Empty)}&token={Uri.EscapeDataString(values["token"]?.ToString() ?? string.Empty)}";

        public override string? GetUriByAddress<TAddress>(TAddress address, RouteValueDictionary values, string scheme, HostString host, PathString pathBase = default, FragmentString fragment = default, LinkOptions? options = null)
            => $"{scheme}://{host}/Auth/ConfirmEmail?userId={Uri.EscapeDataString(values["userId"]?.ToString() ?? string.Empty)}&token={Uri.EscapeDataString(values["token"]?.ToString() ?? string.Empty)}";
    }

    private sealed class FakeUserApplicationService : IUserApplicationService
    {
        public ResendUserEmailConfirmationCommand? LastResendCommand { get; private set; }

        public Result ResendResult { get; set; } = Result.Success();

        public Task<Result<CustomerDto>> RegisterCustomerAsync(RegisterCustomerCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<UserDto>> UpdateUserBasicDataAsync(UpdateUserBasicDataCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<UserDto>> ConfirmUserEmailAsync(ConfirmUserEmailCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result> ResendUserEmailConfirmationAsync(ResendUserEmailConfirmationCommand command, CancellationToken cancellationToken = default)
        {
            LastResendCommand = command;
            return Task.FromResult(ResendResult);
        }

        public Task<Result<UserDto>> ActivateUserAsync(ActivateUserCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<UserDto>> DeactivateUserAsync(DeactivateUserCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<UserDto>> GetUserByIdAsync(GetUserByIdQuery query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<UserDto>> GetUserByEmailAsync(GetUserByEmailQuery query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class FakeTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
     }
 }
