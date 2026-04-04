namespace PlataformaECommerce.Application.Common.Notifications;

/// <summary>
/// Representa los datos necesarios para enviar un correo de recuperación de contraseña.
/// </summary>
public sealed record PasswordResetEmailNotification
{
    /// <summary>
    /// Correo electrónico del destinatario.
    /// </summary>
    public string ToEmail { get; init; } = string.Empty;

    /// <summary>
    /// Nombre visible del destinatario.
    /// </summary>
    public string RecipientName { get; init; } = string.Empty;

    /// <summary>
    /// URL absoluta del restablecimiento de contraseña.
    /// </summary>
    public string ResetUrl { get; init; } = string.Empty;

    /// <summary>
    /// Fecha UTC en que expira el enlace temporal.
    /// </summary>
    public DateTime ExpiresAtUtc { get; init; }
}
