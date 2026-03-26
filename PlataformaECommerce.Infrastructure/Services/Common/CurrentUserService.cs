using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PlataformaECommerce.Application.Common.Security;
using PlataformaECommerce.Application.Interfaces.Services.Common;

namespace PlataformaECommerce.Infrastructure.Services.Common;

/// <summary>
/// Implementa el acceso desacoplado al usuario autenticado actual a partir
/// del contexto HTTP administrado por ASP.NET Core.
/// </summary>
/// <remarks>
/// Este adaptador permite que la capa Application consulte información de identidad
/// sin depender directamente de <see cref="HttpContext"/>, <see cref="ClaimsPrincipal"/>
/// ni de la tecnología concreta de autenticación utilizada por la aplicación web.
/// </remarks>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="CurrentUserService"/>.
    /// </summary>
    /// <param name="httpContextAccessor">Accesor al contexto HTTP actual.</param>
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <inheritdoc />
    public Guid? UserId
    {
        get
        {
            string? value = GetClaimValue(ClaimTypes.NameIdentifier)
                ?? GetClaimValue(JwtRegisteredClaimNames.Sub);

            return Guid.TryParse(value, out Guid userId)
                ? userId
                : null;
        }
    }

    /// <inheritdoc />
    public string? UserName => GetClaimValue(ClaimTypes.Name)
        ?? GetClaimValue("name")
        ?? Principal?.Identity?.Name;

    /// <inheritdoc />
    public string? Email => GetClaimValue(ClaimTypes.Email)
        ?? GetClaimValue(JwtRegisteredClaimNames.Email);

    /// <inheritdoc />
    public string? Role => GetClaimValue(SecurityClaimTypes.PrimaryRole)
        ?? GetClaimValue(ClaimTypes.Role)
        ?? GetClaimValue("role");

    /// <inheritdoc />
    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    /// <inheritdoc />
    public bool IsInRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            throw new ArgumentException("El rol a validar es obligatorio.", nameof(role));
        }

        return Principal?.IsInRole(role.Trim()) == true;
    }

    /// <inheritdoc />
    public string? GetClaimValue(string claimType)
    {
        if (string.IsNullOrWhiteSpace(claimType))
        {
            throw new ArgumentException("El tipo de claim es obligatorio.", nameof(claimType));
        }

        return Principal?
            .Claims
            .FirstOrDefault(claim => string.Equals(claim.Type, claimType.Trim(), StringComparison.OrdinalIgnoreCase))?
            .Value;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<string> GetClaimValues(string claimType)
    {
        if (string.IsNullOrWhiteSpace(claimType))
        {
            throw new ArgumentException("El tipo de claim es obligatorio.", nameof(claimType));
        }

        return Principal?
            .Claims
            .Where(claim => string.Equals(claim.Type, claimType.Trim(), StringComparison.OrdinalIgnoreCase))
            .Select(claim => claim.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray()
            ?? Array.Empty<string>();
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;
}
