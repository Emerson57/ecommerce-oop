using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PlataformaECommerce.Web.Pages.Auth;

/// <summary>
/// Muestra la confirmación posterior al registro público de la cuenta.
/// </summary>
[AllowAnonymous]
public sealed class RegisterConfirmationModel : PageModel
{
    /// <summary>
    /// Mensaje funcional del flujo de registro.
    /// </summary>
    [TempData]
    public string? StatusMessage { get; set; }

    /// <summary>
    /// Correo asociado al registro reciente, cuando esté disponible.
    /// </summary>
    [TempData]
    public string? RegisteredEmail { get; set; }

    /// <summary>
    /// Inicializa la página de confirmación del registro.
    /// </summary>
    public void OnGet()
    {
    }
}
