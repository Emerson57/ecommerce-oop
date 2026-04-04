using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlataformaECommerce.Application.Features.Users.Commands;
using PlataformaECommerce.Application.Interfaces.Services.Users;

namespace PlataformaECommerce.Web.Pages.Auth;

/// <summary>
/// Procesa la confirmación de correo electrónico desde el enlace recibido por email.
/// </summary>
[AllowAnonymous]
public sealed class ConfirmEmailModel : PageModel
{
    private readonly IUserApplicationService _userApplicationService;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="ConfirmEmailModel"/>.
    /// </summary>
    public ConfirmEmailModel(IUserApplicationService userApplicationService)
    {
        _userApplicationService = userApplicationService ?? throw new ArgumentNullException(nameof(userApplicationService));
    }

    /// <summary>
    /// Indica si la confirmación fue exitosa.
    /// </summary>
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// Mensaje funcional mostrado al usuario.
    /// </summary>
    public string StatusMessage { get; private set; } = string.Empty;

    /// <summary>
    /// Procesa el enlace de confirmación del correo.
    /// </summary>
    public async Task<IActionResult> OnGetAsync(Guid userId, string? token, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty || string.IsNullOrWhiteSpace(token))
        {
            IsSuccess = false;
            StatusMessage = "El enlace de confirmación no es válido o no contiene la información requerida.";
            return Page();
        }

        var result = await _userApplicationService.ConfirmUserEmailAsync(new ConfirmUserEmailCommand
        {
            UserId = userId,
            ConfirmationToken = token.Trim(),
            RequestedByUserId = userId,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            Source = "Web.Auth.ConfirmEmail",
            ExternalReference = "Web.Auth.ConfirmEmail"
        }, cancellationToken);

        IsSuccess = result.IsSuccess;
        StatusMessage = result.IsSuccess
            ? "Tu correo electrónico fue confirmado correctamente. Ya puedes iniciar sesión con tu cuenta."
            : result.Error.Message;

        return Page();
    }
}
