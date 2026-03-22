using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlataformaECommerce.Web.Authorization;

namespace PlataformaECommerce.Web.Pages.Auth;

/// <summary>
/// Gestiona el cierre de sesión de usuarios autenticados basado en cookies.
/// </summary>
/// <remarks>
/// Esta página invalida cualquier cookie autenticada activa y redirige al usuario
/// hacia la página principal, restaurando un contexto público no autenticado.
/// </remarks>
[Authorize(AuthenticationSchemes = AuthorizationPolicies.AdminCookieScheme + "," + AuthorizationPolicies.CustomerCookieScheme)]
public sealed class LogoutModel : PageModel
{
    /// <summary>
    /// Cierra la sesión actual y redirige a la página principal.
    /// </summary>
    /// <returns>Resultado de navegación posterior al cierre de sesión.</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        await HttpContext.SignOutAsync(AuthorizationPolicies.AdminCookieScheme);
        await HttpContext.SignOutAsync(AuthorizationPolicies.CustomerCookieScheme);
        return RedirectToPage("/Index");
    }
}
