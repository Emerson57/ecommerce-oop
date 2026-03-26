using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using PlataformaECommerce.Application.Interfaces.Repositories.Users;
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
    private static readonly TimeSpan NonPersistentSessionLifetime = TimeSpan.FromHours(8);
    private static readonly TimeSpan PersistentSessionLifetime = TimeSpan.FromHours(24);

    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="CustomerCookieSecurityService"/>.
    /// </summary>
    /// <param name="userRepository">Repositorio de usuarios.</param>
    public CustomerCookieSecurityService(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
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

        if (!HasValidSessionLifetime(properties))
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
}
