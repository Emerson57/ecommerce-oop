using PlataformaECommerce.Application.Common;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Auth.Commands;
using PlataformaECommerce.Application.Features.Auth.DTOs;

namespace PlataformaECommerce.Application.Interfaces.Services.Auth;

/// <summary>
/// Define el contrato del servicio de aplicación encargado de gestionar
/// los casos de uso relacionados con la autenticación de usuarios.
/// </summary>
/// <remarks>
/// Este servicio centraliza la lógica de inicio de sesión y futuras
/// capacidades asociadas al contexto de autenticación, manteniendo
/// desacoplada la capa consumidora de la implementación concreta.
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
}