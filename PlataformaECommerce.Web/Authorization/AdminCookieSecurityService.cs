using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using PlataformaECommerce.Application.Common.Security;
using PlataformaECommerce.Application.Interfaces.Repositories.Users;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Web.Authorization;

/// <summary>
/// Valida la consistencia y vigencia de la sesión autenticada del backoffice administrativo.
/// </summary>
/// <remarks>
/// Este servicio refuerza la seguridad de la cookie administrativa verificando en cada solicitud que:
/// - el principal siga siendo administrativo,
/// - la sesión no haya superado su vida útil absoluta,
/// - y el actor autenticado continúe existiendo y habilitado en persistencia.
/// </remarks>
public sealed class AdminCookieSecurityService
{
    private static readonly TimeSpan NonPersistentSessionLifetime = TimeSpan.FromHours(8);
    private static readonly TimeSpan PersistentSessionLifetime = TimeSpan.FromHours(24);

    private readonly IUserRepository _userRepository;
    private readonly ITenantContextAccessor _tenantContextAccessor;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="AdminCookieSecurityService"/>.
    /// </summary>
    /// <param name="userRepository">Repositorio de usuarios.</param>
    /// <param name="tenantContextAccessor">Accesor al tenant resuelto para la solicitud actual.</param>
    public AdminCookieSecurityService(IUserRepository userRepository, ITenantContextAccessor tenantContextAccessor)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _tenantContextAccessor = tenantContextAccessor ?? throw new ArgumentNullException(nameof(tenantContextAccessor));
    }

    /// <summary>
    /// Determina si el principal autenticado mantiene una sesión administrativa válida.
    /// </summary>
    /// <param name="principal">Principal autenticado a evaluar.</param>
    /// <param name="properties">Propiedades de autenticación asociadas a la cookie.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns><see langword="true"/> cuando la sesión sigue siendo válida.</returns>
    public async Task<bool> IsPrincipalValidAsync(
        ClaimsPrincipal? principal,
        AuthenticationProperties? properties,
        CancellationToken cancellationToken = default)
    {
        if (!AuthorizationPolicies.IsAdministrativePrincipal(principal))
        {
            return false;
        }

        if (!HasValidSessionLifetime(properties))
        {
            return false;
        }

        if (!HasConsistentTenantClaim(principal, _tenantContextAccessor.TenantId))
        {
            return false;
        }

        Guid? userId = GetUserId(principal);
        if (!userId.HasValue)
        {
            return false;
        }

        Administrador? actor = await _userRepository
            .GetAdministratorByIdAsync(userId.Value, cancellationToken)
            .ConfigureAwait(false);

        return actor is not null
            && actor.EstaHabilitado()
            && HasConsistentAdministrativeClaims(actor, principal!);
    }

    private static Guid? GetUserId(ClaimsPrincipal? principal)
    {
        string? rawUserId = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(rawUserId, out Guid userId)
            ? userId
            : null;
    }

    private static bool HasValidSessionLifetime(AuthenticationProperties? properties)
    {
        DateTimeOffset? issuedUtc = properties?.IssuedUtc;
        if (!issuedUtc.HasValue)
        {
            return false;
        }

        TimeSpan allowedLifetime = properties?.IsPersistent == true
            ? PersistentSessionLifetime
            : NonPersistentSessionLifetime;

        return DateTimeOffset.UtcNow <= issuedUtc.Value.Add(allowedLifetime);
    }

    private static bool HasConsistentAdministrativeClaims(Administrador actor, ClaimsPrincipal principal)
    {
        string? primaryRole = principal.FindFirstValue(AuthorizationPolicies.PrimaryRoleClaimType);
        if (!string.Equals(primaryRole, actor.Rol.ToString(), StringComparison.Ordinal))
        {
            return false;
        }

        bool hasSuperUserClaim = bool.TryParse(
            principal.FindFirstValue(AuthorizationPolicies.SuperUserClaimType),
            out bool isSuperUser)
            && isSuperUser;

        if (hasSuperUserClaim != actor.EsSuperUsuario)
        {
            return false;
        }

        string? administrativeArea = principal.FindFirstValue(AuthorizationPolicies.AdminAreaClaimType);
        if (!string.Equals(administrativeArea, actor.Area, StringComparison.Ordinal))
        {
            return false;
        }

        string[] principalAdministrativeRoles = principal
            .FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value)
            .Where(RolUsuarioExtensions.EsValorDeRolAdministrativo)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();

        string[] expectedAdministrativeRoles = actor.Rol
            .ObtenerRolesEfectivos()
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();

        return principalAdministrativeRoles.SequenceEqual(expectedAdministrativeRoles, StringComparer.Ordinal);
    }

    private static bool HasConsistentTenantClaim(ClaimsPrincipal? principal, string resolvedTenantId)
    {
        if (string.IsNullOrWhiteSpace(resolvedTenantId))
        {
            return false;
        }

        string? tenantClaim = principal?.FindFirstValue(SecurityClaimTypes.TenantId);
        return !string.IsNullOrWhiteSpace(tenantClaim)
            && string.Equals(tenantClaim.Trim(), resolvedTenantId.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
