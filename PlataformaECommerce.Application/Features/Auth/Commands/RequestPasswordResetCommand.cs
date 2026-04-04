namespace PlataformaECommerce.Application.Features.Auth.Commands;

/// <summary>
/// Representa la solicitud de inicio del flujo de recuperación de contraseña.
/// </summary>
/// <remarks>
/// Este comando transporta únicamente la identidad declarada del usuario y metadatos
/// de contexto, manteniendo fuera de la UI y de la capa superior la lógica de emisión,
/// validación y endurecimiento del proceso de recuperación.
/// </remarks>
public sealed class RequestPasswordResetCommand
{
    /// <summary>
    /// Correo electrónico del usuario que solicita recuperar su contraseña.
    /// </summary>
    public string Email { get; init; } = string.Empty;

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
    /// URL absoluta del restablecimiento que se enviará al usuario por correo.
    /// </summary>
    public string? ResetPasswordUrl { get; init; }

    /// <summary>
    /// Fecha UTC en la que la capa superior registró la solicitud.
    /// </summary>
    public DateTime? RequestedAtUtc { get; init; }
}
