using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace PlataformaECommerce.Web.Authorization;

/// <summary>
/// Ejecuta validaciones adicionales sobre la cookie de clientes antes de aceptar la sesión del sitio público.
/// </summary>
public sealed class CustomerCookieAuthenticationEvents : CookieAuthenticationEvents
{
    private readonly CustomerCookieSecurityService _securityService;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="CustomerCookieAuthenticationEvents"/>.
    /// </summary>
    /// <param name="securityService">Servicio de validación de sesión de clientes.</param>
    public CustomerCookieAuthenticationEvents(CustomerCookieSecurityService securityService)
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
            await context.HttpContext.SignOutAsync(AuthorizationPolicies.CustomerCookieScheme).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (IsApiRequest(context.HttpContext.Request))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }

        return base.RedirectToLogin(context);
    }

    /// <inheritdoc />
    public override Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (IsApiRequest(context.HttpContext.Request))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }

        return base.RedirectToAccessDenied(context);
    }

    private static bool IsApiRequest(HttpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.Path.StartsWithSegments("/api");
    }
}
