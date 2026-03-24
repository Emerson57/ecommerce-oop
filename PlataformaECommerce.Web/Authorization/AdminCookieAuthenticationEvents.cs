using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace PlataformaECommerce.Web.Authorization;

/// <summary>
/// Ejecuta validaciones adicionales sobre la cookie administrativa antes de aceptar la sesión del backoffice.
/// </summary>
public sealed class AdminCookieAuthenticationEvents : CookieAuthenticationEvents
{
    private readonly AdminCookieSecurityService _securityService;
    private readonly ILogger<AdminCookieAuthenticationEvents> _logger;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="AdminCookieAuthenticationEvents"/>.
    /// </summary>
    /// <param name="securityService">Servicio de validación de sesión administrativa.</param>
    /// <param name="logger">Registrador estructurado de eventos de sesión administrativa.</param>
    public AdminCookieAuthenticationEvents(
        AdminCookieSecurityService securityService,
        ILogger<AdminCookieAuthenticationEvents> logger)
    {
        _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            _logger.LogWarning(
                "Se revocó una sesión administrativa inválida. UserId: {UserId}. RemoteIp: {RemoteIp}.",
                context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                context.HttpContext.Connection.RemoteIpAddress?.ToString());

            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(AuthorizationPolicies.AdminCookieScheme).ConfigureAwait(false);
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
