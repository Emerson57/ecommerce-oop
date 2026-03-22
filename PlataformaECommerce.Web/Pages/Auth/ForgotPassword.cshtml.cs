using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlataformaECommerce.Application.Features.Auth.Commands;
using PlataformaECommerce.Application.Interfaces.Services.Auth;

namespace PlataformaECommerce.Web.Pages.Auth;

/// <summary>
/// Gestiona la solicitud interactiva de recuperación de contraseña para el acceso administrativo.
/// </summary>
[AllowAnonymous]
public sealed class ForgotPasswordModel : PageModel
{
    private readonly IAuthApplicationService _authApplicationService;
    private readonly IWebHostEnvironment _environment;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="ForgotPasswordModel"/>.
    /// </summary>
    public ForgotPasswordModel(IAuthApplicationService authApplicationService, IWebHostEnvironment environment)
    {
        _authApplicationService = authApplicationService ?? throw new ArgumentNullException(nameof(authApplicationService));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    /// <summary>
    /// Modelo de entrada del formulario de recuperación.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// Mensaje funcional de error de la página.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Mensaje temporal de éxito mostrado en la página de confirmación.
    /// </summary>
    [TempData]
    public string? StatusMessage { get; set; }

    /// <summary>
    /// Enlace temporal de recuperación mostrado únicamente en desarrollo.
    /// </summary>
    [TempData]
    public string? DevelopmentResetUrl { get; set; }

    /// <summary>
    /// Inicializa la página.
    /// </summary>
    public void OnGet()
    {
    }

    /// <summary>
    /// Procesa la solicitud de recuperación y redirige a la confirmación.
    /// </summary>
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _authApplicationService.RequestPasswordResetAsync(new RequestPasswordResetCommand
        {
            Email = Input.Email,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString(),
            Source = "Web.Auth.ForgotPassword",
            ExternalReference = "Web.Auth.ForgotPassword",
            RequestedAtUtc = DateTime.UtcNow
        }, cancellationToken);

        if (result.IsFailure)
        {
            ErrorMessage = result.Error.Message;
            return Page();
        }

        StatusMessage = "Si la cuenta existe y está habilitada, se generó un enlace temporal para restablecer la contraseña.";
        DevelopmentResetUrl = BuildDevelopmentResetUrl(result.Value);
        return RedirectToPage("/Auth/ForgotPasswordConfirmation");
    }

    private string? BuildDevelopmentResetUrl(PlataformaECommerce.Application.Features.Auth.DTOs.PasswordResetRequestResultDto result)
    {
        if (!_environment.IsDevelopment() || !result.CanPreviewResetLink)
        {
            return null;
        }

        return Url.Page(
            "/Auth/ResetPassword",
            pageHandler: null,
            values: new { userId = result.UserId, token = result.ResetToken },
            protocol: Request.Scheme);
    }

    /// <summary>
    /// Modelo de entrada del formulario de recuperación.
    /// </summary>
    public sealed class InputModel
    {
        /// <summary>
        /// Correo electrónico de la cuenta administrativa.
        /// </summary>
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]
        public string Email { get; set; } = string.Empty;
    }
}
