using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlataformaECommerce.Application.Features.Auth.Commands;
using PlataformaECommerce.Application.Features.Auth.DTOs;
using PlataformaECommerce.Application.Interfaces.Services.Auth;
using PlataformaECommerce.Web.Authorization;

namespace PlataformaECommerce.Web.Pages.Auth
{
    /// <summary>
    /// Proporciona el flujo de autenticación administrativa interactiva para Razor Pages.
    /// </summary>
    /// <remarks>
    /// Esta página valida credenciales administrativas y emite una cookie autenticada
    /// destinada al backoffice, permitiendo acceso controlado a funcionalidades
    /// protegidas como la auditoría administrativa.
    /// </remarks>
    [AllowAnonymous]
    public sealed class LoginModel : PageModel
    {
        private readonly IAuthApplicationService _authApplicationService;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="LoginModel"/>.
        /// </summary>
        /// <param name="authApplicationService">Servicio de aplicación de autenticación.</param>
        public LoginModel(IAuthApplicationService authApplicationService)
        {
            _authApplicationService = authApplicationService ?? throw new ArgumentNullException(nameof(authApplicationService));
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
        /// Inicializa la página de autenticación administrativa.
        /// </summary>
        public void OnGet()
        {
        }

        /// <summary>
        /// Procesa la autenticación administrativa y emite una cookie de sesión para el backoffice.
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
                new LoginCommand
                {
                    Email = Input.Email,
                    Password = Input.Password,
                    RememberMe = Input.RememberMe,
                    ExternalReference = "Web.Auth.Login"
                },
                cancellationToken);

            if (result.IsFailure || !IsAdministrator(result.Value.User))
            {
                ErrorMessage = "Las credenciales administrativas suministradas no son válidas o el usuario no está habilitado.";
                return Page();
            }

            ClaimsPrincipal principal = BuildPrincipal(result.Value.User);
            AuthenticationProperties authenticationProperties = new()
            {
                IsPersistent = Input.RememberMe,
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(Input.RememberMe ? 24 : 8)
            };

            await HttpContext.SignInAsync(
                AuthorizationPolicies.AdminCookieScheme,
                principal,
                authenticationProperties);

            if (!string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            {
                return LocalRedirect(ReturnUrl);
            }

            return RedirectToPage("/Admin/Index");
        }

        private static bool IsAdministrator(CurrentUserDto user)
        {
            ArgumentNullException.ThrowIfNull(user);

            return string.Equals(user.Role, "Administrador", StringComparison.Ordinal)
                || user.Roles.Contains("Administrador", StringComparer.Ordinal);
        }

        private static ClaimsPrincipal BuildPrincipal(CurrentUserDto user)
        {
            ArgumentNullException.ThrowIfNull(user);

            string primaryRole = string.IsNullOrWhiteSpace(user.Role)
                ? "Administrador"
                : user.Role;

            Claim[] claims =
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.DisplayName),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, primaryRole),
                new("area", user.Permissions.FirstOrDefault() ?? "Operaciones"),
                new("user_type", "admin")
            };

            ClaimsIdentity identity = new(claims, AuthorizationPolicies.AdminCookieScheme);
            return new ClaimsPrincipal(identity);
        }

        /// <summary>
        /// Representa el modelo de entrada del formulario de autenticación administrativa.
        /// </summary>
        public sealed class InputModel
        {
            /// <summary>
            /// Obtiene o establece el correo electrónico del administrador.
            /// </summary>
            [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
            [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]
            public string Email { get; set; } = string.Empty;

            /// <summary>
            /// Obtiene o establece la contraseña del administrador.
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
