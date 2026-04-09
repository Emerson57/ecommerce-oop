using System.ComponentModel.DataAnnotations;

namespace PlataformaECommerce.Web.Security;

/// <summary>
/// Define las directivas configurables de Content Security Policy para la aplicación web.
/// </summary>
public sealed class ContentSecurityPolicyOptions
{
    /// <summary>
    /// Indica si la aplicación debe emitir Content Security Policy.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Indica si Development debe emitir la política en modo report-only.
    /// </summary>
    public bool UseReportOnlyInDevelopment { get; set; }

    /// <summary>
    /// Indica si debe agregarse <c>upgrade-insecure-requests</c> fuera de Development.
    /// </summary>
    public bool IncludeUpgradeInsecureRequests { get; set; } = true;

    [Required]
    public string[] DefaultSources { get; set; } = ["'self'"];

    [Required]
    public string[] BaseUriSources { get; set; } = ["'self'"];

    [Required]
    public string[] ObjectSources { get; set; } = ["'none'"];

    [Required]
    public string[] FrameAncestorSources { get; set; } = ["'none'"];

    [Required]
    public string[] ImageSources { get; set; } = ["'self'", "data:", "https:"];

    [Required]
    public string[] StyleSources { get; set; } = ["'self'"];

    [Required]
    public string[] ScriptSources { get; set; } = ["'self'"];

    [Required]
    public string[] FontSources { get; set; } = ["'self'", "data:"];

    [Required]
    public string[] ConnectSources { get; set; } = ["'self'"];

    [Required]
    public string[] FormActionSources { get; set; } = ["'self'"];
}
