using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Users.DTOs;

namespace PlataformaECommerce.Application.Features.Users.Commands;

/// <summary>
/// Representa el comando de aplicación para desactivar un usuario existente dentro del sistema.
/// </summary>
/// <remarks>
/// Este comando modela una intención explícita de escritura sobre el sistema,
/// correspondiente al caso de uso de inhabilitar un usuario para su operación
/// dentro de la plataforma.
///
/// Su propósito es desacoplar la desactivación del usuario respecto de otras acciones
/// del ciclo de vida, tales como:
/// - registro,
/// - actualización de datos básicos,
/// - confirmación de correo,
/// - cambio de contraseña,
/// - activación.
///
/// La lógica de validación del estado actual del usuario y las reglas del dominio
/// deben resolverse en el servicio de aplicación correspondiente y en la entidad del dominio.
///
/// El resultado esperado de la operación es un <see cref="Result{TValue}"/>
/// que contiene un <see cref="UserDto"/> cuando la ejecución es exitosa.
/// </remarks>
public sealed class DeactivateUserCommand
{
    #region Identificación

    /// <summary>
    /// Identificador único del usuario que será desactivado.
    /// </summary>
    public Guid UserId { get; init; }

    #endregion

    #region Contexto y trazabilidad

    /// <summary>
    /// Identificador del usuario que solicita o ejecuta la desactivación.
    /// </summary>
    /// <remarks>
    /// Este valor puede utilizarse para trazabilidad, auditoría
    /// o control de seguridad cuando la capa superior desee enviarlo explícitamente.
    /// </remarks>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>
    /// Dirección IP desde la cual se originó la solicitud, cuando esté disponible.
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// Canal de origen de la solicitud, cuando la capa superior desee informarlo.
    /// </summary>
    /// <remarks>
    /// Ejemplos comunes:
    /// - Web
    /// - Mobile
    /// - AdminPortal
    /// - InternalTool
    /// </remarks>
    public string? Source { get; init; }

    /// <summary>
    /// Referencia externa opcional asociada al proceso de desactivación.
    /// </summary>
    /// <remarks>
    /// Puede representar un identificador de ticket, correlación funcional
    /// o cualquier referencia útil para observabilidad y seguimiento.
    /// </remarks>
    public string? ExternalReference { get; init; }

    /// <summary>
    /// Motivo funcional o comentario asociado a la desactivación del usuario.
    /// </summary>
    /// <remarks>
    /// Este campo puede ser útil para auditoría, seguimiento operativo,
    /// control administrativo o cumplimiento normativo.
    /// </remarks>
    public string? Reason { get; init; }

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del comando de desactivación de usuario.
    /// </summary>
    /// <returns>Cadena representativa del comando.</returns>
    public override string ToString()
    {
        return $"DeactivateUserCommand | UserId: {UserId} | RequestedByUserId: {RequestedByUserId} | Reason: {Reason}";
    }

    #endregion
}