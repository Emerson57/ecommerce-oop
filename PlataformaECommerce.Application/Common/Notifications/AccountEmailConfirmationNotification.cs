namespace PlataformaECommerce.Application.Common.Notifications;

/// <summary>
/// Representa los datos necesarios para enviar un correo de confirmación de cuenta.
/// </summary>
public sealed record AccountEmailConfirmationNotification
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
    /// URL absoluta del enlace de confirmación de cuenta.
    /// </summary>
    public string ConfirmationUrl { get; init; } = string.Empty;
}
