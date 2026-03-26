namespace PlataformaECommerce.Application.Features.Auth.DTOs;

/// <summary>
/// Representa la solicitud de autenticación de un usuario dentro del sistema.
/// </summary>
/// <remarks>
/// Este DTO se utiliza como contrato de entrada para el proceso de inicio de sesión
/// en la capa Application, desacoplando la información enviada por la capa superior
/// respecto de las entidades del dominio y de la infraestructura de autenticación.
///
/// Su propósito principal es transportar de forma clara, segura y mantenible
/// las credenciales y metadatos necesarios para ejecutar el caso de uso de login,
/// permitiendo que los validadores y servicios de aplicación apliquen las reglas
/// estructurales, de seguridad y de trazabilidad correspondientes.
///
/// Esta clase no debe contener lógica de negocio ni reglas de autenticación.
/// Dichas responsabilidades deben residir en validadores, servicios de aplicación,
/// componentes criptográficos y proveedores de identidad especializados.
/// </remarks>
public sealed class LoginRequestDto
{
    #region Credenciales principales

    /// <summary>
    /// Correo electrónico utilizado por el usuario para autenticarse en el sistema.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Contraseña en texto plano proporcionada por el usuario
    /// para el proceso de autenticación.
    /// </summary>
    /// <remarks>
    /// Este valor debe ser tratado como información sensible
    /// y nunca debe persistirse, registrarse en logs ni exponerse
    /// en respuestas, excepciones o auditorías.
    /// </remarks>
    public string Password { get; init; } = string.Empty;

    #endregion

    #region Opciones de autenticación

    /// <summary>
    /// Indica si el usuario desea mantener la sesión iniciada
    /// durante un periodo más prolongado.
    /// </summary>
    /// <remarks>
    /// Esta propiedad puede utilizarse para definir políticas de expiración
    /// diferenciadas para tokens, cookies o sesiones persistentes.
    /// </remarks>
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
    /// se originó la solicitud, cuando esté disponible.
    /// </summary>
    /// <remarks>
    /// Normalmente corresponde al valor del encabezado User-Agent
    /// o a una representación equivalente del cliente consumidor.
    /// </remarks>
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
    /// Referencia externa opcional asociada a la solicitud de inicio de sesión.
    /// </summary>
    /// <remarks>
    /// Puede representar un identificador de correlación, sesión temporal,
    /// ticket de soporte o referencia funcional útil para observabilidad.
    /// </remarks>
    public string? ExternalReference { get; init; }

    #endregion

    #region Propiedades calculadas

    /// <summary>
    /// Indica si la solicitud contiene un identificador de acceso informado.
    /// </summary>
    public bool HasEmail => !string.IsNullOrWhiteSpace(Email);

    /// <summary>
    /// Indica si la solicitud contiene una contraseña informada.
    /// </summary>
    public bool HasPassword => !string.IsNullOrWhiteSpace(Password);

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida y segura de la solicitud de autenticación.
    /// </summary>
    /// <returns>Cadena representativa del DTO.</returns>
    /// <remarks>
    /// Por motivos de seguridad, esta representación nunca expone la contraseña.
    /// </remarks>
    public override string ToString()
    {
        return $"LoginRequestDto | Email: {Email} | RememberMe: {RememberMe} | Source: {Source} | ExternalReference: {ExternalReference}";
    }

    #endregion
}