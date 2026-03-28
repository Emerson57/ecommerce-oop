using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PlataformaECommerce.Web.Pages.Auth;

/// <summary>
/// Presenta el resultado genérico del inicio del flujo de recuperación de contraseña.
/// </summary>
[AllowAnonymous]
public sealed class ForgotPasswordConfirmationModel : PageModel
{
    private readonly IWebHostEnvironment _environment;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="ForgotPasswordConfirmationModel"/>.
    /// </summary>
    /// <param name="environment">Entorno de ejecución actual.</param>
    public ForgotPasswordConfirmationModel(IWebHostEnvironment environment)
    {
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    /// <summary>
    /// Mensaje temporal de confirmación del flujo.
    /// </summary>
    [TempData]
    public string? StatusMessage { get; set; }

    /// <summary>
    /// Enlace temporal disponible únicamente en entornos controlados.
    /// </summary>
    [TempData]
    public string? DevelopmentResetUrl { get; set; }

    /// <summary>
    /// Obtiene el enlace temporal visible únicamente en entornos controlados.
    /// </summary>
    public string? VisibleDevelopmentResetUrl =>
        _environment.IsDevelopment()
            ? DevelopmentResetUrl
            : null;

    /// <summary>
    /// Inicializa la página de confirmación.
    /// </summary>
    public void OnGet()
    {
    }
}