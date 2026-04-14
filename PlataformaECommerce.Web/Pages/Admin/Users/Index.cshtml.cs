using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using PlataformaECommerce.Application.Common.Security;
using PlataformaECommerce.Application.Features.Admin;
using PlataformaECommerce.Application.Features.Admin.Commands;
using PlataformaECommerce.Application.Features.Admin.DTOs;
using PlataformaECommerce.Application.Features.Admin.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Admin;
using PlataformaECommerce.Web.Authorization;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Web.Pages.Admin.Users;

/// <summary>
/// Representa el listado operativo de usuarios del backoffice.
/// </summary>
/// <remarks>
/// Esta página concentra la consulta segura de usuarios del sistema y expone la acción
/// controlada de restablecimiento administrativo de contraseña para sesiones de super usuario.
/// </remarks>
[Authorize(Policy = AuthorizationPolicies.SuperUserOnly)]
[EnableRateLimiting(RateLimitingOptions.AdministrationPolicyName)]
public sealed class IndexModel : PageModel
{
    private readonly IAdminUserService _adminUserService;
    private readonly AdminUsersBackofficeOptions _options;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="IndexModel"/>.
    /// </summary>
    /// <param name="adminUserService">Servicio administrativo especializado en usuarios.</param>
    /// <param name="options">Opciones de disponibilidad del módulo de usuarios.</param>
    public IndexModel(
        IAdminUserService adminUserService,
        IOptions<AdminUsersBackofficeOptions> options)
    {
        _adminUserService = adminUserService ?? throw new ArgumentNullException(nameof(adminUserService));
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;
    }

    /// <summary>
    /// Obtiene el resumen del backoffice de usuarios.
    /// </summary>
    public AdminUsersBackofficeDto UsersBackoffice { get; private set; } = new();

    /// <summary>
    /// Obtiene las cuentas administrativas visibles en el backoffice.
    /// </summary>
    public IReadOnlyCollection<AdminBackofficeUserDto> AdministrativeUsers =>
        UsersBackoffice.Users
            .Where(user => user.IsAdministrative)
            .ToArray();

    /// <summary>
    /// Obtiene todos los usuarios visibles en el backoffice.
    /// </summary>
    public IReadOnlyCollection<AdminBackofficeUserDto> VisibleUsers => UsersBackoffice.Users;

    /// <summary>
    /// Obtiene el usuario actualmente seleccionado para restablecimiento de contraseña.
    /// </summary>
    public AdminBackofficeUserDto? SelectedUser { get; private set; }

    /// <summary>
    /// Obtiene o establece el modelo de entrada del restablecimiento administrativo.
    /// </summary>
    [BindProperty]
    public ResetPasswordInputModel ResetPassword { get; set; } = new();

    /// <summary>
    /// Obtiene el mensaje de error funcional del módulo cuando la consulta falla.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Obtiene el mensaje funcional de éxito publicado tras registrar un administrador.
    /// </summary>
    [TempData]
    public string? StatusMessage { get; set; }

    /// <summary>
    /// Indica si la entrada web del alta de administradores se encuentra disponible para navegación.
    /// </summary>
    public bool IsAdminCreationUiAvailable => _options.EnableAdministratorCreationUi;

    /// <summary>
    /// Inicializa la página del backoffice de usuarios con su resumen consolidado.
    /// </summary>
    public async Task OnGetAsync(Guid? selectedUserId, CancellationToken cancellationToken)
    {
        await LoadUsersAsync(selectedUserId, cancellationToken);
    }

    /// <summary>
    /// Procesa el restablecimiento administrativo de contraseña del usuario seleccionado.
    /// </summary>
    public async Task<IActionResult> OnPostResetPasswordAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadUsersAsync(ResetPassword.TargetUserId, cancellationToken);
            return Page();
        }

        var result = await _adminUserService.ResetUserPasswordAsync(new ResetUserPasswordCommand
        {
            TargetUserId = ResetPassword.TargetUserId,
            NewPassword = ResetPassword.NewPassword,
            ConfirmPassword = ResetPassword.ConfirmPassword,
            RequestedByUserId = GetRequestedByUserId(),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            Source = "AdminPortal",
            ExternalReference = "Admin.Users.ResetPassword",
            Reason = string.IsNullOrWhiteSpace(ResetPassword.Reason) ? null : ResetPassword.Reason.Trim()
        }, cancellationToken);

        if (result.IsFailure)
        {
            ErrorMessage = result.Error.Message;
            await LoadUsersAsync(ResetPassword.TargetUserId, cancellationToken);
            return Page();
        }

        StatusMessage = $"La contraseña del usuario '{result.Value.Name}' fue restablecida correctamente.";
        return RedirectToPage();
    }

    private async Task LoadUsersAsync(Guid? selectedUserId, CancellationToken cancellationToken)
    {
        var result = await _adminUserService.GetUsersAsync(new GetAdminUsersQuery
        {
            OnlyAdministrativeUsers = false,
            RequestedByUserId = GetRequestedByUserId(),
            RequestedByUserName = User.Identity?.Name,
            Source = "AdminPortal"
        }, cancellationToken);

        if (result.IsFailure)
        {
            ErrorMessage = result.Error.Message;
            SelectedUser = null;
            return;
        }

        UsersBackoffice = result.Value;

        if (!selectedUserId.HasValue || selectedUserId == Guid.Empty)
        {
            SelectedUser = null;
            return;
        }

        SelectedUser = UsersBackoffice.Users.FirstOrDefault(user => user.Id == selectedUserId.Value);

        if (SelectedUser is null)
        {
            ErrorMessage = $"No se encontró el usuario seleccionado con identificador '{selectedUserId.Value}'.";
            return;
        }

        ResetPassword.TargetUserId = SelectedUser.Id;
    }

    private Guid? GetRequestedByUserId()
    {
        string? rawUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(rawUserId, out Guid userId)
            ? userId
            : null;
    }

    /// <summary>
    /// Representa la captura del formulario de restablecimiento administrativo.
    /// </summary>
    public sealed class ResetPasswordInputModel
    {
        /// <summary>
        /// Identificador del usuario objetivo.
        /// </summary>
        [Required(ErrorMessage = "El usuario objetivo es obligatorio.")]
        public Guid TargetUserId { get; set; }

        /// <summary>
        /// Nueva contraseña a aplicar al usuario.
        /// </summary>
        [Display(Name = "Nueva contraseña")]
        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [StringLength(PasswordPolicyRules.MaxLength, MinimumLength = PasswordPolicyRules.MinLength, ErrorMessage = "La nueva contraseña debe tener entre 8 y 100 caracteres.")]
        public string NewPassword { get; set; } = string.Empty;

        /// <summary>
        /// Confirmación de la nueva contraseña.
        /// </summary>
        [Display(Name = "Confirmar nueva contraseña")]
        [Required(ErrorMessage = "La confirmación de la nueva contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "La confirmación de la nueva contraseña no coincide con la contraseña ingresada.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        /// <summary>
        /// Motivo operativo del restablecimiento.
        /// </summary>
        [Display(Name = "Motivo operativo")]
        [StringLength(AdminRegistrationPolicies.MaxReasonLength, ErrorMessage = "El motivo operativo supera la longitud permitida.")]
        public string? Reason { get; set; }
    }
}
