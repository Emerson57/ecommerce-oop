using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PlataformaECommerce.Web.Pages.Auth;

/// <summary>
/// Representa la página de acceso denegado para áreas protegidas del sistema.
/// </summary>
/// <remarks>
/// Esta página comunica de forma clara y profesional que el usuario actual no posee
/// permisos suficientes para acceder a recursos administrativos restringidos.
/// </remarks>
[AllowAnonymous]
public sealed class AccessDeniedModel : PageModel
{
    /// <summary>
    /// Inicializa la página informativa de acceso denegado.
    /// </summary>
    public void OnGet()
    {
    }
}
