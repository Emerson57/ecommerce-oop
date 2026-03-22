using PlataformaECommerce.Application.Features.Auth.DTOs;
using PlataformaECommerce.Domain.Entities.Users;

namespace PlataformaECommerce.Application.Interfaces.Services.Auth;

/// <summary>
/// Define el contrato encargado de emitir y validar tokens temporales de recuperación de contraseña.
/// </summary>
/// <remarks>
/// Este contrato abstrae la tecnología concreta de protección y expiración del token,
/// permitiendo que la capa Application mantenga el control del flujo sin depender del detalle
/// de Data Protection u otros mecanismos criptográficos de infraestructura.
/// </remarks>
public interface IPasswordResetTokenService
{
    /// <summary>
    /// Genera un token temporal de recuperación de contraseña para el usuario indicado.
    /// </summary>
    string GenerateToken(Usuario usuario, TimeSpan lifetime);

    /// <summary>
    /// Valida y desprotege un token temporal de recuperación.
    /// </summary>
    PasswordResetTokenValidationDto? ValidateToken(string token);
}
