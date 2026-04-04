using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Users.Commands;
using PlataformaECommerce.Application.Interfaces.Services.Users;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Web.Pages.Auth;

/// <summary>
/// Gestiona la solicitud pública de reenvío del correo de confirmación de cuenta.
/// </summary>
[AllowAnonymous]
[EnableRateLimiting(WebRateLimitingOptions.AuthFlowPolicyName)]
public sealed class ResendEmailConfirmationModel : PageModel
{
    private readonly IUserApplicationService _userApplicationService;
    private readonly LinkGenerator _linkGenerator;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="ResendEmailConfirmationModel"/>.
    /// </summary>
    public ResendEmailConfirmationModel(IUserApplicationService userApplicationService, LinkGenerator linkGenerator)
    {
        _userApplicationService = userApplicationService ?? throw new ArgumentNullException(nameof(userApplicationService));
        _linkGenerator = linkGenerator ?? throw new ArgumentNullException(nameof(linkGenerator));
    }

    /// <summary>
    /// Modelo de entrada del formulario.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// Mensaje funcional de error de la página.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Mensaje temporal de éxito mostrado tras el reenvío.
    /// </summary>
    [TempData]
    public string? StatusMessage { get; set; }

    /// <summary>
    /// Inicializa la página con un correo precargado cuando se suministra por query string.
    /// </summary>
    public void OnGet(string? email)
    {
        if (!string.IsNullOrWhiteSpace(email))
        {
            Input.Email = email.Trim();
        }
    }

    /// <summary>
    /// Procesa la solicitud de reenvío y permanece en el mismo flujo con PRG.
    /// </summary>
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        Result result = await _userApplicationService.ResendUserEmailConfirmationAsync(new ResendUserEmailConfirmationCommand
        {
            Email = Input.Email,
            EmailConfirmationUrl = BuildEmailConfirmationUrl(),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            Source = "Web.Auth.ResendEmailConfirmation",
            ExternalReference = "Web.Auth.ResendEmailConfirmation"
        }, cancellationToken);

        if (result.IsFailure)
        {
            ErrorMessage = result.Error.Message;
            return Page();
        }

        StatusMessage = "Si la cuenta existe y aún no ha confirmado su correo, se envió un nuevo enlace de activación.";
        return RedirectToPage("/Auth/ResendEmailConfirmation", new { email = Input.Email });
    }

    private string BuildEmailConfirmationUrl()
    {
        return _linkGenerator.GetUriByPage(
            HttpContext,
            page: "/Auth/ConfirmEmail",
            handler: null,
            values: new { userId = "{userId}", token = "{token}" },
            scheme: Request.Scheme) ?? string.Empty;
    }

    /// <summary>
    /// Modelo de entrada del formulario de reenvío.
    /// </summary>
    public sealed class InputModel
    {
        /// <summary>
        /// Correo electrónico asociado a la cuenta.
        /// </summary>
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]
        public string Email { get; set; } = string.Empty;
    }
}
