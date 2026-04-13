using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace PlataformaECommerce.Web.Configuration;

/// <summary>
/// Define la configuración endurecida de cookies de autenticación para sesiones del storefront y del backoffice.
/// </summary>
public sealed class WebAuthenticationCookiesOptions
{
    /// <summary>
    /// Nombre de la sección de configuración.
    /// </summary>
    public const string SectionName = "AuthenticationCookies";

    /// <summary>
    /// Tiempo máximo de inactividad permitido para una sesión autenticada antes de expirar.
    /// </summary>
    [Range(5, 1440)]
    public int SessionIdleTimeoutMinutes { get; set; } = 480;

    /// <summary>
    /// Vida útil absoluta máxima permitida para sesiones persistentes.
    /// </summary>
    [Range(1, 168)]
    public int PersistentSessionAbsoluteLifetimeHours { get; set; } = 24;

    /// <summary>
    /// Indica si la cookie puede renovarse automáticamente mientras la sesión siga activa.
    /// </summary>
    public bool SlidingExpiration { get; set; } = true;

    /// <summary>
    /// Política SameSite aplicada a las cookies autenticadas.
    /// </summary>
    public SameSiteMode SameSite { get; set; } = SameSiteMode.Lax;

    /// <summary>
    /// Política Secure aplicada a las cookies autenticadas.
    /// </summary>
    public CookieSecurePolicy SecurePolicy { get; set; } = CookieSecurePolicy.Always;

    /// <summary>
    /// Indica si las cookies autenticadas deben ser accesibles solo por HTTP.
    /// </summary>
    public bool HttpOnly { get; set; } = true;

    /// <summary>
    /// Dominio compartido opcional para reutilizar cookies autenticadas entre instancias bajo subdominios del mismo e-commerce.
    /// </summary>
    public string? SharedCookieDomain { get; set; }

    /// <summary>
    /// Indica si las cookies autenticadas son esenciales para la operación del sitio.
    /// </summary>
    public bool IsEssential { get; set; } = true;
}
