using System.Security.Claims;
using PlataformaECommerce.Application.Common.Security;
using PlataformaECommerce.Application.Features.Auth.DTOs;
using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Web.Authorization;

internal static class AuthenticatedSessionFactory
{
    public static bool TryCreate(CurrentUserDto user, string? tenantId, out AuthenticatedSession? session)
    {
        ArgumentNullException.ThrowIfNull(user);

        session = null;

        if (!CanIssueSession(user, tenantId, out string authenticationScheme, out string redirectPage, out string[] effectiveRoles))
        {
            return false;
        }

        ClaimsIdentity identity = new(authenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
        identity.AddClaim(new Claim(ClaimTypes.Name, user.DisplayName));
        identity.AddClaim(new Claim(ClaimTypes.Email, user.Email.Trim()));
        identity.AddClaim(new Claim(SecurityClaimTypes.TenantId, tenantId!.Trim()));
        identity.AddClaim(new Claim(AuthorizationPolicies.PrimaryRoleClaimType, user.Role!.Trim()));
        identity.AddClaim(new Claim(AuthorizationPolicies.SuperUserClaimType, user.IsSuperUser.ToString()));

        foreach (string role in effectiveRoles)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        if (!string.IsNullOrWhiteSpace(user.Area))
        {
            identity.AddClaim(new Claim(AuthorizationPolicies.AdminAreaClaimType, user.Area.Trim()));
        }

        foreach (string permission in user.Permissions
                     .Where(permission => !string.IsNullOrWhiteSpace(permission))
                     .Select(permission => permission.Trim())
                     .Distinct(StringComparer.Ordinal))
        {
            identity.AddClaim(new Claim(AuthorizationPolicies.PermissionClaimType, permission));
        }

        session = new AuthenticatedSession(
            authenticationScheme,
            new ClaimsPrincipal(identity),
            redirectPage);

        return true;
    }

    private static bool CanIssueSession(
        CurrentUserDto user,
        string? tenantId,
        out string authenticationScheme,
        out string redirectPage,
        out string[] effectiveRoles)
    {
        authenticationScheme = string.Empty;
        redirectPage = string.Empty;
        effectiveRoles = [];

        if (user.Id == Guid.Empty
            || !user.IsActive
            || !user.IsEmailConfirmed
            || string.IsNullOrWhiteSpace(user.Email)
            || string.IsNullOrWhiteSpace(user.DisplayName)
            || string.IsNullOrWhiteSpace(user.Role)
            || string.IsNullOrWhiteSpace(tenantId))
        {
            return false;
        }

        if (AuthorizationPolicies.IsAdministrativeRole(user.Role))
        {
            if (!TryCreateAdministrativeSession(user, out authenticationScheme, out redirectPage, out effectiveRoles))
            {
                return false;
            }

            return true;
        }

        if (!AuthorizationPolicies.IsCustomerRole(user.Role)
            || user.IsSuperUser)
        {
            return false;
        }

        effectiveRoles = user.Roles
            .Where(role => AuthorizationPolicies.IsCustomerRole(role))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (effectiveRoles.Length != 1
            || !string.Equals(effectiveRoles[0], RolUsuario.Cliente.ToString(), StringComparison.Ordinal)
            || !string.IsNullOrWhiteSpace(user.Area))
        {
            return false;
        }

        authenticationScheme = AuthorizationPolicies.CustomerCookieScheme;
        redirectPage = "/Index";
        return true;
    }

    private static bool TryCreateAdministrativeSession(
        CurrentUserDto user,
        out string authenticationScheme,
        out string redirectPage,
        out string[] effectiveRoles)
    {
        authenticationScheme = string.Empty;
        redirectPage = string.Empty;
        effectiveRoles = [];

        if (string.IsNullOrWhiteSpace(user.Area))
        {
            return false;
        }

        if (!Enum.TryParse(user.Role.Trim(), ignoreCase: false, out RolUsuario primaryRole)
            || !primaryRole.EsAdministrativo())
        {
            return false;
        }

        bool shouldBeSuperUser = primaryRole == RolUsuario.SuperUsuario;
        if (user.IsSuperUser != shouldBeSuperUser)
        {
            return false;
        }

        string[] expectedRoles = primaryRole
            .ObtenerRolesEfectivos()
            .OrderBy(role => role, StringComparer.Ordinal)
            .ToArray();

        effectiveRoles = user.Roles
            .Where(role => RolUsuarioExtensions.EsValorDeRolAdministrativo(role))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(role => role, StringComparer.Ordinal)
            .ToArray();

        if (!effectiveRoles.SequenceEqual(expectedRoles, StringComparer.Ordinal))
        {
            return false;
        }

        authenticationScheme = AuthorizationPolicies.AdminCookieScheme;
        redirectPage = "/Admin/Index";
        return true;
    }
}

internal sealed record AuthenticatedSession(
    string AuthenticationScheme,
    ClaimsPrincipal Principal,
    string RedirectPage);
