using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Users;
using PlataformaECommerce.Application.Features.Users.Commands;
using PlataformaECommerce.Application.Features.Users.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Users;
using PlataformaECommerce.Web.Configuration;
using PlataformaECommerce.Web.OnlineValidation;

namespace PlataformaECommerce.Web.Pages.Auth;

/// <summary>
/// Proporciona el flujo público de creación de cuenta para clientes de la plataforma.
/// </summary>
/// <remarks>
/// Esta página integra la captura interactiva del registro con los casos de uso existentes
/// de Application, manteniendo validación estructural en servidor y reforzando la experiencia
/// con validación cliente y verificación online de disponibilidad del correo.
/// </remarks>
[AllowAnonymous]
[EnableRateLimiting(WebRateLimitingOptions.AuthFlowPolicyName)]
public sealed class RegisterModel : PageModel
{
    private const string RegisterSource = "Web.Auth.Register";
    private readonly IUserApplicationService _userApplicationService;
    private readonly LinkGenerator _linkGenerator;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="RegisterModel"/>.
    /// </summary>
    /// <param name="userApplicationService">Servicio de aplicación del módulo de usuarios.</param>
    public RegisterModel(IUserApplicationService userApplicationService, LinkGenerator linkGenerator)
    {
        _userApplicationService = userApplicationService ?? throw new ArgumentNullException(nameof(userApplicationService));
        _linkGenerator = linkGenerator ?? throw new ArgumentNullException(nameof(linkGenerator));
    }

    /// <summary>
    /// Obtiene o establece el modelo de entrada del formulario de registro.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// Obtiene el mensaje funcional de error asociado al registro actual.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Obtiene o establece el mensaje temporal mostrado tras completar el registro.
    /// </summary>
    [TempData]
    public string? StatusMessage { get; set; }

    /// <summary>
    /// Correo electrónico registrado mostrado en la confirmación posterior al alta.
    /// </summary>
    [TempData]
    public string? RegisteredEmail { get; set; }

    /// <summary>
    /// Inicializa la página de registro público.
    /// </summary>
    public void OnGet()
    {
    }

    /// <summary>
    /// Valida en línea la disponibilidad del correo electrónico ingresado.
    /// </summary>
    /// <param name="email">Correo electrónico a verificar.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Respuesta JSON con el resultado de disponibilidad.</returns>
    public async Task<IActionResult> OnGetEmailAvailabilityAsync(string? email, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return OnlineValidationHttpResults.Ok("Register.EmailRequired", "El correo electrónico es obligatorio.", isValid: false, isAvailable: false);
        }

        if (cancellationToken.IsCancellationRequested || HttpContext.RequestAborted.IsCancellationRequested)
        {
            return OnlineValidationHttpResults.Canceled();
        }

        try
        {
            Result<PlataformaECommerce.Application.Features.Users.DTOs.UserDto> result = await _userApplicationService.GetUserByEmailAsync(
                new GetUserByEmailQuery(email)
                {
                    ExternalReference = RegisterSource
                },
                cancellationToken);

            if (result.IsSuccess)
            {
                return OnlineValidationHttpResults.Ok("Register.EmailAlreadyExists", "El correo electrónico ya se encuentra registrado.", isValid: true, isAvailable: false);
            }

            return result.Error.Code == "Users.NotFoundByEmail"
                ? OnlineValidationHttpResults.Ok("Register.EmailAvailable", "El correo electrónico está disponible.", isValid: true, isAvailable: true)
                : OnlineValidationHttpResults.Ok(result.Error.Code, result.Error.Message, isValid: false, isAvailable: false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested || HttpContext.RequestAborted.IsCancellationRequested)
        {
            return OnlineValidationHttpResults.Canceled();
        }
        catch (TaskCanceledException)
        {
            return OnlineValidationHttpResults.ServiceUnavailable("Register.EmailAvailabilityUnavailable", "No fue posible validar el correo electrónico en este momento.");
        }
        catch (TimeoutException)
        {
            return OnlineValidationHttpResults.ServiceUnavailable("Register.EmailAvailabilityUnavailable", "No fue posible validar el correo electrónico en este momento.");
        }
    }

    /// <summary>
    /// Procesa la creación pública de una nueva cuenta de cliente.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Resultado de navegación correspondiente al registro.</returns>
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        ValidateRequiredConsents();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _userApplicationService.RegisterCustomerAsync(
            CreateRegisterCustomerCommand(),
            cancellationToken);

        if (result.IsFailure)
        {
            ErrorMessage = result.Error.Message;
            return Page();
        }

        StatusMessage = "La cuenta fue creada correctamente. Revisa tu correo electrónico para confirmar la activación antes de iniciar sesión.";
        RegisteredEmail = Input.Email;
        return RedirectToPage("/Auth/RegisterConfirmation");
    }

    private RegisterCustomerCommand CreateRegisterCustomerCommand()
    {
        return new RegisterCustomerCommand
        {
            Name = Input.Name,
            Email = Input.Email,
            Password = Input.Password,
            ConfirmPassword = Input.ConfirmPassword,
            Preferences = ParsePreferences(Input.PreferencesText),
            AcceptTermsAndConditions = Input.AcceptTermsAndConditions,
            AcceptPrivacyPolicy = Input.AcceptPrivacyPolicy,
            AcceptMarketingCommunications = Input.AcceptMarketingCommunications,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            Source = RegisterSource,
            ExternalReference = RegisterSource,
            EmailConfirmationUrl = BuildEmailConfirmationUrl()
        };
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

    private void ValidateRequiredConsents()
    {
        if (!Input.AcceptTermsAndConditions)
        {
            ModelState.AddModelError(
                $"{nameof(Input)}.{nameof(InputModel.AcceptTermsAndConditions)}",
                "Debes aceptar los términos y condiciones.");
        }

        if (!Input.AcceptPrivacyPolicy)
        {
            ModelState.AddModelError(
                $"{nameof(Input)}.{nameof(InputModel.AcceptPrivacyPolicy)}",
                "Debes aceptar la política de tratamiento de datos personales.");
        }
    }

    private static IReadOnlyCollection<string> ParsePreferences(string? preferencesText)
    {
        if (string.IsNullOrWhiteSpace(preferencesText))
        {
            return Array.Empty<string>();
        }

        return preferencesText
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <summary>
    /// Representa el modelo de entrada del formulario de registro público.
    /// </summary>
    public sealed class InputModel
    {
        /// <summary>
        /// Nombre completo del cliente.
        /// </summary>
        [Display(Name = "Nombre completo")]
        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(CustomerRegistrationPolicies.MaxNameLength, MinimumLength = CustomerRegistrationPolicies.MinNameLength, ErrorMessage = "El nombre completo debe tener entre 3 y 100 caracteres.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Correo electrónico principal de la cuenta.
        /// </summary>
        [Display(Name = "Correo electrónico")]
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]
        [StringLength(CustomerRegistrationPolicies.MaxEmailLength, ErrorMessage = "El correo electrónico supera la longitud permitida.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña de acceso de la cuenta.
        /// </summary>
        [Display(Name = "Contraseña")]
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [StringLength(CustomerRegistrationPolicies.MaxPasswordLength, MinimumLength = CustomerRegistrationPolicies.MinPasswordLength, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres.")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Confirmación de la contraseña ingresada.
        /// </summary>
        [Display(Name = "Confirmar contraseña")]
        [Required(ErrorMessage = "La confirmación de la contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "La confirmación de la contraseña no coincide con la contraseña ingresada.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        /// <summary>
        /// Preferencias o intereses iniciales separados por coma.
        /// </summary>
        [Display(Name = "Intereses iniciales")]
        public string? PreferencesText { get; set; }

        /// <summary>
        /// Indica si el cliente acepta términos y condiciones.
        /// </summary>
        [Display(Name = "Acepto los términos y condiciones")]
        public bool AcceptTermsAndConditions { get; set; }

        /// <summary>
        /// Indica si el cliente acepta la política de tratamiento de datos.
        /// </summary>
        [Display(Name = "Acepto la política de tratamiento de datos")]
        public bool AcceptPrivacyPolicy { get; set; }

        /// <summary>
        /// Indica si el cliente desea recibir comunicaciones comerciales.
        /// </summary>
        [Display(Name = "Deseo recibir novedades y comunicaciones comerciales")]
        public bool AcceptMarketingCommunications { get; set; }
    }
}
