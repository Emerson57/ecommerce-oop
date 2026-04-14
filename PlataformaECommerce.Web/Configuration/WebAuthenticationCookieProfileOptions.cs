using System.ComponentModel.DataAnnotations;

namespace PlataformaECommerce.Web.Configuration;

/// <summary>
/// Define el perfil de expiración absoluta e inactividad aplicado a una cookie autenticada.
/// </summary>
public sealed class WebAuthenticationCookieProfileOptions
{
    /// <summary>
    /// Tiempo máximo de inactividad en minutos antes de invalidar la sesión.
    /// </summary>
    [Range(5, 24 * 60)]
    public int SessionIdleTimeoutMinutes { get; set; } = 60;

    /// <summary>
    /// Vida útil absoluta máxima en horas para una sesión persistente.
    /// </summary>
    [Range(1, 30 * 24)]
    public int PersistentSessionAbsoluteLifetimeHours { get; set; } = 8;
}
