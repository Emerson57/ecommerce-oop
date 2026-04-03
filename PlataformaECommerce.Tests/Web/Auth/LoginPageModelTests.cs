using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Auth.Commands;
using PlataformaECommerce.Application.Features.Auth.DTOs;
using PlataformaECommerce.Application.Features.Auth.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Auth;
using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.ValueObjects;
using PlataformaECommerce.Web.Authorization;
using PlataformaECommerce.Web.Pages.Auth;

namespace PlataformaECommerce.Tests.Web.Auth;

[TestFixture]
public class LoginPageModelTests
{
    [Test]
    public async Task OnPostAsync_AdminValido_EmiteCookieYRedirigeAAuditoria()
    {
        Administrador administrator = new("Admin Demo", new Email("admin@plataforma.com"), "hash-seguro-admin-2026", "Operaciones");
        administrator.ConfirmarCorreoElectronico();
        FakeAuthenticationService authenticationService = new();
        LoginModel pageModel = CreatePageModel(administrator, authenticationService);
        pageModel.Input.Email = "admin@plataforma.com";
        pageModel.Input.Password = "Password#2026";

        var result = await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.InstanceOf<Microsoft.AspNetCore.Mvc.RedirectToPageResult>());
        Assert.That(authenticationService.SignedInPrincipal?.IsInRole("Administrador"), Is.True);
    }

    [Test]
    public async Task OnPostAsync_ClienteValido_EmiteCookieDeClienteYRedirigeAlInicio()
    {
        Cliente customer = new("Cliente Demo", new Email("cliente@plataforma.com"), "hash-seguro-cliente-2026");
        customer.ConfirmarCorreoElectronico();
        FakeAuthenticationService authenticationService = new();
        LoginModel pageModel = CreatePageModel(customer, authenticationService);
        pageModel.Input.Email = "cliente@plataforma.com";
        pageModel.Input.Password = "Password#2026";

        var result = await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.InstanceOf<Microsoft.AspNetCore.Mvc.RedirectToPageResult>());
        Assert.That(authenticationService.LastSignInScheme, Is.EqualTo(AuthorizationPolicies.CustomerCookieScheme));
        Assert.That(authenticationService.SignedInPrincipal?.IsInRole(RolUsuario.Cliente.ToString()), Is.True);
    }

    [Test]
    public async Task OnPostAsync_SuperUsuarioValido_EmiteCookieConRolesEfectivos()
    {
        Administrador superUser = new("Root Demo", new Email("root@plataforma.com"), "hash-seguro-root-2026", "Plataforma", RolUsuario.SuperUsuario);
        superUser.ConfirmarCorreoElectronico();
        FakeAuthenticationService authenticationService = new();
        LoginModel pageModel = CreatePageModel(superUser, authenticationService);
        pageModel.Input.Email = "root@plataforma.com";
        pageModel.Input.Password = "Password#2026";

        var result = await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.InstanceOf<Microsoft.AspNetCore.Mvc.RedirectToPageResult>());
        Assert.That(authenticationService.LastSignInScheme, Is.EqualTo(AuthorizationPolicies.AdminCookieScheme));
        Assert.That(authenticationService.SignedInPrincipal?.IsInRole(RolUsuario.SuperUsuario.ToString()), Is.True);
        Assert.That(authenticationService.SignedInPrincipal?.IsInRole(RolUsuario.Administrador.ToString()), Is.True);
    }

    [Test]
    public async Task OnPostAsync_IdentidadAdministrativaInconsistente_RetornaPaginaConError()
    {
        Administrador administrator = new("Admin Demo", new Email("admin@plataforma.com"), "hash-seguro-admin-2026", "Operaciones");
        administrator.ConfirmarCorreoElectronico();
        FakeAuthenticationService authenticationService = new();
        FakeAuthApplicationService authApplicationService = new(administrator)
        {
            LoginUserOverride = new CurrentUserDto
            {
                Id = administrator.Id,
                UserName = administrator.CorreoElectronico.Value,
                Email = administrator.CorreoElectronico.Value,
                FullName = administrator.Nombre,
                IsActive = true,
                IsEmailConfirmed = true,
                Role = RolUsuario.Administrador.ToString(),
                Roles = [RolUsuario.Administrador.ToString()],
                IsSuperUser = true,
                Area = administrator.Area
            }
        };

        LoginModel pageModel = CreatePageModel(authApplicationService, authenticationService);
        pageModel.Input.Email = "admin@plataforma.com";
        pageModel.Input.Password = "Password#2026";

        var result = await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.InstanceOf<PageResult>());
        Assert.That(pageModel.ErrorMessage, Does.Contain("seguridad"));
    }

    [Test]
    public async Task OnPostAsync_ReturnUrlLocal_RedireccionaLocalmente()
    {
        Cliente customer = new("Cliente Demo", new Email("cliente@plataforma.com"), "hash-seguro-cliente-2026");
        customer.ConfirmarCorreoElectronico();
        FakeAuthenticationService authenticationService = new();
        LoginModel pageModel = CreatePageModel(customer, authenticationService);
        pageModel.Input.Email = "cliente@plataforma.com";
        pageModel.Input.Password = "Password#2026";
        pageModel.ReturnUrl = "/Orders/Index";

        var result = await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<Microsoft.AspNetCore.Mvc.LocalRedirectResult>());
    }

    [Test]
    public async Task OnPostAsync_ReturnUrlExterna_IgnoraDestinoNoConfiable()
    {
        Cliente customer = new("Cliente Demo", new Email("cliente@plataforma.com"), "hash-seguro-cliente-2026");
        customer.ConfirmarCorreoElectronico();
        FakeAuthenticationService authenticationService = new();
        LoginModel pageModel = CreatePageModel(customer, authenticationService);
        pageModel.Input.Email = "cliente@plataforma.com";
        pageModel.Input.Password = "Password#2026";
        pageModel.ReturnUrl = "https://malicioso.example/callback";

        var result = await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<Microsoft.AspNetCore.Mvc.RedirectToPageResult>());
        Assert.That(((Microsoft.AspNetCore.Mvc.RedirectToPageResult)result).PageName, Is.EqualTo("/Index"));
    }

    [Test]
    public async Task OnPostAsync_CredencialesInvalidas_NoEmiteCookie()
    {
        Cliente customer = new("Cliente Demo", new Email("cliente@plataforma.com"), "hash-seguro-cliente-2026");
        customer.ConfirmarCorreoElectronico();
        FakeAuthenticationService authenticationService = new();
        LoginModel pageModel = CreatePageModel(customer, authenticationService);
        pageModel.Input.Email = "cliente@plataforma.com";
        pageModel.Input.Password = "Password-invalido";

        var result = await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(authenticationService.SignedInPrincipal, Is.Null);
        Assert.That(pageModel.ErrorMessage, Does.Contain("no son válidos"));
    }

    private static LoginModel CreatePageModel(Usuario user, FakeAuthenticationService authenticationService)
    {
        FakeAuthApplicationService authApplicationService = new(user);
        return CreatePageModel(authApplicationService, authenticationService);
    }

    private static LoginModel CreatePageModel(FakeAuthApplicationService authApplicationService, FakeAuthenticationService authenticationService)
    {
        LoginModel pageModel = new(authApplicationService, NullLogger<LoginModel>.Instance);

        ServiceCollection services = new();
        services.AddSingleton<IAuthenticationService>(authenticationService);
        DefaultHttpContext httpContext = new()
        {
            RequestServices = services.BuildServiceProvider()
        };

        pageModel.PageContext = new Microsoft.AspNetCore.Mvc.RazorPages.PageContext
        {
            HttpContext = httpContext
        };
        pageModel.Url = new FakeUrlHelper();

        return pageModel;
    }

    private sealed class FakeUrlHelper : IUrlHelper
    {
        public Microsoft.AspNetCore.Mvc.ActionContext ActionContext { get; } = new();

        public string? Action(Microsoft.AspNetCore.Mvc.Routing.UrlActionContext actionContext) => null;
        public string? Content(string? contentPath) => contentPath;
        public bool IsLocalUrl(string? url) => !string.IsNullOrWhiteSpace(url) && url.StartsWith("/", StringComparison.Ordinal) && !url.StartsWith("//", StringComparison.Ordinal);
        public string? Link(string? routeName, object? values) => null;
        public string? RouteUrl(UrlRouteContext routeContext) => null;
    }

    private sealed class FakeAuthApplicationService : IAuthApplicationService
    {
        private readonly Usuario _user;

        public CurrentUserDto? LoginUserOverride { get; init; }

        public FakeAuthApplicationService(Usuario user)
        {
            _user = user;
        }

        public Task<Result<AuthResponseDto>> LoginAsync(LoginCommand command, CancellationToken cancellationToken = default)
        {
            if (!string.Equals(command.Email, _user.CorreoElectronico.Value, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(command.Password, "Password#2026", StringComparison.Ordinal))
            {
                return Task.FromResult(Result.Failure<AuthResponseDto>(
                    Error.Unauthorized("Auth.InvalidCredentials", "Las credenciales suministradas no son válidas.")));
            }

            CurrentUserDto currentUser = LoginUserOverride ?? new CurrentUserDto
            {
                Id = _user.Id,
                UserName = _user.CorreoElectronico.Value,
                Email = _user.CorreoElectronico.Value,
                FullName = _user.Nombre,
                IsActive = _user.Activo,
                IsEmailConfirmed = _user.CorreoConfirmado,
                Role = _user.Rol.ToString(),
                Roles = _user is Administrador { EsSuperUsuario: true }
                    ? [RolUsuario.SuperUsuario.ToString(), RolUsuario.Administrador.ToString()]
                    : [_user.Rol.ToString()],
                Area = _user is Administrador administrativeUser ? administrativeUser.Area : null,
                IsSuperUser = _user is Administrador { EsSuperUsuario: true },
                Permissions = _user is Administrador administrator
                    ? new[] { administrator.Area }
                    : Array.Empty<string>()
            };

            AuthResponseDto response = new()
            {
                AccessToken = "token-prueba",
                RefreshToken = "refresh-prueba",
                ExpiresAtUtc = DateTime.UtcNow.AddHours(1),
                ExpiresInSeconds = 3600,
                User = currentUser,
                IssuedAtUtc = DateTime.UtcNow,
                IsPersistentSession = command.RememberMe
            };

            return Task.FromResult(Result.Success(response));
        }

        public Task<Result<CurrentUserDto>> GetCurrentUserAsync(GetCurrentUserQuery query, CancellationToken cancellationToken = default)
        {
            CurrentUserDto currentUser = new()
            {
                Id = _user.Id,
                UserName = _user.CorreoElectronico.Value,
                Email = _user.CorreoElectronico.Value,
                FullName = _user.Nombre,
                IsActive = _user.Activo,
                IsEmailConfirmed = _user.CorreoConfirmado,
                Role = _user.Rol.ToString(),
                Roles = new[] { _user.Rol.ToString() }
            };

            return Task.FromResult(Result.Success(currentUser));
        }

        public Task<Result<PasswordResetRequestResultDto>> RequestPasswordResetAsync(RequestPasswordResetCommand command, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result.Success(new PasswordResetRequestResultDto()));
        }

        public Task<Result> ChangePasswordAsync(ChangePasswordCommand command, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result.Success());
        }

        public Task<Result> ResetPasswordAsync(ResetPasswordCommand command, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result.Success());
        }
    }

    private sealed class FakeAuthenticationService : IAuthenticationService
    {
        public ClaimsPrincipal? SignedInPrincipal { get; private set; }
        public string? LastSignInScheme { get; private set; }

        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme)
            => Task.FromResult(AuthenticateResult.NoResult());

        public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties)
        {
            if (!string.Equals(scheme, AuthorizationPolicies.AdminCookieScheme, StringComparison.Ordinal)
                && !string.Equals(scheme, AuthorizationPolicies.CustomerCookieScheme, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("El esquema de autenticación no corresponde a uno de los esquemas esperados de la aplicación.");
            }

            LastSignInScheme = scheme;
            SignedInPrincipal = principal;
            return Task.CompletedTask;
        }

        public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;
    }
}
