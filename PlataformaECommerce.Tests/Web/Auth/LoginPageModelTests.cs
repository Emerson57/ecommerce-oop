using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Auth.Commands;
using PlataformaECommerce.Application.Features.Auth.DTOs;
using PlataformaECommerce.Application.Features.Auth.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Auth;
using PlataformaECommerce.Domain.Entities.Users;
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
    public async Task OnPostAsync_UsuarioNoAdministrador_RetornaPaginaConError()
    {
        Cliente customer = new("Cliente Demo", new Email("cliente@plataforma.com"), "hash-seguro-cliente-2026");
        customer.ConfirmarCorreoElectronico();
        FakeAuthenticationService authenticationService = new();
        LoginModel pageModel = CreatePageModel(customer, authenticationService);
        pageModel.Input.Email = "cliente@plataforma.com";
        pageModel.Input.Password = "Password#2026";

        var result = await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.InstanceOf<PageResult>());
        Assert.That(pageModel.ErrorMessage, Is.Not.Null.And.Not.Empty);
    }

    private static LoginModel CreatePageModel(Usuario user, FakeAuthenticationService authenticationService)
    {
        FakeAuthApplicationService authApplicationService = new(user);
        LoginModel pageModel = new(authApplicationService);

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

        return pageModel;
    }

    private sealed class FakeAuthApplicationService : IAuthApplicationService
    {
        private readonly Usuario _user;

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

            CurrentUserDto currentUser = new()
            {
                Id = _user.Id,
                UserName = _user.CorreoElectronico.Value,
                Email = _user.CorreoElectronico.Value,
                FullName = _user.Nombre,
                IsActive = _user.Activo,
                IsEmailConfirmed = _user.CorreoConfirmado,
                Role = _user.Rol.ToString(),
                Roles = new[] { _user.Rol.ToString() },
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
    }

    private sealed class FakeAuthenticationService : IAuthenticationService
    {
        public ClaimsPrincipal? SignedInPrincipal { get; private set; }

        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme)
            => Task.FromResult(AuthenticateResult.NoResult());

        public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties)
        {
            if (!string.Equals(scheme, AuthorizationPolicies.AdminCookieScheme, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("El esquema de autenticación no corresponde al esperado para administración.");
            }

            SignedInPrincipal = principal;
            return Task.CompletedTask;
        }

        public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;
    }
}
