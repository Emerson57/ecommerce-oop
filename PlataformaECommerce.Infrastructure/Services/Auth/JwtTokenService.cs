using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PlataformaECommerce.Application.Common.Security;
using PlataformaECommerce.Application.Interfaces.Services.Auth;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Infrastructure.Configurations;

namespace PlataformaECommerce.Infrastructure.Services.Auth;

/// <summary>
/// Implementa la emisión y lectura de tokens JWT para autenticación de usuarios.
/// </summary>
/// <remarks>
/// Esta implementación encapsula el detalle técnico de firma, expiración,
/// normalización de claims y validación estructural de tokens para que la capa
/// Application opere sobre una abstracción estable y testeable.
/// </remarks>
public sealed class JwtTokenService : ITokenService
{
    private const string AccessTokenType = "access";
    private const string RefreshTokenType = "refresh";
    private const string TokenTypeClaim = "token_type";

    private readonly JwtSettings _settings;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();
    private readonly SigningCredentials _signingCredentials;
    private readonly SymmetricSecurityKey _securityKey;
    private readonly ITenantContextAccessor _tenantContextAccessor;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="JwtTokenService"/>.
    /// </summary>
    /// <param name="options">Opciones JWT de la solución.</param>
    public JwtTokenService(IOptions<JwtSettings> options, ITenantContextAccessor tenantContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(options);

        _settings = options.Value;
        _tenantContextAccessor = tenantContextAccessor ?? throw new ArgumentNullException(nameof(tenantContextAccessor));

        if (string.IsNullOrWhiteSpace(_settings.Issuer))
        {
            throw new InvalidOperationException("La configuración JWT requiere un emisor válido.");
        }

        if (string.IsNullOrWhiteSpace(_settings.Audience))
        {
            throw new InvalidOperationException("La configuración JWT requiere una audiencia válida.");
        }

        if (string.IsNullOrWhiteSpace(_settings.SigningKey) || _settings.SigningKey.Length < 32)
        {
            throw new InvalidOperationException("La configuración JWT requiere una clave de firma de al menos 32 caracteres.");
        }

        _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SigningKey));
        _signingCredentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256);
    }

    /// <inheritdoc />
    public string GenerateAccessToken(Usuario usuario)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        return GenerateToken(usuario, AccessTokenType, DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes));
    }

    /// <inheritdoc />
    public string GenerateRefreshToken(Usuario usuario)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        return GenerateToken(usuario, RefreshTokenType, DateTime.UtcNow.AddDays(_settings.RefreshTokenExpirationDays));
    }

    /// <inheritdoc />
    public DateTime GetAccessTokenExpirationUtc(string accessToken)
    {
        return ReadValidatedToken(accessToken, AccessTokenType).ValidTo;
    }

    /// <inheritdoc />
    public DateTime GetRefreshTokenExpirationUtc(string refreshToken)
    {
        return ReadValidatedToken(refreshToken, RefreshTokenType).ValidTo;
    }

    /// <inheritdoc />
    public ClaimsPrincipal? GetPrincipalFromAccessToken(string accessToken)
    {
        return TryGetPrincipal(accessToken, AccessTokenType);
    }

    /// <inheritdoc />
    public ClaimsPrincipal? GetPrincipalFromRefreshToken(string refreshToken)
    {
        return TryGetPrincipal(refreshToken, RefreshTokenType);
    }

    private string GenerateToken(Usuario usuario, string tokenType, DateTime expiresAtUtc)
    {
        string tenantId = _tenantContextAccessor.TenantId;
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new InvalidOperationException("No se puede emitir un token sin un tenant activo resuelto.");
        }

        List<Claim> claims = BuildClaims(usuario, tokenType, tenantId);

        JwtSecurityToken token = new(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAtUtc,
            signingCredentials: _signingCredentials);

        return _tokenHandler.WriteToken(token);
    }

    private static List<Claim> BuildClaims(Usuario usuario, string tokenType, string tenantId)
    {
        string userId = usuario.Id.ToString();
        string email = usuario.CorreoElectronico.Value;
        string role = usuario.Rol.ToString();
        IReadOnlyCollection<string> roles = usuario.Rol.ObtenerRolesEfectivos();

        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.UniqueName, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, usuario.Nombre),
            new(ClaimTypes.Email, email),
            new(SecurityClaimTypes.TenantId, tenantId.Trim()),
            new(SecurityClaimTypes.PrimaryRole, role),
            new(SecurityClaimTypes.IsSuperUser, usuario.Rol == RolUsuario.SuperUsuario ? bool.TrueString : bool.FalseString),
            new(TokenTypeClaim, tokenType)
        ];

        foreach (string effectiveRole in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, effectiveRole));
        }

        if (usuario is Administrador admin)
        {
            claims.Add(new Claim(SecurityClaimTypes.AdminArea, admin.Area));
        }

        return claims;
    }

    private JwtSecurityToken ReadValidatedToken(string token, string expectedTokenType)
    {
        ClaimsPrincipal principal = ValidateToken(token, expectedTokenType, validateLifetime: false, out SecurityToken validatedToken);
        EnsureExpectedTokenType(principal, expectedTokenType);

        return validatedToken as JwtSecurityToken
            ?? throw new ArgumentException("El token proporcionado no corresponde a un JWT válido.", nameof(token));
    }

    private ClaimsPrincipal? TryGetPrincipal(string token, string expectedTokenType)
    {
        try
        {
            ClaimsPrincipal principal = ValidateToken(token, expectedTokenType, validateLifetime: false, out _);
            EnsureExpectedTokenType(principal, expectedTokenType);
            return principal;
        }
        catch (ArgumentException)
        {
            return null;
        }
        catch (SecurityTokenException)
        {
            return null;
        }
    }

    private ClaimsPrincipal ValidateToken(
        string token,
        string expectedTokenType,
        bool validateLifetime,
        out SecurityToken validatedToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("El token es obligatorio.", nameof(token));
        }

        TokenValidationParameters validationParameters = BuildValidationParameters(validateLifetime);
        ClaimsPrincipal principal = _tokenHandler.ValidateToken(token, validationParameters, out validatedToken);

        EnsureExpectedTokenType(principal, expectedTokenType);

        return principal;
    }

    private TokenValidationParameters BuildValidationParameters(bool validateLifetime)
    {
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _securityKey,
            ValidateIssuer = true,
            ValidIssuer = _settings.Issuer,
            ValidateAudience = true,
            ValidAudience = _settings.Audience,
            ValidateLifetime = validateLifetime,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    }

    private static void EnsureExpectedTokenType(ClaimsPrincipal principal, string expectedTokenType)
    {
        string? tokenType = principal.Claims.FirstOrDefault(claim => claim.Type == TokenTypeClaim)?.Value;

        if (!string.Equals(tokenType, expectedTokenType, StringComparison.Ordinal))
        {
            throw new SecurityTokenException("El tipo de token no coincide con la operación solicitada.");
        }
    }
}
