using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Admin.Commands;
using PlataformaECommerce.Application.Features.Admin.DTOs;
using PlataformaECommerce.Application.Features.Admin.Queries;

namespace PlataformaECommerce.Application.Interfaces.Services.Admin;

/// <summary>
/// Define el contrato del servicio de aplicación encargado de coordinar
/// los casos de uso administrativos del backoffice.
/// </summary>
/// <remarks>
/// Este contrato constituye la frontera pública del módulo <c>Admin</c> dentro de
/// <c>Application</c>. Los comandos que recibe representan solicitudes del caso de uso
/// administrativo y son procesados por un servicio de aplicación especializado.
/// </remarks>
public interface IAdminApplicationService
{
    /// <summary>
    /// Registra un nuevo administrador dentro del sistema.
    /// </summary>
    /// <param name="command">Comando de registro del administrador.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con la representación del administrador registrado cuando la operación es exitosa.
    /// </returns>
    Task<Result<AdminDto>> RegisterAdminAsync(
        RegisterAdminCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la definición funcional requerida por el formulario de creación de administradores del backoffice.
    /// </summary>
    /// <param name="query">Consulta del caso de uso de alta administrativa.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Resultado con la definición funcional del caso de uso cuando el acceso es válido.</returns>
    Task<Result<AdminRegistrationDefinitionDto>> GetAdminRegistrationDefinitionAsync(
        GetAdminRegistrationDefinitionQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el resumen operativo del dashboard administrativo aplicando
    /// los criterios de la consulta suministrada.
    /// </summary>
    /// <param name="query">Consulta del dashboard administrativo.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Resultado con métricas reales y actividad reciente del backoffice.</returns>
    Task<Result<AdminDashboardDto>> GetDashboardAsync(
        GetAdminDashboardQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el resumen consolidado del backoffice de usuarios del sistema.
    /// </summary>
    /// <param name="query">Consulta del módulo administrativo de usuarios.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Resultado con métricas y usuarios proyectados para el backoffice.</returns>
    Task<Result<AdminUsersBackofficeDto>> GetUsersAsync(
        GetAdminUsersQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Restablece administrativamente la contraseña de un usuario del sistema.
    /// </summary>
    /// <param name="command">Solicitud administrativa de restablecimiento.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Resultado con la proyección actualizada del usuario afectado.</returns>
    Task<Result<AdminBackofficeUserDto>> ResetUserPasswordAsync(
        ResetUserPasswordCommand command,
        CancellationToken cancellationToken = default);
}
