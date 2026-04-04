namespace PlataformaECommerce.Application.Features.Users.Commands;

/// <summary>
/// Representa el comando de aplicación para reenviar el correo de confirmación de una cuenta.
/// </summary>
public sealed class ResendUserEmailConfirmationCommand
{
    /// <summary>
    /// Correo electrónico de la cuenta a evaluar para reenvío.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// URL absoluta base que se utilizará para confirmar el correo del usuario.
    /// </summary>
    public string EmailConfirmationUrl { get; init; } = string.Empty;

    /// <summary>
    /// Identificador del usuario que solicita el reenvío, cuando aplique.
    /// </summary>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>
    /// Dirección IP desde la cual se originó la solicitud.
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// Canal de origen del reenvío.
    /// </summary>
    public string? Source { get; init; }

    /// <summary>
    /// Referencia externa opcional del proceso.
    /// </summary>
    public string? ExternalReference { get; init; }
}
