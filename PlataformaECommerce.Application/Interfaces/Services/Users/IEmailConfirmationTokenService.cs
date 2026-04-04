using PlataformaECommerce.Application.Features.Users.DTOs;
using PlataformaECommerce.Domain.Entities.Users;

namespace PlataformaECommerce.Application.Interfaces.Services.Users;

/// <summary>
/// Define el contrato encargado de emitir y validar tokens temporales de confirmación de correo.
/// </summary>
public interface IEmailConfirmationTokenService
{
    /// <summary>
    /// Genera un token temporal de confirmación para el usuario indicado.
    /// </summary>
    string GenerateToken(Usuario usuario, TimeSpan lifetime);

    /// <summary>
    /// Valida y desprotege un token temporal de confirmación de correo.
    /// </summary>
    EmailConfirmationTokenValidationDto? ValidateToken(string token);
}
