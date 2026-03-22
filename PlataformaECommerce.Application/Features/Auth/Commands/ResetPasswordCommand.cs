namespace PlataformaECommerce.Application.Features.Auth.Commands;

/// <summary>
/// Representa la solicitud de restablecimiento de contraseña a partir de un token temporal.
/// </summary>
/// <remarks>
/// Este comando encapsula el identificador del usuario, el token emitido por el sistema,
/// la nueva contraseña y los metadatos de contexto necesarios para ejecutar el cambio de forma segura.
/// </remarks>
public sealed class ResetPasswordCommand
{
    /// <summary>
    /// Identificador del usuario cuya contraseña será restablecida.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Token temporal de recuperación emitido por el sistema.
    /// </summary>
    public string Token { get; init; } = string.Empty;

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
