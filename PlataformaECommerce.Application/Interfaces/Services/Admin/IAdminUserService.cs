using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Admin.Commands;
using PlataformaECommerce.Application.Features.Admin.DTOs;
using PlataformaECommerce.Application.Features.Admin.Queries;

namespace PlataformaECommerce.Application.Interfaces.Services.Admin;

/// <summary>
/// Define la frontera de operaciones administrativas sobre usuarios y aprovisionamiento.
/// </summary>
public interface IAdminUserService
{
    /// <summary>
    /// Registra un nuevo administrador dentro del sistema.
    /// </summary>
    Task<Result<AdminDto>> RegisterAdminAsync(
        RegisterAdminCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la definición funcional requerida por el formulario de creación de administradores.
    /// </summary>
    Task<Result<AdminRegistrationDefinitionDto>> GetAdminRegistrationDefinitionAsync(
        GetAdminRegistrationDefinitionQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el resumen consolidado del backoffice de usuarios del sistema.
    /// </summary>
    Task<Result<AdminUsersBackofficeDto>> GetUsersAsync(
        GetAdminUsersQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Restablece administrativamente la contraseña de un usuario del sistema.
    /// </summary>
    Task<Result<AdminBackofficeUserDto>> ResetUserPasswordAsync(
        ResetUserPasswordCommand command,
        CancellationToken cancellationToken = default);
}
