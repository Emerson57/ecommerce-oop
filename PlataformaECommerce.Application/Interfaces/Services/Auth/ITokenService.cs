using System.Security.Claims;
using PlataformaECommerce.Domain.Entities.Users;

namespace PlataformaECommerce.Application.Interfaces.Services.Auth;

/// <summary>
/// Define el contrato del servicio responsable de la generación
/// y validación lógica de tokens de autenticación dentro de la capa de aplicación.
/// </summary>
/// <remarks>
/// Este servicio abstrae la lógica relacionada con emisión y lectura de tokens,
/// evitando que la capa Application dependa directamente de una tecnología específica
/// como JWT, OpenIddict, IdentityServer o cualquier otro mecanismo de autenticación.
///
/// Su responsabilidad incluye:
/// - generar tokens de acceso,
/// - generar tokens de refresco,
/// - extraer la identidad representada en un token,
/// - y exponer metadatos temporales asociados a su vigencia.
///
/// La implementación concreta de esta interfaz debe residir en la capa Infrastructure
/// y encapsular completamente los detalles técnicos de:
/// - firma,
/// - expiración,
/// - claims,
/// - algoritmos criptográficos,
/// - y validación estructural del token.
///
/// Esta interfaz permite que la capa Application trabaje con autenticación
/// de forma desacoplada, expresiva y testeable.
/// </remarks>
public interface ITokenService
{
    /// <summary>
    /// Genera un token de acceso para el usuario especificado.
    /// </summary>
    /// <param name="usuario">Usuario autenticado para el cual se emitirá el token.</param>
    /// <returns>
    /// Cadena que representa el token de acceso emitido.
    /// </returns>
    /// <remarks>
    /// El token de acceso debe contener la información mínima necesaria
    /// para identificar al usuario y autorizar sus operaciones dentro del sistema.
    /// </remarks>
    string GenerateAccessToken(Usuario usuario);

    /// <summary>
    /// Genera un token de refresco para el usuario especificado.
    /// </summary>
    /// <param name="usuario">Usuario autenticado para el cual se emitirá el token de refresco.</param>
    /// <returns>
    /// Cadena que representa el token de refresco emitido.
    /// </returns>
    /// <remarks>
    /// El token de refresco se utiliza normalmente para renovar sesiones
    /// sin exigir nuevamente autenticación interactiva inmediata.
    /// </remarks>
    string GenerateRefreshToken(Usuario usuario);

    /// <summary>
    /// Obtiene la fecha y hora UTC de expiración del token de acceso especificado.
    /// </summary>
    /// <param name="accessToken">Token de acceso a inspeccionar.</param>
    /// <returns>
    /// Fecha y hora UTC de expiración del token.
    /// </returns>
    DateTime GetAccessTokenExpirationUtc(string accessToken);

    /// <summary>
    /// Obtiene la fecha y hora UTC de expiración del token de refresco especificado.
    /// </summary>
    /// <param name="refreshToken">Token de refresco a inspeccionar.</param>
    /// <returns>
    /// Fecha y hora UTC de expiración del token.
    /// </returns>
    DateTime GetRefreshTokenExpirationUtc(string refreshToken);

    /// <summary>
    /// Extrae la identidad representada en un token de acceso.
    /// </summary>
    /// <param name="accessToken">Token de acceso a interpretar.</param>
    /// <returns>
    /// Un objeto <see cref="ClaimsPrincipal"/> con la identidad y claims del token,
    /// o <see langword="null"/> si el token no puede interpretarse correctamente.
    /// </returns>
    ClaimsPrincipal? GetPrincipalFromAccessToken(string accessToken);

    /// <summary>
    /// Extrae la identidad representada en un token de refresco.
    /// </summary>
    /// <param name="refreshToken">Token de refresco a interpretar.</param>
    /// <returns>
    /// Un objeto <see cref="ClaimsPrincipal"/> con la identidad y claims del token,
    /// o <see langword="null"/> si el token no puede interpretarse correctamente.
    /// </returns>
    ClaimsPrincipal? GetPrincipalFromRefreshToken(string refreshToken);
}