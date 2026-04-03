using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using PlataformaECommerce.Application.Features.Admin;
using PlataformaECommerce.Application.Features.Admin.Commands;
using PlataformaECommerce.Application.Features.Admin.DTOs;
using PlataformaECommerce.Application.Features.Admin.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Admin;
using PlataformaECommerce.Web.Authorization;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Web.Pages.Admin.Users;

/// <summary>
/// Representa la página de creación segura de administradores del backoffice.
/// </summary>
/// <remarks>
/// Esta página captura únicamente los datos del alta administrativa, delegando validación estructural,
/// autorización, reglas de negocio, persistencia y auditoría a la capa Application.
/// </remarks>
[Authorize(
    Policy = AuthorizationPolicies.SuperUserOnly,
    AuthenticationSchemes = AuthorizationPolicies.AdminCookieScheme)]
[EnableRateLimiting(WebRateLimitingOptions.SensitiveApiPolicyName)]
public sealed class CreateModel : PageModel
{
    private readonly IAdminUserService _adminUserService;
    private readonly AdminUsersBackofficeOptions _options;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="CreateModel"/>.
    /// </summary>
    /// <param name="adminUserService">Servicio administrativo especializado en usuarios.</param>
    /// <param name="options">Opciones de disponibilidad del módulo de usuarios.</param>
    public CreateModel(
        IAdminUserService adminUserService,
        IOptions<AdminUsersBackofficeOptions> options)
    {
        _adminUserService = adminUserService ?? throw new ArgumentNullException(nameof(adminUserService));
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;
    }

    /// <summary>
    /// Obtiene la definición funcional del caso de uso de alta administrativa.
    /// </summary>
    public AdminRegistrationDefinitionDto Definition { get; private set; } = new();

    /// <summary>
    /// Obtiene el modelo de entrada asociado al formulario.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// Obtiene el mensaje de error funcional de la página cuando la consulta falla.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Obtiene o establece el mensaje funcional de éxito que será mostrado tras completar el alta.
    /// </summary>
    [TempData]
    public string? StatusMessage { get; set; }

    /// <summary>
    /// Inicializa la página con la definición funcional y los valores por defecto del caso de uso.
    /// </summary>
    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (!_options.EnableAdministratorCreationUi)
        {
            return NotFound();
        }

        return await LoadDefinitionAndRenderPageAsync(cancellationToken, applyDefaultValues: true);
    }

    /// <summary>
    /// Procesa el formulario de creación de administrador.
    /// </summary>
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!_options.EnableAdministratorCreationUi)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return await LoadDefinitionAndRenderPageAsync(cancellationToken, applyDefaultValues: false);
        }

        var result = await _adminUserService.RegisterAdminAsync(
            CreateRegisterAdminCommand(),
            cancellationToken);

        if (result.IsFailure)
        {
            ErrorMessage = result.Error.Message;
            return await LoadDefinitionAndRenderPageAsync(cancellationToken, applyDefaultValues: false);
        }

        StatusMessage = $"El administrador '{result.Value.Name}' fue creado correctamente con el correo '{result.Value.Email}'.";
        return RedirectToPage("./Index");
    }

    private RegisterAdminCommand CreateRegisterAdminCommand()
    {
        return new RegisterAdminCommand
        {
            Name = Input.Name,
            Email = Input.Email,
            Password = Input.Password,
            ConfirmPassword = Input.ConfirmPassword,
            Area = Input.Area,
            IsActive = Input.IsActive,
            IsEmailConfirmed = Input.IsEmailConfirmed,
            RequestedByUserId = GetRequestedByUserId(),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            Source = "AdminPortal",
            Reason = string.IsNullOrWhiteSpace(Input.Reason) ? null : Input.Reason.Trim()
        };
    }

    private async Task<IActionResult> LoadDefinitionAndRenderPageAsync(
        CancellationToken cancellationToken,
        bool applyDefaultValues)
    {
        var result = await _adminUserService.GetAdminRegistrationDefinitionAsync(new GetAdminRegistrationDefinitionQuery
        {
            RequestedByUserId = GetRequestedByUserId(),
            RequestedByUserName = User.Identity?.Name,
            Source = "AdminPortal"
        }, cancellationToken);

        if (result.IsFailure)
        {
            ErrorMessage = result.Error.Message;
            return Page();
        }

        Definition = result.Value;

        if (applyDefaultValues && string.IsNullOrWhiteSpace(Input.Area))
        {
            Input.Area = Definition.DefaultArea;
            Input.IsActive = Definition.DefaultIsActive;
            Input.IsEmailConfirmed = Definition.DefaultIsEmailConfirmed;
        }

        return Page();
    }

    private Guid? GetRequestedByUserId()
    {
        string? rawUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(rawUserId, out Guid userId)
            ? userId
            : null;
    }

    /// <summary>
    /// Representa el modelo de entrada del formulario de alta administrativa.
    /// </summary>
    public sealed class InputModel
    {
        /// <summary>
        /// Nombre visible del administrador.
        /// </summary>
        [Display(Name = "Nombre completo")]
        [Required(ErrorMessage = "El nombre del administrador es obligatorio.")]
        [StringLength(AdminRegistrationPolicies.MaxNameLength, MinimumLength = AdminRegistrationPolicies.MinNameLength, ErrorMessage = "El nombre del administrador debe tener entre 3 y 100 caracteres.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Correo electrónico del administrador.
        /// </summary>
        [Display(Name = "Correo electrónico")]
        [Required(ErrorMessage = "El correo electrónico del administrador es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo electrónico del administrador no tiene un formato válido.")]
        [StringLength(AdminRegistrationPolicies.MaxEmailLength, ErrorMessage = "El correo electrónico del administrador supera la longitud permitida.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña temporal de la cuenta.
        /// </summary>
        [Display(Name = "Contraseña temporal")]
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [StringLength(AdminRegistrationPolicies.MaxPasswordLength, MinimumLength = AdminRegistrationPolicies.MinPasswordLength, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres.")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Confirmación de la contraseña temporal.
        /// </summary>
        [Display(Name = "Confirmar contraseña")]
        [Required(ErrorMessage = "La confirmación de la contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "La confirmación de la contraseña no coincide con la contraseña ingresada.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        /// <summary>
        /// Área organizacional del nuevo administrador.
        /// </summary>
        [Display(Name = "Área organizacional")]
        [Required(ErrorMessage = "El área del administrador es obligatoria.")]
        [StringLength(AdminRegistrationPolicies.MaxAreaLength, MinimumLength = AdminRegistrationPolicies.MinAreaLength, ErrorMessage = "El área del administrador debe tener entre 3 y 60 caracteres.")]
        public string Area { get; set; } = string.Empty;

        /// <summary>
        /// Indica si la cuenta debe crearse activa.
        /// </summary>
        [Display(Name = "Crear cuenta activa")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Indica si el correo debe crearse como confirmado.
        /// </summary>
        [Display(Name = "Marcar correo como confirmado")]
        public bool IsEmailConfirmed { get; set; }

        /// <summary>
        /// Observación operativa asociada al alta.
        /// </summary>
        [Display(Name = "Observación operativa")]
        [StringLength(AdminRegistrationPolicies.MaxReasonLength, ErrorMessage = "La observación operativa supera la longitud permitida.")]
        public string? Reason { get; set; }
    }
}
