using System.ComponentModel.DataAnnotations;
using PlataformaECommerce.Web.Security;

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
    /// Define la configuración detallada de <c>Content-Security-Policy</c>.
    /// </summary>
    [Required]
    public ContentSecurityPolicyOptions ContentSecurityPolicy { get; set; } = new();

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

}
