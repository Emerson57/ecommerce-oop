namespace PlataformaECommerce.Application.Features.Auth.DTOs;

/// <summary>
/// Representa los datos confiables obtenidos tras validar un token de recuperación de contraseña.
/// </summary>
public sealed record PasswordResetTokenValidationDto
{
    /// <summary>
    /// Identificador del usuario al que pertenece el token.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Correo electrónico incorporado al token protegido.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Valor de versión del usuario usado para invalidar tokens antiguos.
    /// </summary>
    public long UserVersionTicks { get; init; }

    /// <summary>
    /// Fecha y hora UTC de expiración del token protegido.
    /// </summary>
    public DateTime ExpiresAtUtc { get; init; }
}
