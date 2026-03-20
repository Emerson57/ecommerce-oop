namespace PlataformaECommerce.Application.Features.Admin.DTOs;

/// <summary>
/// Representa la solicitud de registro de un administrador dentro de la capa de aplicación.
/// </summary>
/// <remarks>
/// Este DTO se utiliza para transportar la información necesaria para registrar
/// un nuevo administrador dentro del sistema, desacoplando la entrada externa
/// respecto de las entidades del dominio.
///
/// Su propósito es servir como contrato de entrada para:
/// - endpoints HTTP administrativos,
/// - comandos de aplicación,
/// - servicios de aplicación,
/// - flujos de aprovisionamiento interno,
/// - procesos de onboarding organizacional.
///
/// La estructura contiene únicamente datos de transporte y no debe incluir
/// lógica de negocio ni reglas de validación complejas, las cuales deben
/// resolverse en la capa Application mediante validadores especializados
/// y, posteriormente, reforzarse en el dominio.
/// </remarks>
public sealed class RegisterAdminRequestDto
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
    /// La capa Application debe transformarlo mediante un servicio de hashing
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
    /// Devuelve una representación resumida de la solicitud de registro de administrador.
    /// </summary>
    /// <returns>Cadena representativa de la solicitud.</returns>
    public override string ToString()
    {
        return $"RegisterAdminRequestDto | Name: {Name} | Email: {Email} | Area: {Area} | IsActive: {IsActive} | IsEmailConfirmed: {IsEmailConfirmed} | RequestedByUserId: {RequestedByUserId}";
    }

    #endregion
}