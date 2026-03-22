namespace PlataformaECommerce.Application.Features.Admin.Commands;

/// <summary>
/// Representa el comando administrativo para restablecer la contraseña de un usuario del sistema.
/// </summary>
/// <remarks>
/// Este comando modela una intención explícita de escritura ejecutada desde el backoffice
/// por un super usuario autenticado, permitiendo reemplazar de forma controlada la credencial
/// vigente de cualquier cuenta del sistema.
/// </remarks>
public sealed class ResetUserPasswordCommand
{
    /// <summary>
    /// Identificador del usuario objetivo cuyo secreto será restablecido.
    /// </summary>
    public Guid TargetUserId { get; init; }

    /// <summary>
    /// Nueva contraseña temporal o definitiva suministrada por el operador autorizado.
    /// </summary>
    public string NewPassword { get; init; } = string.Empty;

    /// <summary>
    /// Confirmación de la nueva contraseña capturada por la interfaz de administración.
    /// </summary>
    public string ConfirmPassword { get; init; } = string.Empty;

    /// <summary>
    /// Identificador opcional del usuario que solicita la operación.
    /// </summary>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>
    /// Dirección IP desde la cual se origina el restablecimiento, cuando está disponible.
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// Canal lógico desde el cual se ejecuta la operación.
    /// </summary>
    public string? Source { get; init; }

    /// <summary>
    /// Referencia externa opcional asociada al restablecimiento.
    /// </summary>
    public string? ExternalReference { get; init; }

    /// <summary>
    /// Motivo funcional registrado para auditoría operativa.
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// Devuelve una representación resumida del comando sin exponer datos sensibles.
    /// </summary>
    /// <returns>Cadena representativa del restablecimiento solicitado.</returns>
    public override string ToString()
    {
        return $"ResetUserPasswordCommand | TargetUserId: {TargetUserId} | RequestedByUserId: {RequestedByUserId} | Source: {Source} | ExternalReference: {ExternalReference}";
    }
}
