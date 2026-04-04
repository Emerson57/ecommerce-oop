using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using PlataformaECommerce.Application.Features.Auth.Commands;
using PlataformaECommerce.Application.Interfaces.Services.Auth;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Web.Pages.Auth;

/// <summary>
/// Gestiona el restablecimiento de contraseña mediante un enlace temporal.
/// </summary>
[AllowAnonymous]
[EnableRateLimiting(WebRateLimitingOptions.AuthFlowPolicyName)]
public sealed class ResetPasswordModel : PageModel
{
    private readonly IAuthApplicationService _authApplicationService;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="ResetPasswordModel"/>.
    /// </summary>
    public ResetPasswordModel(IAuthApplicationService authApplicationService)
    {
        _authApplicationService = authApplicationService ?? throw new ArgumentNullException(nameof(authApplicationService));
    }

    /// <summary>
    /// Captura la información necesaria para restablecer la contraseña.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// Mensaje funcional asociado al restablecimiento.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Mensaje temporal mostrado al volver al ingreso.
    /// </summary>
    [TempData]
    public string? StatusMessage { get; set; }

    /// <summary>
    /// Inicializa el formulario a partir del enlace recibido.
    /// </summary>
    public IActionResult OnGet(Guid userId, string? token)
    {
        if (userId == Guid.Empty || string.IsNullOrWhiteSpace(token))
        {
            StatusMessage = "El enlace de recuperación no es válido o no contiene la información necesaria.";
            return RedirectToPage("/Auth/Login");
        }

        Input.UserId = userId;
        Input.Token = token.Trim();
        return Page();
    }

    /// <summary>
    /// Procesa el restablecimiento de la contraseña.
    /// </summary>
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _authApplicationService.ResetPasswordAsync(new ResetPasswordCommand
        {
            UserId = Input.UserId,
            Token = Input.Token,
            NewPassword = Input.NewPassword,
            ConfirmPassword = Input.ConfirmPassword,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString(),
            Source = "Web.Auth.ResetPassword",
            ExternalReference = "Web.Auth.ResetPassword",
            RequestedAtUtc = DateTime.UtcNow
        }, cancellationToken);

        if (result.IsFailure)
        {
            ErrorMessage = result.Error.Message;
            return Page();
        }

        StatusMessage = "Tu contraseña fue restablecida correctamente. Ya puedes ingresar con la nueva credencial.";
        return RedirectToPage("/Auth/Login");
    }

    /// <summary>
    /// Modelo de entrada del restablecimiento de contraseña.
    /// </summary>
    public sealed class InputModel
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string Token { get; set; } = string.Empty;

        [Display(Name = "Nueva contraseña")]
        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Display(Name = "Confirmar contraseña")]
        [Required(ErrorMessage = "La confirmación de la nueva contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "La confirmación de la nueva contraseña no coincide con la contraseña ingresada.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
