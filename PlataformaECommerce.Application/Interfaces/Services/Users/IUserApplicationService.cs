using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Users.Commands;
using PlataformaECommerce.Application.Features.Users.DTOs;
using PlataformaECommerce.Application.Features.Users.Queries;

namespace PlataformaECommerce.Application.Interfaces.Services.Users;

/// <summary>
/// Define el contrato del servicio de aplicación encargado de coordinar
/// los casos de uso del módulo de usuarios.
/// </summary>
/// <remarks>
/// Este contrato constituye la frontera pública del módulo de usuarios dentro de
/// <c>Application</c>. Los comandos y consultas asociados a sus métodos se utilizan
/// como modelos de entrada del caso de uso, manteniendo desacoplada la capa consumidora.
/// </remarks>
public interface IUserApplicationService
{
    /// <summary>
    /// Registra un nuevo cliente dentro del sistema.
    /// </summary>
    Task<Result<CustomerDto>> RegisterCustomerAsync(
        RegisterCustomerCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza la información básica de un usuario existente.
    /// </summary>
    Task<Result<UserDto>> UpdateUserBasicDataAsync(
        UpdateUserBasicDataCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirma el correo electrónico de un usuario existente.
    /// </summary>
    Task<Result<UserDto>> ConfirmUserEmailAsync(
        ConfirmUserEmailCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reenvía el correo de confirmación para una cuenta no confirmada.
    /// </summary>
    Task<Result> ResendUserEmailConfirmationAsync(
        ResendUserEmailConfirmationCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Activa un usuario existente dentro del sistema.
    /// </summary>
    Task<Result<UserDto>> ActivateUserAsync(
        ActivateUserCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Desactiva un usuario existente dentro del sistema.
    /// </summary>
    Task<Result<UserDto>> DeactivateUserAsync(
        DeactivateUserCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un usuario por su identificador único.
    /// </summary>
    Task<Result<UserDto>> GetUserByIdAsync(
        GetUserByIdQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un usuario por su correo electrónico.
    /// </summary>
    Task<Result<UserDto>> GetUserByEmailAsync(
        GetUserByEmailQuery query,
        CancellationToken cancellationToken = default);
}
