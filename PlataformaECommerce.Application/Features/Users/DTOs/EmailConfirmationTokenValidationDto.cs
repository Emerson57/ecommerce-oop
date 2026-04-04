namespace PlataformaECommerce.Application.Features.Users.DTOs;

/// <summary>
/// Representa el resultado de validar un token temporal de confirmación de correo.
/// </summary>
public sealed record EmailConfirmationTokenValidationDto
{
    /// <summary>
    /// Identificador del usuario confirmado por el token.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Correo electrónico asociado al token.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Versión temporal del usuario para invalidar tokens antiguos.
    /// </summary>
    public long UserVersionTicks { get; init; }

    /// <summary>
    /// Fecha UTC en la que expira el token.
    /// </summary>
    public DateTime ExpiresAtUtc { get; init; }
}
