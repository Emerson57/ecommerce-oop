using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Auth.Commands;
using PlataformaECommerce.Application.Features.Auth.DTOs;
using PlataformaECommerce.Application.Features.Auth.Queries;
using PlataformaECommerce.Application.Features.Users.Commands;
using PlataformaECommerce.Application.Features.Users.DTOs;
using PlataformaECommerce.Application.Features.Users.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Auth;
using PlataformaECommerce.Application.Interfaces.Services.Users;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Web.Authorization;
using PlataformaECommerce.Web.Pages.Account;

namespace PlataformaECommerce.Tests.Web.Account;

[TestFixture]
public class AccountIndexPageModelTests
{
    [Test]
    public async Task OnGetAsync_ClienteAutenticado_CargaPerfilYRetornaPagina()
    {
        FakeAuthApplicationService authApplicationService = new();
        IndexModel pageModel = CreatePageModel(authApplicationService, new FakeUserApplicationService(), Guid.NewGuid(), "cliente@plataforma.com");

        IActionResult result = await pageModel.OnGetAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(pageModel.Account.Email, Is.EqualTo("cliente@plataforma.com"));
        Assert.That(pageModel.Profile.Name, Is.EqualTo("Cliente Demo"));
    }

    [Test]
    public async Task OnPostUpdateProfileAsync_MismoCorreo_ActualizaDatosYRedirigeAMiCuenta()
    {
        FakeAuthApplicationService authApplicationService = new();
        FakeUserApplicationService userApplicationService = new();
        IndexModel pageModel = CreatePageModel(authApplicationService, userApplicationService, Guid.NewGuid(), "cliente@plataforma.com");
        pageModel.Profile = new IndexModel.UpdateProfileInputModel
        {
            Name = "Cliente Ajustado",
            Email = "cliente@plataforma.com"
        };

        IActionResult result = await pageModel.OnPostUpdateProfileAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<RedirectToPageResult>());
        Assert.That(userApplicationService.LastUpdateCommand?.Name, Is.EqualTo("Cliente Ajustado"));
        Assert.That(userApplicationService.LastUpdateCommand?.Email, Is.EqualTo("cliente@plataforma.com"));
    }

    [Test]
    public async Task OnPostUpdateProfileAsync_CambioDeCorreo_RedireccionaALoginYRevocaSesion()
    {
        FakeAuthApplicationService authApplicationService = new();
        FakeUserApplicationService userApplicationService = new();
        FakeAuthenticationService authenticationService = new();
        IndexModel pageModel = CreatePageModel(authApplicationService, userApplicationService, Guid.NewGuid(), "cliente@plataforma.com", authenticationService);
        pageModel.Profile = new IndexModel.UpdateProfileInputModel
        {
            Name = "Cliente Ajustado",
            Email = "nuevo@plataforma.com"
        };

        IActionResult result = await pageModel.OnPostUpdateProfileAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<RedirectToPageResult>());
        Assert.That(authenticationService.LastSignOutScheme, Is.EqualTo(AuthorizationPolicies.CustomerCookieScheme));
    }

    [Test]
    public async Task OnPostChangePasswordAsync_OperacionExitosa_RedireccionaALoginYRevocaSesion()
    {
        FakeAuthApplicationService authApplicationService = new();
        FakeAuthenticationService authenticationService = new();
        IndexModel pageModel = CreatePageModel(authApplicationService, new FakeUserApplicationService(), Guid.NewGuid(), "cliente@plataforma.com", authenticationService);
        pageModel.PasswordChange = new IndexModel.ChangePasswordInputModel
        {
            CurrentPassword = "Password#2026",
            NewPassword = "Password#2027",
            ConfirmPassword = "Password#2027"
        };

        IActionResult result = await pageModel.OnPostChangePasswordAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<RedirectToPageResult>());
        Assert.That(authApplicationService.LastChangePasswordCommand?.CurrentPassword, Is.EqualTo("Password#2026"));
        Assert.That(authenticationService.LastSignOutScheme, Is.EqualTo(AuthorizationPolicies.CustomerCookieScheme));
    }

    [Test]
    public async Task OnGetAsync_SinIdentificadorAutenticado_RedireccionaALoginYRevocaSesion()
    {
        FakeAuthApplicationService authApplicationService = new();
        FakeAuthenticationService authenticationService = new();
        IndexModel pageModel = CreatePageModel(authApplicationService, new FakeUserApplicationService(), null, null, authenticationService);

        IActionResult result = await pageModel.OnGetAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<RedirectToPageResult>());
        Assert.That(authenticationService.LastSignOutScheme, Is.EqualTo(AuthorizationPolicies.CustomerCookieScheme));
    }

    private static IndexModel CreatePageModel(
        FakeAuthApplicationService authApplicationService,
        FakeUserApplicationService userApplicationService,
        Guid? authenticatedUserId,
        string? email,
        FakeAuthenticationService? authenticationService = null)
    {
        authenticationService ??= new FakeAuthenticationService();
        IndexModel pageModel = new(authApplicationService, userApplicationService);

        ServiceCollection services = new();
        services.AddSingleton<IAuthenticationService>(authenticationService);
        DefaultHttpContext httpContext = new()
        {
            RequestServices = services.BuildServiceProvider(),
            User = CreatePrincipal(authenticatedUserId, email)
        };

        pageModel.PageContext = new PageContext
        {
            HttpContext = httpContext
        };
        pageModel.TempData = new TempDataDictionary(httpContext, new FakeTempDataProvider());

        return pageModel;
    }

    private static ClaimsPrincipal CreatePrincipal(Guid? authenticatedUserId, string? email)
    {
        List<Claim> claims = [];

        if (authenticatedUserId.HasValue)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, authenticatedUserId.Value.ToString()));
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            claims.Add(new Claim(ClaimTypes.Email, email));
        }

        claims.Add(new Claim(ClaimTypes.Name, "Cliente Demo"));
        claims.Add(new Claim(ClaimTypes.Role, RolUsuario.Cliente.ToString()));
        claims.Add(new Claim(AuthorizationPolicies.PrimaryRoleClaimType, RolUsuario.Cliente.ToString()));
        claims.Add(new Claim(AuthorizationPolicies.SuperUserClaimType, bool.FalseString));

        return new ClaimsPrincipal(new ClaimsIdentity(claims, AuthorizationPolicies.CustomerCookieScheme));
    }

    private sealed class FakeAuthApplicationService : IAuthApplicationService
    {
        public ChangePasswordCommand? LastChangePasswordCommand { get; private set; }

        public Result<CurrentUserDto> CurrentUserResult { get; set; } = Result.Success(new CurrentUserDto
        {
            Id = Guid.NewGuid(),
            UserName = "cliente@plataforma.com",
            Email = "cliente@plataforma.com",
            FullName = "Cliente Demo",
            IsActive = true,
            IsEmailConfirmed = true,
            Role = RolUsuario.Cliente.ToString(),
            Roles = [RolUsuario.Cliente.ToString()],
            CreatedAtUtc = DateTime.UtcNow.AddDays(-10),
            LastLoginAtUtc = DateTime.UtcNow.AddMinutes(-30)
        });

        public Task<Result<AuthResponseDto>> LoginAsync(LoginCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<PasswordResetRequestResultDto>> RequestPasswordResetAsync(RequestPasswordResetCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result> ChangePasswordAsync(ChangePasswordCommand command, CancellationToken cancellationToken = default)
        {
            LastChangePasswordCommand = command;
            return Task.FromResult(Result.Success());
        }

        public Task<Result> ResetPasswordAsync(ResetPasswordCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<CurrentUserDto>> GetCurrentUserAsync(GetCurrentUserQuery query, CancellationToken cancellationToken = default)
            => Task.FromResult(CurrentUserResult);
    }

    private sealed class FakeUserApplicationService : IUserApplicationService
    {
        public UpdateUserBasicDataCommand? LastUpdateCommand { get; private set; }

        public Task<Result<CustomerDto>> RegisterCustomerAsync(RegisterCustomerCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<UserDto>> UpdateUserBasicDataAsync(UpdateUserBasicDataCommand command, CancellationToken cancellationToken = default)
        {
            LastUpdateCommand = command;
            return Task.FromResult(Result.Success(new UserDto
            {
                Id = command.UserId,
                Name = command.Name,
                Email = command.Email,
                Role = RolUsuario.Cliente,
                IsActive = true,
                IsEmailConfirmed = string.Equals(command.Email, "cliente@plataforma.com", StringComparison.OrdinalIgnoreCase),
                CreatedAtUtc = DateTime.UtcNow.AddDays(-10)
            }));
        }

        public Task<Result<UserDto>> ConfirmUserEmailAsync(ConfirmUserEmailCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result> ResendUserEmailConfirmationAsync(ResendUserEmailConfirmationCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<UserDto>> ActivateUserAsync(ActivateUserCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<UserDto>> DeactivateUserAsync(DeactivateUserCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<UserDto>> GetUserByIdAsync(GetUserByIdQuery query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<UserDto>> GetUserByEmailAsync(GetUserByEmailQuery query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class FakeAuthenticationService : IAuthenticationService
    {
        public string? LastSignOutScheme { get; private set; }

        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme)
            => Task.FromResult(AuthenticateResult.NoResult());

        public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
        {
            LastSignOutScheme = scheme;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
