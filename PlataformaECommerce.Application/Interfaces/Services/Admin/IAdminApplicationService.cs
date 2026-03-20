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
    /// Obtiene el resumen operativo del dashboard administrativo aplicando
    /// los criterios de la consulta suministrada.
    /// </summary>
    /// <param name="query">Consulta del dashboard administrativo.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Resultado con métricas reales y actividad reciente del backoffice.</returns>
    Task<Result<AdminDashboardDto>> GetDashboardAsync(
        GetAdminDashboardQuery query,
        CancellationToken cancellationToken = default);
}
