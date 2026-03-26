using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlataformaECommerce.Application.Common.Security;
using PlataformaECommerce.Application.Features.Auth.Commands;
using PlataformaECommerce.Application.Features.Auth.DTOs;
using PlataformaECommerce.Application.Features.Auth.Queries;
using PlataformaECommerce.Application.Features.Users;
using PlataformaECommerce.Application.Features.Users.Commands;
using PlataformaECommerce.Application.Interfaces.Services.Auth;
using PlataformaECommerce.Application.Interfaces.Services.Users;
using PlataformaECommerce.Web.Authorization;

namespace PlataformaECommerce.Web.Pages.Account;

/// <summary>
/// Proporciona la experiencia principal de Mi cuenta para clientes autenticados.
/// </summary>
/// <remarks>
/// Esta página consolida la información esencial del perfil del cliente autenticado,
/// reutilizando los casos de uso de consulta, actualización básica y cambio autenticado
/// de contraseña sin exponer lógica de negocio en la capa web.
/// </remarks>
[Authorize(
    Policy = AuthorizationPolicies.CustomerOnly,
    AuthenticationSchemes = AuthorizationPolicies.CustomerCookieScheme)]
public sealed class IndexModel : PageModel
{
    private const string AccountSource = "Web.Account.Index";
    private readonly IAuthApplicationService _authApplicationService;
    private readonly IUserApplicationService _userApplicationService;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="IndexModel"/>.
    /// </summary>
    /// <param name="authApplicationService">Servicio de aplicación de autenticación.</param>
    /// <param name="userApplicationService">Servicio de aplicación del módulo de usuarios.</param>
    public IndexModel(
        IAuthApplicationService authApplicationService,
        IUserApplicationService userApplicationService)
    {
        _authApplicationService = authApplicationService ?? throw new ArgumentNullException(nameof(authApplicationService));
        _userApplicationService = userApplicationService ?? throw new ArgumentNullException(nameof(userApplicationService));
    }

    /// <summary>
    /// Obtiene el perfil consolidado del cliente autenticado.
    /// </summary>
    public CustomerAccountViewModel Account { get; private set; } = new();

    /// <summary>
    /// Modelo de captura para la edición básica del perfil.
    /// </summary>
    [BindProperty]
    public UpdateProfileInputModel Profile { get; set; } = new();

    /// <summary>
    /// Modelo de captura para el cambio autenticado de contraseña.
    /// </summary>
    [BindProperty]
    public ChangePasswordInputModel PasswordChange { get; set; } = new();

    /// <summary>
    /// Obtiene el mensaje funcional publicado cuando una operación no puede completarse.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Obtiene o establece el mensaje temporal mostrado tras completar una operación exitosa.
    /// </summary>
    [TempData]
    public string? StatusMessage { get; set; }

    /// <summary>
    /// Carga la experiencia de Mi cuenta para el cliente autenticado actual.
    /// </summary>
    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        Guid? authenticatedUserId = GetAuthenticatedUserId();
        if (!authenticatedUserId.HasValue)
        {
            await InvalidateCustomerSessionAsync().ConfigureAwait(false);
            return RedirectToPage("/Auth/Login");
        }

        bool wasLoaded = await LoadCustomerContextAsync(authenticatedUserId.Value, cancellationToken, preserveProfileInput: false).ConfigureAwait(false);
        return wasLoaded
            ? Page()
            : RedirectToPage("/Auth/Login");
    }

    /// <summary>
    /// Procesa la actualización segura del nombre y correo del cliente autenticado.
    /// </summary>
    public async Task<IActionResult> OnPostUpdateProfileAsync(CancellationToken cancellationToken)
    {
        Guid? authenticatedUserId = GetAuthenticatedUserId();
        if (!authenticatedUserId.HasValue)
        {
            await InvalidateCustomerSessionAsync().ConfigureAwait(false);
            return RedirectToPage("/Auth/Login");
        }

        bool wasLoaded = await LoadCustomerContextAsync(authenticatedUserId.Value, cancellationToken, preserveProfileInput: true).ConfigureAwait(false);
        if (!wasLoaded)
        {
            return RedirectToPage("/Auth/Login");
        }

        RemoveModelStateEntries(nameof(PasswordChange));
        if (!ValidateInputModel(Profile, nameof(Profile)))
        {
            return Page();
        }

        string trimmedEmail = Profile.Email.Trim();
        bool emailChanged = !string.Equals(Account.Email, trimmedEmail, StringComparison.OrdinalIgnoreCase);

        var result = await _userApplicationService.UpdateUserBasicDataAsync(
            new UpdateUserBasicDataCommand
            {
                UserId = authenticatedUserId.Value,
                Name = Profile.Name.Trim(),
                Email = trimmedEmail,
                RequestedByUserId = authenticatedUserId.Value,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Source = AccountSource,
                ExternalReference = "Web.Account.UpdateProfile",
                Reason = "Autoservicio de perfil del cliente autenticado."
            },
            cancellationToken);

        if (result.IsFailure)
        {
            ErrorMessage = result.Error.Message;
            return Page();
        }

        if (emailChanged)
        {
            TempData[nameof(StatusMessage)] = "Tu correo fue actualizado. Por seguridad debes iniciar sesión nuevamente cuando confirmes la nueva dirección registrada.";
            await InvalidateCustomerSessionAsync().ConfigureAwait(false);
            return RedirectToPage("/Auth/Login");
        }

        StatusMessage = "Tu información básica fue actualizada correctamente.";
        return RedirectToPage();
    }

    /// <summary>
    /// Procesa el cambio autenticado de contraseña del cliente actual.
    /// </summary>
    public async Task<IActionResult> OnPostChangePasswordAsync(CancellationToken cancellationToken)
    {
        Guid? authenticatedUserId = GetAuthenticatedUserId();
        if (!authenticatedUserId.HasValue)
        {
            await InvalidateCustomerSessionAsync().ConfigureAwait(false);
            return RedirectToPage("/Auth/Login");
        }

        bool wasLoaded = await LoadCustomerContextAsync(authenticatedUserId.Value, cancellationToken, preserveProfileInput: true).ConfigureAwait(false);
        if (!wasLoaded)
        {
            return RedirectToPage("/Auth/Login");
        }

        RemoveModelStateEntries(nameof(Profile));
        if (!ValidateInputModel(PasswordChange, nameof(PasswordChange)))
        {
            return Page();
        }

        var result = await _authApplicationService.ChangePasswordAsync(
            new ChangePasswordCommand
            {
                UserId = authenticatedUserId.Value,
                CurrentPassword = PasswordChange.CurrentPassword,
                NewPassword = PasswordChange.NewPassword,
                ConfirmPassword = PasswordChange.ConfirmPassword,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString(),
                Source = AccountSource,
                ExternalReference = "Web.Account.ChangePassword",
                RequestedAtUtc = DateTime.UtcNow
            },
            cancellationToken);

        if (result.IsFailure)
        {
            ErrorMessage = result.Error.Message;
            return Page();
        }

        TempData[nameof(StatusMessage)] = "Tu contraseña fue actualizada correctamente. Inicia sesión nuevamente para continuar.";
        await InvalidateCustomerSessionAsync().ConfigureAwait(false);
        return RedirectToPage("/Auth/Login");
    }

    private async Task<bool> LoadCustomerContextAsync(Guid authenticatedUserId, CancellationToken cancellationToken, bool preserveProfileInput)
    {
        var result = await _authApplicationService.GetCurrentUserAsync(
            new GetCurrentUserQuery(authenticatedUserId)
            {
                AuthenticatedUserName = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name,
                ExternalReference = AccountSource,
                Source = AccountSource
            },
            cancellationToken);

        if (result.IsFailure)
        {
            ErrorMessage = result.Error.Message;
            await InvalidateCustomerSessionAsync().ConfigureAwait(false);
            return false;
        }

        if (!AuthorizationPolicies.IsCustomerRole(result.Value.Role))
        {
            await InvalidateCustomerSessionAsync().ConfigureAwait(false);
            return false;
        }

        Account = Map(result.Value);

        if (!preserveProfileInput)
        {
            Profile = new UpdateProfileInputModel
            {
                Name = Account.FullName,
                Email = Account.Email
            };
        }

        return true;
    }

    private Guid? GetAuthenticatedUserId()
    {
        string? rawUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(rawUserId, out Guid userId)
            ? userId
            : null;
    }

    private Task InvalidateCustomerSessionAsync()
    {
        return HttpContext.SignOutAsync(AuthorizationPolicies.CustomerCookieScheme);
    }

    private void RemoveModelStateEntries(string prefix)
    {
        string[] keys = ModelState.Keys
            .Where(key => string.Equals(key, prefix, StringComparison.Ordinal)
                || key.StartsWith($"{prefix}.", StringComparison.Ordinal))
            .ToArray();

        foreach (string key in keys)
        {
            ModelState.Remove(key);
        }
    }

    private bool ValidateInputModel(object model, string prefix)
    {
        ArgumentNullException.ThrowIfNull(model);

        ValidationContext validationContext = new(model);
        List<ValidationResult> validationResults = [];
        bool isValid = Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);

        foreach (ValidationResult validationResult in validationResults)
        {
            if (validationResult.MemberNames.Any())
            {
                foreach (string memberName in validationResult.MemberNames)
                {
                    ModelState.AddModelError($"{prefix}.{memberName}", validationResult.ErrorMessage ?? "El valor informado no es válido.");
                }

                continue;
            }

            ModelState.AddModelError(prefix, validationResult.ErrorMessage ?? "El valor informado no es válido.");
        }

        return isValid;
    }

    private static CustomerAccountViewModel Map(CurrentUserDto currentUser)
    {
        ArgumentNullException.ThrowIfNull(currentUser);

        return new CustomerAccountViewModel
        {
            UserId = currentUser.Id,
            FullName = currentUser.DisplayName,
            Email = currentUser.Email,
            Role = currentUser.Role ?? "Cliente",
            IsActive = currentUser.IsActive,
            IsEmailConfirmed = currentUser.IsEmailConfirmed,
            CreatedAtUtc = currentUser.CreatedAtUtc,
            LastLoginAtUtc = currentUser.LastLoginAtUtc,
            Roles = currentUser.Roles
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Distinct(StringComparer.Ordinal)
                .ToArray()
        };
    }

    /// <summary>
    /// Representa la proyección de Mi cuenta mostrada al cliente autenticado.
    /// </summary>
    public sealed class CustomerAccountViewModel
    {
        public Guid UserId { get; init; }
        public string FullName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Role { get; init; } = string.Empty;
        public bool IsActive { get; init; }
        public bool IsEmailConfirmed { get; init; }
        public DateTime? CreatedAtUtc { get; init; }
        public DateTime? LastLoginAtUtc { get; init; }
        public IReadOnlyCollection<string> Roles { get; init; } = Array.Empty<string>();
        public bool IsEnabled => IsActive && IsEmailConfirmed;

        public string StatusLabel
        {
            get
            {
                if (IsEnabled)
                {
                    return "Cuenta habilitada";
                }

                return IsActive
                    ? "Pendiente de confirmación"
                    : "Cuenta inactiva";
            }
        }
    }

    /// <summary>
    /// Captura la actualización de información básica del perfil.
    /// </summary>
    public sealed class UpdateProfileInputModel
    {
        [Display(Name = "Nombre completo")]
        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(CustomerRegistrationPolicies.MaxNameLength, MinimumLength = CustomerRegistrationPolicies.MinNameLength, ErrorMessage = "El nombre completo debe tener entre 3 y 100 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Correo electrónico")]
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]
        [StringLength(CustomerRegistrationPolicies.MaxEmailLength, ErrorMessage = "El correo electrónico supera la longitud permitida.")]
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// Captura el cambio autenticado de contraseña del cliente.
    /// </summary>
    public sealed class ChangePasswordInputModel
    {
        [Display(Name = "Contraseña actual")]
        [Required(ErrorMessage = "La contraseña actual es obligatoria.")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Display(Name = "Nueva contraseña")]
        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [StringLength(PasswordPolicyRules.MaxLength, MinimumLength = PasswordPolicyRules.MinLength, ErrorMessage = "La nueva contraseña debe tener entre 8 y 100 caracteres.")]
        public string NewPassword { get; set; } = string.Empty;

        [Display(Name = "Confirmar nueva contraseña")]
        [Required(ErrorMessage = "La confirmación de la nueva contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "La confirmación de la nueva contraseña no coincide con la contraseña ingresada.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
