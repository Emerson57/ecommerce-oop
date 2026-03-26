using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Users.DTOs;

namespace PlataformaECommerce.Application.Features.Users.Commands;

/// <summary>
/// Representa el comando de aplicación para confirmar el correo electrónico
/// de un usuario dentro del sistema.
/// </summary>
/// <remarks>
/// Este comando modela una intención explícita de escritura sobre el sistema,
/// correspondiente al caso de uso de confirmación de correo electrónico.
///
/// Su propósito es desacoplar esta operación respecto de otras acciones
/// del ciclo de vida del usuario, tales como:
/// - registro,
/// - actualización de datos básicos,
/// - cambio de contraseña,
/// - activación o desactivación.
///
/// La lógica de validación del token, código o mecanismo de confirmación
/// debe resolverse en el servicio de aplicación correspondiente y en los servicios auxiliares
/// que la capa Application utilice para este propósito.
///
/// El resultado esperado de la operación es un <see cref="Result{TValue}"/>
/// que contiene un <see cref="UserDto"/> cuando la ejecución es exitosa.
/// </remarks>
public sealed class ConfirmUserEmailCommand
{
    #region Identificación

    /// <summary>
    /// Identificador único del usuario cuyo correo electrónico será confirmado.
    /// </summary>
    public Guid UserId { get; init; }

    #endregion

    #region Información de confirmación

    /// <summary>
    /// Token de confirmación de correo electrónico.
    /// </summary>
    /// <remarks>
    /// Este valor puede corresponder a un token generado por el sistema,
    /// un código firmado, un identificador temporal o cualquier mecanismo
    /// que la implementación concreta utilice para validar la confirmación.
    /// </remarks>
    public string ConfirmationToken { get; init; } = string.Empty;

    /// <summary>
    /// Código de confirmación alternativo o complementario al token.
    /// </summary>
    /// <remarks>
    /// Esta propiedad permite soportar escenarios en los que la confirmación
    /// se realiza mediante códigos de verificación enviados al usuario.
    /// Si la implementación no lo requiere, puede permanecer vacío o nulo.
    /// </remarks>
    public string? ConfirmationCode { get; init; }

    #endregion

    #region Contexto y trazabilidad

    /// <summary>
    /// Identificador del usuario que solicita o ejecuta la confirmación.
    /// </summary>
    /// <remarks>
    /// Normalmente este valor coincide con el propio usuario confirmado,
    /// aunque puede ser informado explícitamente por la capa superior
    /// para fines de auditoría o trazabilidad.
    /// </remarks>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>
    /// Dirección IP desde la cual se originó la solicitud de confirmación,
    /// cuando esté disponible.
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// Canal de origen de la solicitud, cuando la capa superior desee informarlo.
    /// </summary>
    /// <remarks>
    /// Ejemplos comunes:
    /// - Web
    /// - Mobile
    /// - EmailLink
    /// - AdminPortal
    /// </remarks>
    public string? Source { get; init; }

    /// <summary>
    /// Referencia externa opcional asociada al proceso de confirmación.
    /// </summary>
    /// <remarks>
    /// Puede representar un identificador de correlación, ticket
    /// o cualquier referencia funcional útil para trazabilidad.
    /// </remarks>
    public string? ExternalReference { get; init; }

    /// <summary>
    /// Motivo funcional o comentario asociado a la confirmación.
    /// </summary>
    /// <remarks>
    /// Este campo puede ser útil para auditoría, soporte u observabilidad.
    /// </remarks>
    public string? Reason { get; init; }

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del comando de confirmación de correo.
    /// </summary>
    /// <returns>Cadena representativa del comando.</returns>
    public override string ToString()
    {
        return $"ConfirmUserEmailCommand | UserId: {UserId} | HasToken: {!string.IsNullOrWhiteSpace(ConfirmationToken)} | HasCode: {!string.IsNullOrWhiteSpace(ConfirmationCode)} | RequestedByUserId: {RequestedByUserId}";
    }

    #endregion
}