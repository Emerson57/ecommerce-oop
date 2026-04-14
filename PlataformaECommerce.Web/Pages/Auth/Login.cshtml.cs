using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Application.Common.Security;
using PlataformaECommerce.Application.Features.Auth.Commands;
using PlataformaECommerce.Application.Features.Auth.DTOs;
using PlataformaECommerce.Application.Interfaces.Services.Auth;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Web.Authorization;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Web.Pages.Auth
{
    /// <summary>
    /// Proporciona el flujo interactivo de inicio de sesión por correo electrónico para Razor Pages.
    /// </summary>
    /// <remarks>
    /// Esta página captura credenciales de acceso basadas en correo electrónico y delega la autenticación
    /// a <c>Application</c>, emitiendo posteriormente la cookie apropiada según el tipo de cuenta autenticada.
    /// </remarks>
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingOptions.AuthenticationPolicyName)]
    public sealed class LoginModel : PageModel
    {
        private readonly IAuthApplicationService _authApplicationService;
        private readonly ITenantContextAccessor _tenantContextAccessor;
        private readonly WebAuthenticationCookiesOptions _authenticationCookiesOptions;
        private readonly ILogger<LoginModel> _logger;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="LoginModel"/>.
        /// </summary>
        /// <param name="authApplicationService">Servicio de aplicación de autenticación.</param>
        /// <param name="tenantContextAccessor">Accesor al tenant activo para emitir una identidad acotada al contexto resuelto.</param>
        /// <param name="authenticationCookiesOptions">Opciones endurecidas de expiración y persistencia de cookies autenticadas.</param>
        /// <param name="logger">Registrador estructurado del flujo de autenticación web.</param>
        public LoginModel(
            IAuthApplicationService authApplicationService,
            ITenantContextAccessor tenantContextAccessor,
            IOptions<WebAuthenticationCookiesOptions> authenticationCookiesOptions,
            ILogger<LoginModel> logger)
        {
            ArgumentNullException.ThrowIfNull(authenticationCookiesOptions);

            _authApplicationService = authApplicationService ?? throw new ArgumentNullException(nameof(authApplicationService));
            _tenantContextAccessor = tenantContextAccessor ?? throw new ArgumentNullException(nameof(tenantContextAccessor));
            _authenticationCookiesOptions = authenticationCookiesOptions.Value;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtiene o establece el modelo de entrada del formulario de autenticación.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; } = new();

        /// <summary>
        /// Obtiene o establece la URL local de retorno posterior al inicio de sesión.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        /// <summary>
        /// Obtiene o establece un mensaje general de autenticación cuando la operación falla.
        /// </summary>
        public string? ErrorMessage { get; private set; }

        /// <summary>
        /// Obtiene o establece el mensaje funcional mostrado tras un restablecimiento exitoso.
        /// </summary>
        [TempData]
        public string? StatusMessage { get; set; }

        /// <summary>
        /// Indica si la interfaz debe ofrecer el reenvío de confirmación de correo.
        /// </summary>
        public bool CanResendEmailConfirmation { get; private set; }

        /// <summary>
        /// Correo electrónico asociado a una cuenta pendiente de confirmación.
        /// </summary>
        public string? PendingEmailConfirmationAddress { get; private set; }

        /// <summary>
        /// Inicializa la página de autenticación.
        /// </summary>
        public void OnGet()
        {
        }

        /// <summary>
        /// Procesa la autenticación y emite una cookie de sesión acorde al tipo de cuenta autenticada.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
        /// <returns>Resultado de navegación correspondiente al flujo de autenticación.</returns>
        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _authApplicationService.LoginAsync(
                CreateLoginCommand(),
                cancellationToken);

            if (result.IsFailure)
            {
                _logger.LogWarning(
                    "Se rechazó un intento de autenticación por correo. Email: {Email}. RemoteIp: {RemoteIp}. ErrorCode: {ErrorCode}",
                    Input.Email,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    result.Error.Code);

                CanResendEmailConfirmation = string.Equals(result.Error.Code, "Auth.EmailNotConfirmed", StringComparison.Ordinal);
                PendingEmailConfirmationAddress = CanResendEmailConfirmation ? Input.Email?.Trim() : null;
                ErrorMessage = "El correo electrónico o la contraseña no son válidos, o la cuenta no está habilitada.";
                return Page();
            }

            if (!AuthenticatedSessionFactory.TryCreate(result.Value.User, _tenantContextAccessor.TenantId, out AuthenticatedSession? authenticatedSession))
            {
                _logger.LogWarning(
                    "Se rechazó la emisión de sesión autenticada por inconsistencia de identidad. UserId: {UserId}. Email: {Email}. Role: {Role}. IsSuperUser: {IsSuperUser}",
                    result.Value.User.Id,
                    result.Value.User.Email,
                    result.Value.User.Role,
                    result.Value.User.IsSuperUser);

                ErrorMessage = "La cuenta autenticada no cumple los requisitos de seguridad y acceso requeridos para esta sección.";
                return Page();
            }

            LogAuthenticationSuccess(result.Value.User, authenticatedSession.AuthenticationScheme, authenticatedSession.RedirectPage);

            AuthenticationProperties authenticationProperties = CreateAuthenticationProperties(authenticatedSession.AuthenticationScheme);

            await HttpContext.SignOutAsync(AuthorizationPolicies.AdminCookieScheme);
            await HttpContext.SignOutAsync(AuthorizationPolicies.CustomerCookieScheme);

            await HttpContext.SignInAsync(
                authenticatedSession.AuthenticationScheme,
                authenticatedSession.Principal,
                authenticationProperties);

            if (!string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            {
                return LocalRedirect(ReturnUrl);
            }

            return RedirectToPage(authenticatedSession.RedirectPage);
        }

        private void LogAuthenticationSuccess(CurrentUserDto user, string authenticationScheme, string redirectPage)
        {
            ArgumentNullException.ThrowIfNull(user);

            if (string.Equals(authenticationScheme, AuthorizationPolicies.AdminCookieScheme, StringComparison.Ordinal))
            {
                _logger.LogInformation(
                    "Se concedió acceso privilegiado al backoffice. UserId: {UserId}. Email: {Email}. Role: {Role}. IsSuperUser: {IsSuperUser}. RemoteIp: {RemoteIp}. RedirectPage: {RedirectPage}",
                    user.Id,
                    user.Email,
                    user.Role,
                    user.IsSuperUser,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    redirectPage);

                return;
            }

            _logger.LogInformation(
                "Se concedió acceso autenticado al sitio público. UserId: {UserId}. Email: {Email}. Role: {Role}. RemoteIp: {RemoteIp}. RedirectPage: {RedirectPage}",
                user.Id,
                user.Email,
                user.Role,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                redirectPage);
        }

        private LoginCommand CreateLoginCommand()
        {
            return new LoginCommand
            {
                Email = Input.Email,
                Password = Input.Password,
                RememberMe = Input.RememberMe,
                ExternalReference = "Web.Auth.Login"
            };
        }

        private AuthenticationProperties CreateAuthenticationProperties(string authenticationScheme)
        {
            WebAuthenticationCookieProfileOptions cookieProfile = AuthorizationPolicies.GetCookieProfile(authenticationScheme, _authenticationCookiesOptions);
            return CookieAuthenticationSessionProperties.Create(
                cookieProfile,
                Input.RememberMe,
                _authenticationCookiesOptions.SlidingExpiration);
        }

        /// <summary>
        /// Representa el modelo de entrada del formulario de autenticación.
        /// </summary>
        public sealed class InputModel
        {
            /// <summary>
            /// Obtiene o establece el correo electrónico del usuario.
            /// </summary>
            [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
            [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]
            public string Email { get; set; } = string.Empty;

            /// <summary>
            /// Obtiene o establece la contraseña del usuario.
            /// </summary>
            [Required(ErrorMessage = "La contraseña es obligatoria.")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            /// <summary>
            /// Obtiene o establece un valor que indica si la sesión debe persistirse.
            /// </summary>
            public bool RememberMe { get; set; }
        }
    }
}
