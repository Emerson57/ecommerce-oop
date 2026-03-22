using PlataformaECommerce.Application.Common.Security;
using PlataformaECommerce.Domain.Enums;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace PlataformaECommerce.Web.Authorization;

/// <summary>
/// Centraliza los nombres de políticas de autorización utilizadas por la aplicación web.
/// </summary>
/// <remarks>
/// Esta clase evita la dispersión de cadenas mágicas relacionadas con seguridad,
/// facilitando la reutilización consistente de políticas entre Razor Pages,
/// controladores y futuros componentes administrativos.
/// </remarks>
public static class AuthorizationPolicies
{
    /// <summary>
    /// Nombre del esquema de autenticación por políticas que resuelve dinámicamente la cookie activa.
    /// </summary>
    public const string AppCookieScheme = "AppCookie";

    /// <summary>
    /// Nombre del esquema de autenticación por cookies utilizado por el backoffice administrativo.
    /// </summary>
    public const string AdminCookieScheme = "AdminCookie";

    /// <summary>
    /// Nombre del esquema de autenticación por cookies utilizado por clientes autenticados.
    /// </summary>
    public const string CustomerCookieScheme = "CustomerCookie";

    /// <summary>
    /// Nombre físico de la cookie administrativa.
    /// </summary>
    public const string AdminCookieName = "PlataformaECommerce.Admin";

    /// <summary>
    /// Nombre físico de la cookie de clientes.
    /// </summary>
    public const string CustomerCookieName = "PlataformaECommerce.Customer";

    /// <summary>
    /// Nombre de la política que restringe el acceso a usuarios con rol administrativo.
    /// </summary>
    public const string AdminOnly = "AdminOnly";

    /// <summary>
    /// Nombre de la política que restringe operaciones sensibles a super usuarios.
    /// </summary>
    public const string SuperUserOnly = "SuperUserOnly";

    /// <summary>
    /// Nombre de la política que restringe el acceso a clientes autenticados.
    /// </summary>
    public const string CustomerOnly = "CustomerOnly";

    /// <summary>
    /// Claim que identifica el área administrativa efectiva del usuario autenticado.
    /// </summary>
    public const string AdminAreaClaimType = SecurityClaimTypes.AdminArea;

    /// <summary>
    /// Claim que representa el rol primario del usuario autenticado.
    /// </summary>
    public const string PrimaryRoleClaimType = SecurityClaimTypes.PrimaryRole;

    /// <summary>
    /// Claim que indica si la cuenta autenticada posee privilegios de super usuario.
    /// </summary>
    public const string SuperUserClaimType = SecurityClaimTypes.IsSuperUser;

    /// <summary>
    /// Obtiene el conjunto de roles considerados administrativos para el backoffice.
    /// </summary>
    public static IReadOnlyCollection<string> AdministrativeRoles { get; } = RolUsuarioExtensions.RolesAdministrativos
        .Select(role => role.ToString())
        .ToArray();

    /// <summary>
    /// Determina si el valor suministrado corresponde a un rol administrativo soportado.
    /// </summary>
    /// <param name="role">Rol a evaluar.</param>
    /// <returns><see langword="true"/> cuando el rol pertenece al backoffice.</returns>
    public static bool IsAdministrativeRole(string? role)
    {
        return RolUsuarioExtensions.EsValorDeRolAdministrativo(role);
    }

    /// <summary>
    /// Determina si el valor suministrado corresponde a un rol de cliente soportado.
    /// </summary>
    /// <param name="role">Rol a evaluar.</param>
    /// <returns><see langword="true"/> cuando el rol corresponde a cliente.</returns>
    public static bool IsCustomerRole(string? role)
    {
        return string.Equals(role, RolUsuario.Cliente.ToString(), StringComparison.Ordinal);
    }

    /// <summary>
    /// Determina si la colección suministrada contiene al menos un rol administrativo válido.
    /// </summary>
    /// <param name="roles">Colección de roles a evaluar.</param>
    /// <returns><see langword="true"/> cuando existe un rol administrativo válido.</returns>
    public static bool IsAdministrativeUser(IEnumerable<string> roles)
    {
        ArgumentNullException.ThrowIfNull(roles);

        return roles.Any(IsAdministrativeRole);
    }

    /// <summary>
    /// Determina si el principal autenticado representa una cuenta administrativa válida del backoffice.
    /// </summary>
    /// <param name="principal">Principal autenticado a evaluar.</param>
    /// <returns><see langword="true"/> cuando el principal corresponde a un administrador válido.</returns>
    public static bool IsAdministrativePrincipal(ClaimsPrincipal? principal)
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        string[] roles = principal
            .FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToArray();

        if (!IsAdministrativeUser(roles))
        {
            return false;
        }

        string? primaryRole = principal.FindFirstValue(PrimaryRoleClaimType);
        return IsAdministrativeRole(primaryRole);
    }

    /// <summary>
    /// Determina si el principal autenticado representa una cuenta de super usuario válida.
    /// </summary>
    /// <param name="principal">Principal autenticado a evaluar.</param>
    /// <returns><see langword="true"/> cuando el principal corresponde a un super usuario válido.</returns>
    public static bool IsSuperUserPrincipal(ClaimsPrincipal? principal)
    {
        if (!IsAdministrativePrincipal(principal))
        {
            return false;
        }

        bool hasSuperUserClaim = bool.TryParse(
            principal!.FindFirstValue(SuperUserClaimType),
            out bool isSuperUser)
            && isSuperUser;

        return hasSuperUserClaim
            && principal.IsInRole(RolUsuario.SuperUsuario.ToString())
            && string.Equals(
                principal.FindFirstValue(PrimaryRoleClaimType),
                RolUsuario.SuperUsuario.ToString(),
                StringComparison.Ordinal);
    }

    /// <summary>
    /// Determina si el principal autenticado representa una cuenta de cliente válida.
    /// </summary>
    /// <param name="principal">Principal autenticado a evaluar.</param>
    /// <returns><see langword="true"/> cuando el principal corresponde a un cliente válido.</returns>
    public static bool IsCustomerPrincipal(ClaimsPrincipal? principal)
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        string? primaryRole = principal.FindFirstValue(PrimaryRoleClaimType);
        if (!IsCustomerRole(primaryRole))
        {
            return false;
        }

        bool hasCustomerRole = principal.Claims.Any(claim =>
            string.Equals(claim.Type, ClaimTypes.Role, StringComparison.Ordinal)
            && IsCustomerRole(claim.Value));

        bool isSuperUser = bool.TryParse(principal.FindFirstValue(SuperUserClaimType), out bool parsedValue) && parsedValue;

        return hasCustomerRole && !isSuperUser;
    }

    /// <summary>
    /// Configura la cookie de autenticación administrativa del backoffice.
    /// </summary>
    /// <param name="options">Opciones de autenticación por cookies.</param>
    public static void ConfigureAdminCookie(CookieAuthenticationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Cookie.Name = AdminCookieName;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.EventsType = typeof(AdminCookieAuthenticationEvents);
    }

    /// <summary>
    /// Configura la cookie de autenticación utilizada por clientes del sitio público.
    /// </summary>
    /// <param name="options">Opciones de autenticación por cookies.</param>
    public static void ConfigureCustomerCookie(CookieAuthenticationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Cookie.Name = CustomerCookieName;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.EventsType = typeof(CustomerCookieAuthenticationEvents);
    }

    /// <summary>
    /// Resuelve el esquema de autenticación aplicable en función de la cookie activa de la solicitud.
    /// </summary>
    /// <param name="context">Contexto HTTP actual.</param>
    /// <returns>Nombre del esquema que debe autenticar la solicitud.</returns>
    public static string ResolveApplicationCookieScheme(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Request.Cookies.ContainsKey(AdminCookieName))
        {
            return AdminCookieScheme;
        }

        return context.Request.Cookies.ContainsKey(CustomerCookieName)
            ? CustomerCookieScheme
            : CustomerCookieScheme;
    }

    /// <summary>
    /// Registra las políticas del backoffice administrativo.
    /// </summary>
    /// <param name="options">Opciones de autorización de ASP.NET Core.</param>
    public static void ConfigureBackofficePolicies(AuthorizationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.AddPolicy(AdminOnly, policy =>
        {
            policy.AuthenticationSchemes.Add(AdminCookieScheme);
            policy.RequireAuthenticatedUser();
            policy.RequireAssertion(context => IsAdministrativePrincipal(context.User));
        });

        options.AddPolicy(CustomerOnly, policy =>
        {
            policy.AuthenticationSchemes.Add(CustomerCookieScheme);
            policy.RequireAuthenticatedUser();
            policy.RequireAssertion(context => IsCustomerPrincipal(context.User));
        });

        options.AddPolicy(SuperUserOnly, policy =>
        {
            policy.AuthenticationSchemes.Add(AdminCookieScheme);
            policy.RequireAuthenticatedUser();
            policy.RequireAssertion(context => IsSuperUserPrincipal(context.User));
        });
    }
}
