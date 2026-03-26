namespace PlataformaECommerce.Application.Features.Auth.DTOs;

/// <summary>
/// Representa la respuesta resultante de un proceso de autenticación exitoso
/// dentro del sistema.
/// </summary>
/// <remarks>
/// Este DTO se utiliza como contrato de salida para devolver a la capa superior
/// la información necesaria para mantener la sesión autenticada del usuario,
/// así como los metadatos básicos asociados al contexto de seguridad.
///
/// Su propósito es desacoplar la representación interna del mecanismo de
/// autenticación respecto de los consumidores de la capa Application, tales como:
/// - controladores API,
/// - frontends web,
/// - clientes móviles,
/// - integraciones externas,
/// - y portales administrativos.
///
/// Esta clase no debe contener lógica de negocio ni responsabilidades de emisión,
/// firma o validación criptográfica de tokens. Dichas responsabilidades deben
/// residir en servicios especializados de autenticación e infraestructura.
/// </remarks>
public sealed class AuthResponseDto
{
    #region Tokens y sesión

    /// <summary>
    /// Token de acceso emitido para autenticar y autorizar al usuario
    /// en solicitudes posteriores.
    /// </summary>
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>
    /// Token de refresco utilizado para renovar la sesión autenticada,
    /// cuando la estrategia de seguridad implementada lo soporte.
    /// </summary>
    public string? RefreshToken { get; init; }

    /// <summary>
    /// Tipo de token emitido por el sistema.
    /// </summary>
    /// <remarks>
    /// El valor más común suele ser <c>Bearer</c>.
    /// </remarks>
    public string TokenType { get; init; } = "Bearer";

    /// <summary>
    /// Fecha y hora UTC en la que expira el token de acceso.
    /// </summary>
    public DateTime ExpiresAtUtc { get; init; }

    /// <summary>
    /// Tiempo de vida del token expresado en segundos.
    /// </summary>
    public int ExpiresInSeconds { get; init; }

    #endregion

    #region Información del usuario autenticado

    /// <summary>
    /// Información resumida del usuario autenticado.
    /// </summary>
    public CurrentUserDto User { get; init; } = new();

    #endregion

    #region Metadatos de autenticación

    /// <summary>
    /// Indica si el usuario debe realizar un cambio de contraseña
    /// antes de continuar con el uso normal del sistema.
    /// </summary>
    public bool RequiresPasswordChange { get; init; }

    /// <summary>
    /// Indica si la autenticación fue emitida como sesión persistente.
    /// </summary>
    public bool IsPersistentSession { get; init; }

    /// <summary>
    /// Fecha y hora UTC en la que fue emitida la respuesta de autenticación.
    /// </summary>
    public DateTime IssuedAtUtc { get; init; }

    /// <summary>
    /// Referencia externa opcional asociada al proceso de autenticación.
    /// </summary>
    public string? ExternalReference { get; init; }

    #endregion

    #region Propiedades calculadas

    /// <summary>
    /// Indica si la respuesta contiene un token de refresco informado.
    /// </summary>
    public bool HasRefreshToken => !string.IsNullOrWhiteSpace(RefreshToken);

    /// <summary>
    /// Indica si el token de acceso ya se encuentra vencido
    /// respecto de la hora UTC actual del sistema.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida y segura de la respuesta de autenticación.
    /// </summary>
    /// <returns>Cadena representativa del DTO.</returns>
    /// <remarks>
    /// Por motivos de seguridad, esta representación no expone el contenido real
    /// del token de acceso ni del token de refresco.
    /// </remarks>
    public override string ToString()
    {
        return $"AuthResponseDto | TokenType: {TokenType} | ExpiresAtUtc: {ExpiresAtUtc:O} | ExpiresInSeconds: {ExpiresInSeconds} | UserId: {User.Id} | UserName: {User.UserName}";
    }

    #endregion
}