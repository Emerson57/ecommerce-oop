namespace PlataformaECommerce.Application.Features.Auth.DTOs;

/// <summary>
/// Representa el resultado funcional de una solicitud de recuperación de contraseña.
/// </summary>
/// <remarks>
/// El flujo siempre se acepta de forma genérica para no revelar si una cuenta existe o no.
/// Cuando la capa superior opera en un entorno de desarrollo controlado, puede utilizar los
/// datos de previsualización para construir un enlace temporal de restablecimiento sin depender
/// todavía de una infraestructura de correo electrónico.
/// </remarks>
public sealed record PasswordResetRequestResultDto
{
    /// <summary>
    /// Indica si la solicitud fue aceptada de forma genérica por el sistema.
    /// </summary>
    public bool Accepted { get; init; } = true;

    /// <summary>
    /// Identificador del usuario para el cual se emitió un token temporal, cuando aplica.
    /// </summary>
    public Guid? UserId { get; init; }

    /// <summary>
    /// Token temporal emitido para construir un enlace de recuperación, cuando aplica.
    /// </summary>
    public string? ResetToken { get; init; }

    /// <summary>
    /// Fecha y hora UTC en la que expira el token temporal, cuando aplica.
    /// </summary>
    public DateTime? ExpiresAtUtc { get; init; }

    /// <summary>
    /// Determina si el resultado contiene una previsualización utilizable del enlace de recuperación.
    /// </summary>
    public bool CanPreviewResetLink => UserId.HasValue && !string.IsNullOrWhiteSpace(ResetToken) && ExpiresAtUtc.HasValue;
}
