using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Admin.DTOs;

namespace PlataformaECommerce.Application.Features.Admin.Commands;

/// <summary>
/// Representa el comando de aplicación para registrar un nuevo administrador dentro del sistema.
/// </summary>
/// <remarks>
/// Este comando modela una intención explícita de escritura sobre el sistema,
/// correspondiente al caso de uso de registro de un nuevo administrador.
///
/// Su responsabilidad es transportar los datos necesarios desde la capa superior
/// hacia el caso de uso correspondiente, sin contener lógica de negocio ni reglas
/// de validación complejas, las cuales deben resolverse en:
/// - validadores de Application,
/// - servicios de aplicación,
/// - servicios transversales,
/// - y entidades del dominio.
///
/// El resultado esperado de la operación es un <see cref="Result{TValue}"/>
/// que contiene un <see cref="AdminDto"/> cuando la ejecución es exitosa.
///
/// Este comando está orientado a procesos de aprovisionamiento interno
/// y administración del sistema, y puede ser utilizado desde:
/// - portales administrativos,
/// - herramientas internas,
/// - scripts de bootstrap,
/// - procesos controlados de onboarding organizacional.
/// </remarks>
public sealed class RegisterAdminCommand
{
    #region Información básica del administrador

    /// <summary>
    /// Nombre completo o nombre visible del administrador.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Correo electrónico principal del administrador.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Contraseña en texto plano suministrada durante el proceso de registro.
    /// </summary>
    /// <remarks>
    /// Este valor debe ser tratado exclusivamente como dato de entrada temporal.
    /// El servicio de aplicación correspondiente debe transformarlo mediante un servicio de hashing
    /// antes de construir o persistir la entidad del dominio.
    /// </remarks>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Confirmación de la contraseña suministrada por el solicitante.
    /// </summary>
    /// <remarks>
    /// Su objetivo es reforzar la consistencia del proceso de captura
    /// antes de iniciar el caso de uso de registro.
    /// </remarks>
    public string ConfirmPassword { get; init; } = string.Empty;

    #endregion

    #region Información organizacional

    /// <summary>
    /// Área o dependencia organizacional a la que pertenecerá el administrador.
    /// </summary>
    public string Area { get; init; } = string.Empty;

    /// <summary>
    /// Indica si la cuenta del administrador debe crearse inicialmente activa.
    /// </summary>
    public bool IsActive { get; init; } = true;

    /// <summary>
    /// Indica si el correo electrónico del administrador debe considerarse confirmado
    /// desde el momento del registro.
    /// </summary>
    /// <remarks>
    /// Esta propiedad resulta útil en escenarios administrativos controlados
    /// donde el alta es realizada por personal autorizado.
    /// </remarks>
    public bool IsEmailConfirmed { get; init; }

    #endregion

    #region Contexto y trazabilidad del registro

    /// <summary>
    /// Identificador del usuario que solicita o ejecuta el registro del administrador.
    /// </summary>
    /// <remarks>
    /// Puede utilizarse para trazabilidad, auditoría y control de seguridad
    /// cuando la capa superior desee enviarlo explícitamente.
    /// </remarks>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>
    /// Dirección IP desde la cual se originó la solicitud de registro, cuando esté disponible.
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// Canal de origen del registro, cuando la capa superior desee informarlo.
    /// </summary>
    /// <remarks>
    /// Ejemplos comunes:
    /// - AdminPortal
    /// - InternalTool
    /// - MigrationProcess
    /// - BootstrapScript
    /// </remarks>
    public string? Source { get; init; }

    /// <summary>
    /// Referencia externa opcional asociada al proceso de registro.
    /// </summary>
    /// <remarks>
    /// Puede representar un identificador de ticket, solicitud interna,
    /// correlación operativa o cualquier referencia funcional útil para trazabilidad.
    /// </remarks>
    public string? ExternalReference { get; init; }

    /// <summary>
    /// Motivo funcional o comentario asociado a la creación del administrador.
    /// </summary>
    /// <remarks>
    /// Este campo puede ser útil para auditoría y seguimiento operativo.
    /// </remarks>
    public string? Reason { get; init; }

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del comando de registro de administrador.
    /// </summary>
    /// <returns>Cadena representativa del comando.</returns>
    public override string ToString()
    {
        return $"RegisterAdminCommand | Name: {Name} | Email: {Email} | Area: {Area} | IsActive: {IsActive} | IsEmailConfirmed: {IsEmailConfirmed} | RequestedByUserId: {RequestedByUserId}";
    }

    #endregion
}