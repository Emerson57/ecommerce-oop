using System;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Auth.DTOs;

namespace PlataformaECommerce.Application.Features.Auth.Commands;

/// <summary>
/// Representa el comando de aplicación para ejecutar el proceso
/// de autenticación de un usuario dentro del sistema.
/// </summary>
/// <remarks>
/// Este comando modela una intención explícita de escritura/seguridad,
/// correspondiente al caso de uso de inicio de sesión.
///
/// Su responsabilidad es transportar de forma desacoplada las credenciales
/// y metadatos de contexto necesarios para que la capa Application procese
/// la autenticación de manera segura, trazable y mantenible.
///
/// Esta clase no debe contener lógica de autenticación, hashing,
/// generación de tokens ni validaciones complejas. Dichas responsabilidades
/// pertenecen a:
/// - validadores de Application,
/// - servicios de autenticación,
/// - proveedores criptográficos,
/// - y componentes especializados de infraestructura.
///
/// El resultado esperado de la operación es un <see cref="Result{TValue}"/>
/// que contiene un <see cref="AuthResponseDto"/> cuando la autenticación
/// se realiza correctamente.
/// </remarks>
public sealed class LoginCommand
{
    #region Credenciales principales

    /// <summary>
    /// Correo electrónico utilizado por el usuario para autenticarse.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Contraseña en texto plano proporcionada por el usuario
    /// para el proceso de autenticación.
    /// </summary>
    /// <remarks>
    /// Este valor debe tratarse como información altamente sensible
    /// y nunca debe registrarse en logs ni persistirse en texto plano.
    /// </remarks>
    public string Password { get; init; } = string.Empty;

    #endregion

    #region Opciones de autenticación

    /// <summary>
    /// Indica si se solicita una sesión persistente o prolongada.
    /// </summary>
    public bool RememberMe { get; init; }

    #endregion

    #region Contexto y trazabilidad

    /// <summary>
    /// Dirección IP desde la cual se originó la solicitud de autenticación,
    /// cuando dicho dato esté disponible.
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// Información del agente cliente o dispositivo desde el cual
    /// se originó la solicitud.
    /// </summary>
    public string? UserAgent { get; init; }

    /// <summary>
    /// Canal de origen desde el cual se ejecuta el proceso de autenticación.
    /// </summary>
    /// <remarks>
    /// Ejemplos comunes:
    /// - Web
    /// - Mobile
    /// - AdminPortal
    /// - ApiClient
    /// </remarks>
    public string? Source { get; init; }

    /// <summary>
    /// Referencia funcional externa asociada a la solicitud,
    /// cuando la capa superior desee informarla.
    /// </summary>
    public string? ExternalReference { get; init; }

    /// <summary>
    /// Fecha y hora UTC en la que la capa superior registró
    /// la solicitud de autenticación.
    /// </summary>
    public DateTime? RequestedAtUtc { get; init; }

    #endregion

    #region Propiedades calculadas

    /// <summary>
    /// Indica si el comando contiene un identificador de acceso informado.
    /// </summary>
    public bool HasEmail => !string.IsNullOrWhiteSpace(Email);

    /// <summary>
    /// Indica si el comando contiene una contraseña informada.
    /// </summary>
    public bool HasPassword => !string.IsNullOrWhiteSpace(Password);

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida y segura del comando.
    /// </summary>
    /// <returns>Cadena representativa del comando.</returns>
    /// <remarks>
    /// Por motivos de seguridad, esta representación no expone la contraseña.
    /// </remarks>
    public override string ToString()
    {
        return $"LoginCommand | Email: {Email} | RememberMe: {RememberMe} | Source: {Source} | ExternalReference: {ExternalReference}";
    }

    #endregion
}