using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
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
    [EnableRateLimiting(WebRateLimitingOptions.AuthFlowPolicyName)]
    public sealed class LoginModel : PageModel
    {
        private readonly IAuthApplicationService _authApplicationService;
        private readonly ITenantContextAccessor _tenantContextAccessor;
        private readonly ILogger<LoginModel> _logger;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="LoginModel"/>.
        /// </summary>
        /// <param name="authApplicationService">Servicio de aplicación de autenticación.</param>
        /// <param name="tenantContextAccessor">Accesor al tenant activo para emitir una identidad acotada al contexto resuelto.</param>
        /// <param name="logger">Registrador estructurado del flujo de autenticación web.</param>
        public LoginModel(
            IAuthApplicationService authApplicationService,
            ITenantContextAccessor tenantContextAccessor,
            ILogger<LoginModel> logger)
        {
            _authApplicationService = authApplicationService ?? throw new ArgumentNullException(nameof(authApplicationService));
            _tenantContextAccessor = tenantContextAccessor ?? throw new ArgumentNullException(nameof(tenantContextAccessor));
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

            if (!TryBuildAuthenticatedSession(result.Value.User, _tenantContextAccessor.TenantId, out ClaimsPrincipal? principal, out string authenticationScheme, out string redirectPage))
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

            LogAuthenticationSuccess(result.Value.User, authenticationScheme, redirectPage);

            AuthenticationProperties authenticationProperties = CreateAuthenticationProperties();

            await HttpContext.SignOutAsync(AuthorizationPolicies.AdminCookieScheme);
            await HttpContext.SignOutAsync(AuthorizationPolicies.CustomerCookieScheme);

            await HttpContext.SignInAsync(
                authenticationScheme,
                principal!,
                authenticationProperties);

            if (!string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            {
                return LocalRedirect(ReturnUrl);
            }

            return RedirectToPage(redirectPage);
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

        private AuthenticationProperties CreateAuthenticationProperties()
        {
            DateTimeOffset issuedAtUtc = DateTimeOffset.UtcNow;

            return new AuthenticationProperties
            {
                IsPersistent = Input.RememberMe,
                AllowRefresh = true,
                IssuedUtc = issuedAtUtc,
                ExpiresUtc = issuedAtUtc.AddHours(Input.RememberMe ? 24 : 8)
            };
        }

        private static bool TryBuildAuthenticatedSession(
            CurrentUserDto user,
            string tenantId,
            out ClaimsPrincipal? principal,
            out string authenticationScheme,
            out string redirectPage)
        {
            ArgumentNullException.ThrowIfNull(user);

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                principal = null;
                authenticationScheme = string.Empty;
                redirectPage = string.Empty;
                return false;
            }

            if (TryBuildAdministrativePrincipal(user, tenantId, out principal))
            {
                authenticationScheme = AuthorizationPolicies.AdminCookieScheme;
                redirectPage = "/Admin/Index";
                return true;
            }

            if (TryBuildCustomerPrincipal(user, tenantId, out principal))
            {
                authenticationScheme = AuthorizationPolicies.CustomerCookieScheme;
                redirectPage = "/Index";
                return true;
            }

            authenticationScheme = string.Empty;
            redirectPage = string.Empty;
            principal = null;
            return false;
        }

        private static bool TryBuildAdministrativePrincipal(CurrentUserDto user, string tenantId, out ClaimsPrincipal? principal)
        {
            ArgumentNullException.ThrowIfNull(user);

            principal = null;

            if (!TryResolvePrimaryAdministrativeRole(user, out RolUsuario primaryRole))
            {
                return false;
            }

            List<string> roles = user.Roles
                .Where(RolUsuarioExtensions.EsValorDeRolAdministrativo)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            foreach (string effectiveRole in primaryRole.ObtenerRolesEfectivos())
            {
                if (!roles.Contains(effectiveRole, StringComparer.Ordinal))
                {
                    roles.Add(effectiveRole);
                }
            }

            List<Claim> claims =
            [
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.DisplayName),
                new(ClaimTypes.Email, user.Email),
                new(SecurityClaimTypes.TenantId, tenantId.Trim()),
                new(SecurityClaimTypes.PrimaryRole, primaryRole.ToString()),
                new(SecurityClaimTypes.AdminArea, user.Area ?? "Operaciones"),
                new(SecurityClaimTypes.IsSuperUser, user.IsSuperUser.ToString())
            ];

            foreach (string role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            ClaimsIdentity identity = new(claims, AuthorizationPolicies.AdminCookieScheme);
            principal = new ClaimsPrincipal(identity);
            return true;
        }

        private static bool TryBuildCustomerPrincipal(CurrentUserDto user, string tenantId, out ClaimsPrincipal? principal)
        {
            ArgumentNullException.ThrowIfNull(user);

            principal = null;

            if (!TryResolveCustomerRole(user, out RolUsuario primaryRole))
            {
                return false;
            }

            List<Claim> claims =
            [
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.DisplayName),
                new(ClaimTypes.Email, user.Email),
                new(SecurityClaimTypes.TenantId, tenantId.Trim()),
                new(SecurityClaimTypes.PrimaryRole, primaryRole.ToString()),
                new(SecurityClaimTypes.IsSuperUser, bool.FalseString),
                new(ClaimTypes.Role, primaryRole.ToString())
            ];

            ClaimsIdentity identity = new(claims, AuthorizationPolicies.CustomerCookieScheme);
            principal = new ClaimsPrincipal(identity);
            return true;
        }

        private static bool TryResolvePrimaryAdministrativeRole(CurrentUserDto user, out RolUsuario primaryRole)
        {
            if (user.IsSuperUser)
            {
                primaryRole = RolUsuario.SuperUsuario;
                return string.Equals(user.Role, RolUsuario.SuperUsuario.ToString(), StringComparison.Ordinal)
                    || user.Roles.Any(role => string.Equals(role, RolUsuario.SuperUsuario.ToString(), StringComparison.Ordinal));
            }

            if (Enum.TryParse(user.Role, ignoreCase: true, out RolUsuario parsedRole)
                && parsedRole.EsAdministrativo())
            {
                if (parsedRole == RolUsuario.SuperUsuario)
                {
                    primaryRole = default;
                    return false;
                }

                primaryRole = parsedRole;
                return true;
            }

            if (AuthorizationPolicies.IsAdministrativeUser(user.Roles))
            {
                if (user.Roles.Any(role => string.Equals(role, RolUsuario.SuperUsuario.ToString(), StringComparison.Ordinal)))
                {
                    primaryRole = default;
                    return false;
                }

                primaryRole = RolUsuario.Administrador;
                return true;
            }

            primaryRole = default;
            return false;
        }

        private static bool TryResolveCustomerRole(CurrentUserDto user, out RolUsuario primaryRole)
        {
            ArgumentNullException.ThrowIfNull(user);

            if (user.IsSuperUser)
            {
                primaryRole = default;
                return false;
            }

            if (Enum.TryParse(user.Role, ignoreCase: true, out RolUsuario parsedRole)
                && parsedRole == RolUsuario.Cliente)
            {
                primaryRole = parsedRole;
                return user.Roles.Count == 0 || user.Roles.All(role => string.Equals(role, RolUsuario.Cliente.ToString(), StringComparison.Ordinal));
            }

            bool hasOnlyCustomerRoles = user.Roles.Count > 0 && user.Roles.All(role => string.Equals(role, RolUsuario.Cliente.ToString(), StringComparison.Ordinal));
            if (hasOnlyCustomerRoles)
            {
                primaryRole = RolUsuario.Cliente;
                return true;
            }

            primaryRole = default;
            return false;
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
