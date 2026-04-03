using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Admin.DTOs;
using PlataformaECommerce.Application.Features.Admin.Queries;

namespace PlataformaECommerce.Application.Interfaces.Services.Admin;

/// <summary>
/// Define la frontera de lectura analítica del dashboard administrativo.
/// </summary>
public interface IAdminDashboardService
{
    /// <summary>
    /// Obtiene el resumen operativo del dashboard administrativo.
    /// </summary>
    Task<Result<AdminDashboardDto>> GetDashboardAsync(
        GetAdminDashboardQuery query,
        CancellationToken cancellationToken = default);
}
