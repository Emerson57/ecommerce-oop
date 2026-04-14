using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using PlataformaECommerce.Application.Common.Security;
using PlataformaECommerce.Application.Interfaces.Repositories.Users;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Web.Authorization;

/// <summary>
/// Valida la consistencia y vigencia de la sesión autenticada de clientes del sitio público.
/// </summary>
/// <remarks>
/// Este servicio refuerza la seguridad de la cookie del cliente verificando en cada solicitud que:
/// - el principal continúe representando una cuenta de cliente,
/// - la sesión no haya superado su vida útil absoluta,
/// - y el usuario autenticado siga existiendo y habilitado en persistencia.
/// </remarks>
public sealed class CustomerCookieSecurityService
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantContextAccessor _tenantContextAccessor;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="CustomerCookieSecurityService"/>.
    /// </summary>
    /// <param name="userRepository">Repositorio de usuarios.</param>
    /// <param name="tenantContextAccessor">Accesor al tenant resuelto para la solicitud actual.</param>
    public CustomerCookieSecurityService(
        IUserRepository userRepository,
        ITenantContextAccessor tenantContextAccessor)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _tenantContextAccessor = tenantContextAccessor ?? throw new ArgumentNullException(nameof(tenantContextAccessor));
    }

    /// <summary>
    /// Determina si el principal autenticado mantiene una sesión de cliente válida.
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
        if (!AuthorizationPolicies.IsCustomerPrincipal(principal))
        {
            return false;
        }

        if (!CookieAuthenticationSessionProperties.HasValidAbsoluteLifetime(properties))
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

        Cliente? actor = await _userRepository
            .GetCustomerByIdAsync(userId.Value, cancellationToken)
            .ConfigureAwait(false);

        return actor is not null
            && actor.EstaHabilitado()
            && HasConsistentCustomerClaims(actor, principal!);
    }

    private static Guid? GetUserId(ClaimsPrincipal? principal)
    {
        string? rawUserId = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(rawUserId, out Guid userId)
            ? userId
            : null;
    }

    private static bool HasConsistentCustomerClaims(Cliente actor, ClaimsPrincipal principal)
    {
        string? primaryRole = principal.FindFirstValue(AuthorizationPolicies.PrimaryRoleClaimType);
        if (!string.Equals(primaryRole, actor.Rol.ToString(), StringComparison.Ordinal))
        {
            return false;
        }

        bool isSuperUser = bool.TryParse(
            principal.FindFirstValue(AuthorizationPolicies.SuperUserClaimType),
            out bool parsedValue)
            && parsedValue;

        if (isSuperUser)
        {
            return false;
        }

        string[] principalRoles = principal
            .FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();

        return principalRoles.SequenceEqual([RolUsuario.Cliente.ToString()], StringComparer.Ordinal)
            && string.Equals(principal.FindFirstValue(ClaimTypes.Email), actor.CorreoElectronico.Value, StringComparison.OrdinalIgnoreCase)
            && string.Equals(principal.Identity?.Name, actor.Nombre, StringComparison.Ordinal);
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
