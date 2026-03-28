using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlataformaECommerce.Application.Common.Security;
using PlataformaECommerce.Application.Features.Auth.Commands;
using PlataformaECommerce.Application.Interfaces.Services.Auth;

namespace PlataformaECommerce.Web.Pages.Auth;

/// <summary>
/// Gestiona el restablecimiento interactivo de la contraseña a partir de un token temporal.
/// </summary>
[AllowAnonymous]
public sealed class ResetPasswordModel : PageModel
{
    private const string ResetPasswordSource = "Web.Auth.ResetPassword";
    private readonly IAuthApplicationService _authApplicationService;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="ResetPasswordModel"/>.
    /// </summary>
    /// <param name="authApplicationService">Servicio de aplicación de autenticación.</param>
    public ResetPasswordModel(IAuthApplicationService authApplicationService)
    {
        _authApplicationService = authApplicationService ?? throw new ArgumentNullException(nameof(authApplicationService));
    }

    /// <summary>
    /// Identificador del usuario contenido en el enlace temporal.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public Guid UserId { get; set; }

    /// <summary>
    /// Token temporal contenido en el enlace de recuperación.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Modelo de entrada del formulario de restablecimiento.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// Mensaje funcional de error asociado al flujo.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Mensaje temporal de éxito publicado tras completar el restablecimiento.
    /// </summary>
    [TempData]
    public string? StatusMessage { get; set; }

    /// <summary>
    /// Indica si la página posee el contexto mínimo requerido para procesar el restablecimiento.
    /// </summary>
    public bool HasRecoveryContext => UserId != Guid.Empty && !string.IsNullOrWhiteSpace(Token);

    /// <summary>
    /// Inicializa la página a partir del enlace temporal recibido.
    /// </summary>
    public void OnGet()
    {
        if (HasRecoveryContext)
        {
            return;
        }

        ErrorMessage = "El enlace de recuperación no es válido o está incompleto. Solicita uno nuevo para continuar.";
    }

    /// <summary>
    /// Procesa el restablecimiento de contraseña.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Resultado de navegación del flujo.</returns>
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!HasRecoveryContext)
        {
            ErrorMessage = "El enlace de recuperación no es válido o está incompleto. Solicita uno nuevo para continuar.";
            return Page();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _authApplicationService.ResetPasswordAsync(new ResetPasswordCommand
        {
            UserId = UserId,
            Token = Token.Trim(),
            NewPassword = Input.NewPassword,
            ConfirmPassword = Input.ConfirmPassword,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString(),
            Source = ResetPasswordSource,
            ExternalReference = ResetPasswordSource,
            RequestedAtUtc = DateTime.UtcNow
        }, cancellationToken);

        if (result.IsFailure)
        {
            ErrorMessage = result.Error.Message;
            return Page();
        }

        StatusMessage = "La contraseña fue restablecida correctamente. Ahora puedes iniciar sesión con tu nueva credencial.";
        return RedirectToPage("/Auth/Login");
    }

    /// <summary>
    /// Captura la nueva credencial solicitada para el flujo de restablecimiento.
    /// </summary>
    public sealed class InputModel
    {
        /// <summary>
        /// Nueva contraseña propuesta por el usuario.
        /// </summary>
        [Display(Name = "Nueva contraseña")]
        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [StringLength(PasswordPolicyRules.MaxLength, MinimumLength = PasswordPolicyRules.MinLength, ErrorMessage = "La nueva contraseña debe tener entre 8 y 100 caracteres.")]
        public string NewPassword { get; set; } = string.Empty;

        /// <summary>
        /// Confirmación de la nueva contraseña.
        /// </summary>
        [Display(Name = "Confirmar nueva contraseña")]
        [Required(ErrorMessage = "La confirmación de la nueva contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "La confirmación de la nueva contraseña no coincide con la contraseña ingresada.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}