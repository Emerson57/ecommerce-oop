using System.ComponentModel.DataAnnotations;

namespace PlataformaECommerce.Web.Configuration;

/// <summary>
/// Define la configuración de headers de seguridad HTTP aplicada por la capa web.
/// </summary>
public sealed class WebSecurityHeadersOptions
{
    /// <summary>
    /// Nombre de la sección de configuración.
    /// </summary>
    public const string SectionName = "SecurityHeaders";

    /// <summary>
    /// Valor del header <c>Content-Security-Policy</c>.
    /// </summary>
    [Required]
    public string ContentSecurityPolicy { get; set; } = "default-src 'self'; base-uri 'self'; object-src 'none'; frame-ancestors 'none'; img-src 'self' data: https:; style-src 'self' 'unsafe-inline'; script-src 'self' 'unsafe-inline'; font-src 'self' data: https:; connect-src 'self'; form-action 'self'";

    /// <summary>
    /// Valor del header <c>Permissions-Policy</c>.
    /// </summary>
    [Required]
    public string PermissionsPolicy { get; set; } = "camera=(), geolocation=(), microphone=(), payment=(), usb=()";

    /// <summary>
    /// Valor del header <c>Referrer-Policy</c>.
    /// </summary>
    [Required]
    public string ReferrerPolicy { get; set; } = "strict-origin-when-cross-origin";

    /// <summary>
    /// Valor del header <c>X-Frame-Options</c>.
    /// </summary>
    [Required]
    public string FrameOptions { get; set; } = "DENY";

    /// <summary>
    /// Valor del header <c>X-Content-Type-Options</c>.
    /// </summary>
    [Required]
    public string ContentTypeOptions { get; set; } = "nosniff";

    /// <summary>
    /// Valor del header <c>Cross-Origin-Opener-Policy</c>.
    /// </summary>
    [Required]
    public string CrossOriginOpenerPolicy { get; set; } = "same-origin";

    /// <summary>
    /// Valor del header <c>Cross-Origin-Resource-Policy</c>.
    /// </summary>
    [Required]
    public string CrossOriginResourcePolicy { get; set; } = "same-site";

    /// <summary>
    /// Indica si debe emitirse <c>upgrade-insecure-requests</c> dentro de la política CSP.
    /// </summary>
    public bool IncludeUpgradeInsecureRequests { get; set; } = true;
}
