using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Users.DTOs;

namespace PlataformaECommerce.Application.Features.Users.Commands;

/// <summary>
/// Representa el comando de aplicación para registrar un nuevo cliente dentro del sistema.
/// </summary>
/// <remarks>
/// Este comando modela una intención explícita de escritura sobre el sistema,
/// correspondiente al caso de uso de registro de un nuevo cliente.
///
/// Su responsabilidad es transportar los datos necesarios desde la capa superior
/// hacia el caso de uso correspondiente, sin contener lógica de negocio ni reglas
/// de validación complejas, las cuales deben resolverse en:
/// - validadores de Application,
/// - servicios de aplicación,
/// - servicios transversales,
/// - y entidades del dominio.
///
/// El resultado esperado de la operación es un <see cref="Result{TValue}"/>
/// que contiene un <see cref="CustomerDto"/> cuando la ejecución es exitosa.
///
/// Este comando está orientado al proceso de onboarding del cliente
/// y puede ser utilizado desde:
/// - formularios de registro,
/// - endpoints públicos,
/// - procesos de integración,
/// - flujos administrativos controlados.
/// </remarks>
public sealed class RegisterCustomerCommand
{
    #region Información básica del cliente

    /// <summary>
    /// Nombre completo o nombre visible del cliente.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Correo electrónico principal del cliente.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Contraseña en texto plano suministrada durante el proceso de registro.
    /// </summary>
    /// <remarks>
    /// Este valor debe ser tratado exclusivamente como dato de entrada temporal.
    /// El servicio de aplicación correspondiente debe transformarlo mediante un servicio de hashing
    /// antes de construir o persistir la entidad del dominio.
    /// </remarks>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Confirmación de la contraseña suministrada por el cliente.
    /// </summary>
    /// <remarks>
    /// Su objetivo es reforzar la consistencia del proceso de captura
    /// antes de iniciar el caso de uso de registro.
    /// </remarks>
    public string ConfirmPassword { get; init; } = string.Empty;

    #endregion

    #region Información opcional de perfil comercial

    /// <summary>
    /// Colección de preferencias iniciales declaradas por el cliente.
    /// </summary>
    /// <remarks>
    /// Estas preferencias pueden utilizarse posteriormente para personalización,
    /// segmentación comercial o experiencia de usuario.
    /// </remarks>
    public IReadOnlyCollection<string> Preferences { get; init; } = Array.Empty<string>();

    #endregion

    #region Consentimientos y contexto de registro

    /// <summary>
    /// Indica si el cliente acepta los términos y condiciones del sistema.
    /// </summary>
    public bool AcceptTermsAndConditions { get; init; }

    /// <summary>
    /// Indica si el cliente acepta el tratamiento de datos personales.
    /// </summary>
    public bool AcceptPrivacyPolicy { get; init; }

    /// <summary>
    /// Indica si el cliente desea recibir comunicaciones comerciales.
    /// </summary>
    public bool AcceptMarketingCommunications { get; init; }

    /// <summary>
    /// Dirección IP desde la cual se originó la solicitud de registro, cuando esté disponible.
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// Canal de origen del registro, cuando la capa superior desee informarlo.
    /// </summary>
    /// <remarks>
    /// Ejemplos comunes:
    /// - Web
    /// - Mobile
    /// - AdminPortal
    /// - LandingPage
    /// </remarks>
    public string? Source { get; init; }

    /// <summary>
    /// Referencia externa opcional asociada al proceso de registro.
    /// </summary>
    /// <remarks>
    /// Puede representar un identificador de campaña, ticket, correlación
    /// o cualquier referencia funcional útil para trazabilidad.
    /// </remarks>
    public string? ExternalReference { get; init; }

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del comando de registro de cliente.
    /// </summary>
    /// <returns>Cadena representativa del comando.</returns>
    public override string ToString()
    {
        return $"RegisterCustomerCommand | Name: {Name} | Email: {Email} | Preferences: {Preferences.Count} | AcceptTermsAndConditions: {AcceptTermsAndConditions} | AcceptPrivacyPolicy: {AcceptPrivacyPolicy}";
    }

    #endregion
}