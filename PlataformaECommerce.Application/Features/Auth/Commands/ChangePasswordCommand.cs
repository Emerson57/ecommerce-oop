namespace PlataformaECommerce.Application.Features.Auth.Commands;

/// <summary>
/// Representa la solicitud autenticada de cambio de contraseña desde una sesión vigente.
/// </summary>
/// <remarks>
/// Este comando encapsula la contraseña actual del usuario, la nueva credencial propuesta
/// y los metadatos de contexto necesarios para ejecutar el cambio de forma segura sin recurrir
/// a un token temporal de recuperación.
/// </remarks>
public sealed class ChangePasswordCommand
{
    /// <summary>
    /// Identificador del usuario cuya contraseña será actualizada.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Contraseña actual suministrada para validar la identidad del usuario.
    /// </summary>
    public string CurrentPassword { get; init; } = string.Empty;

    /// <summary>
    /// Nueva contraseña propuesta por el usuario.
    /// </summary>
    public string NewPassword { get; init; } = string.Empty;

    /// <summary>
    /// Confirmación de la nueva contraseña.
    /// </summary>
    public string ConfirmPassword { get; init; } = string.Empty;

    /// <summary>
    /// Dirección IP desde la cual se originó la solicitud.
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// User-Agent del cliente que origina la solicitud.
    /// </summary>
    public string? UserAgent { get; init; }

    /// <summary>
    /// Canal de origen de la solicitud.
    /// </summary>
    public string? Source { get; init; }

    /// <summary>
    /// Referencia externa opcional asociada al flujo.
    /// </summary>
    public string? ExternalReference { get; init; }

    /// <summary>
    /// Fecha UTC en la que la capa superior registró la solicitud.
    /// </summary>
    public DateTime? RequestedAtUtc { get; init; }
}
