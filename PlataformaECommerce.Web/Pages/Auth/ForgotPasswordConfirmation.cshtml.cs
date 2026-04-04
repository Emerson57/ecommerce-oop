using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PlataformaECommerce.Web.Pages.Auth;

/// <summary>
/// Muestra la confirmación posterior a solicitar la recuperación de contraseña.
/// </summary>
[AllowAnonymous]
public sealed class ForgotPasswordConfirmationModel : PageModel
{
    /// <summary>
    /// Mensaje funcional del flujo de recuperación.
    /// </summary>
    [TempData]
    public string? StatusMessage { get; set; }

    /// <summary>
    /// Enlace temporal visible únicamente en desarrollo.
    /// </summary>
    [TempData]
    public string? DevelopmentResetUrl { get; set; }

    /// <summary>
    /// Inicializa la página de confirmación.
    /// </summary>
    public void OnGet()
    {
    }
}
