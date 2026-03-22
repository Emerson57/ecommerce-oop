using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace PlataformaECommerce.Web.Authorization;

/// <summary>
/// Ejecuta validaciones adicionales sobre la cookie administrativa antes de aceptar la sesión del backoffice.
/// </summary>
public sealed class AdminCookieAuthenticationEvents : CookieAuthenticationEvents
{
    private readonly AdminCookieSecurityService _securityService;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="AdminCookieAuthenticationEvents"/>.
    /// </summary>
    /// <param name="securityService">Servicio de validación de sesión administrativa.</param>
    public AdminCookieAuthenticationEvents(AdminCookieSecurityService securityService)
    {
        _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
    }

    /// <inheritdoc />
    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        bool isValid = await _securityService
            .IsPrincipalValidAsync(context.Principal, context.Properties, context.HttpContext.RequestAborted)
            .ConfigureAwait(false);

        if (!isValid)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(AuthorizationPolicies.AdminCookieScheme).ConfigureAwait(false);
        }
    }
}
