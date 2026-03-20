using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Auth.Commands;
using PlataformaECommerce.Application.Features.Auth.DTOs;
using PlataformaECommerce.Application.Features.Auth.Queries;

namespace PlataformaECommerce.Application.Interfaces.Services.Auth;

/// <summary>
/// Define el contrato del servicio de aplicación encargado de gestionar
/// los casos de uso relacionados con la autenticación de usuarios.
/// </summary>
/// <remarks>
/// Este contrato define la frontera pública del módulo de autenticación dentro de
/// <c>Application</c>. Sus operaciones representan casos de uso ejecutados por un
/// servicio de aplicación, utilizando comandos y consultas únicamente como modelos
/// de entrada para expresar la intención del flujo solicitado.
/// </remarks>
public interface IAuthApplicationService
{
    /// <summary>
    /// Autentica a un usuario mediante su correo electrónico y contraseña.
    /// </summary>
    /// <param name="command">
    /// Comando que contiene las credenciales necesarias para iniciar sesión.
    /// </param>
    /// <param name="cancellationToken">
    /// Token de cancelación para interrumpir la operación de forma controlada.
    /// </param>
    /// <returns>
    /// Un resultado que contiene la respuesta de autenticación en caso de éxito,
    /// o la información del error en caso de fallo.
    /// </returns>
    Task<Result<AuthResponseDto>> LoginAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la información del usuario autenticado actual.
    /// </summary>
    /// <param name="query">Consulta del usuario actual.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado que contiene la información resumida del usuario autenticado.
    /// </returns>
    Task<Result<CurrentUserDto>> GetCurrentUserAsync(
        GetCurrentUserQuery query,
        CancellationToken cancellationToken = default);
}