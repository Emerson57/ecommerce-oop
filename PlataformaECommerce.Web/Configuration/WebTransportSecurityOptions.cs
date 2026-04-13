using System.ComponentModel.DataAnnotations;

namespace PlataformaECommerce.Web.Configuration;

/// <summary>
/// Define la configuración de transporte seguro HTTP aplicada por ambiente.
/// </summary>
public sealed class WebTransportSecurityOptions
{
    /// <summary>
    /// Nombre de la sección de configuración.
    /// </summary>
    public const string SectionName = "TransportSecurity";

    /// <summary>
    /// Indica si HSTS debe activarse fuera de Development.
    /// </summary>
    public bool HstsEnabled { get; set; } = true;

    /// <summary>
    /// Tiempo máximo en días anunciado por HSTS.
    /// </summary>
    [Range(1, 730)]
    public int HstsMaxAgeDays { get; set; } = 30;

    /// <summary>
    /// Indica si HSTS debe cubrir subdominios.
    /// </summary>
    public bool IncludeSubDomains { get; set; }

    /// <summary>
    /// Indica si la aplicación solicita precarga HSTS en navegadores compatibles.
    /// </summary>
    public bool Preload { get; set; }
}
