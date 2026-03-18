using PlataformaECommerce.Application.Abstractions;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Users.DTOs;

namespace PlataformaECommerce.Application.Features.Users.Commands;

/// <summary>
/// Representa el comando de aplicación para actualizar la información básica
/// de un usuario existente dentro del sistema.
/// </summary>
/// <remarks>
/// Este comando modela una intención explícita de escritura sobre el sistema,
/// correspondiente al caso de uso de modificación de los datos básicos
/// de un usuario, como su nombre y correo electrónico.
///
/// Su propósito es desacoplar esta operación respecto de otras acciones
/// más sensibles o especializadas del usuario, tales como:
/// - cambio de contraseña,
/// - confirmación de correo,
/// - activación o desactivación,
/// - asignación de roles.
///
/// La lógica de validación estructural y consistencia de entrada debe resolverse
/// en validadores de Application, mientras que las reglas de negocio definitivas
/// deben reforzarse en el dominio.
///
/// El resultado esperado de la operación es un <see cref="Result{TValue}"/>
/// que contiene un <see cref="UserDto"/> cuando la ejecución es exitosa.
/// </remarks>
public sealed class UpdateUserBasicDataCommand : ICommand<Result<UserDto>>
{
    #region Identificación

    /// <summary>
    /// Identificador único del usuario que será actualizado.
    /// </summary>
    public Guid UserId { get; init; }

    #endregion

    #region Información básica a modificar

    /// <summary>
    /// Nuevo nombre completo o nombre visible del usuario.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Nuevo correo electrónico principal del usuario.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    #endregion

    #region Contexto y trazabilidad

    /// <summary>
    /// Identificador del usuario que solicita o ejecuta la actualización.
    /// </summary>
    /// <remarks>
    /// Puede utilizarse para trazabilidad, auditoría y control de seguridad
    /// cuando la capa superior desee enviarlo explícitamente.
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
    /// Referencia externa opcional asociada al proceso de actualización.
    /// </summary>
    /// <remarks>
    /// Puede representar un identificador de ticket, correlación funcional
    /// o cualquier referencia útil para observabilidad y seguimiento.
    /// </remarks>
    public string? ExternalReference { get; init; }

    /// <summary>
    /// Motivo funcional o comentario asociado a la actualización.
    /// </summary>
    /// <remarks>
    /// Este campo puede ser útil para auditoría, seguimiento operativo
    /// o control administrativo.
    /// </remarks>
    public string? Reason { get; init; }

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del comando de actualización de usuario.
    /// </summary>
    /// <returns>Cadena representativa del comando.</returns>
    public override string ToString()
    {
        return $"UpdateUserBasicDataCommand | UserId: {UserId} | Name: {Name} | Email: {Email} | RequestedByUserId: {RequestedByUserId}";
    }

    #endregion
}