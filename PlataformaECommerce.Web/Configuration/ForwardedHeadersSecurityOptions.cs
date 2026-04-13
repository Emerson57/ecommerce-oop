using System.ComponentModel.DataAnnotations;

namespace PlataformaECommerce.Web.Configuration;

/// <summary>
/// Define la configuración de confianza aplicada al procesamiento de headers reenviados por proxies o balanceadores.
/// </summary>
public sealed class ForwardedHeadersSecurityOptions
{
    /// <summary>
    /// Nombre de la sección de configuración.
    /// </summary>
    public const string SectionName = "ForwardedHeadersSecurity";

    /// <summary>
    /// Indica si la aplicación debe procesar headers reenviados de proxies confiables.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Limita el número máximo de proxies reenviados que serán procesados por la aplicación.
    /// </summary>
    [Range(1, 10)]
    public int ForwardLimit { get; set; } = 1;

    /// <summary>
    /// Indica si los headers reenviados deben mantener simetría en la cadena de proxies.
    /// </summary>
    public bool RequireHeaderSymmetry { get; set; } = true;

    /// <summary>
    /// Indica si la aplicación puede confiar en `X-Forwarded-Host` cuando proviene de proxies confiables.
    /// </summary>
    public bool TrustForwardedHost { get; set; }

    /// <summary>
    /// Lista explícita de direcciones IP de proxies confiables.
    /// </summary>
    public List<string> TrustedProxies { get; set; } = [];

    /// <summary>
    /// Lista explícita de redes confiables en formato CIDR.
    /// </summary>
    public List<string> TrustedNetworks { get; set; } = [];

    /// <summary>
    /// Lista explícita de hosts públicos permitidos cuando se acepta `X-Forwarded-Host`.
    /// </summary>
    public List<string> AllowedHosts { get; set; } = [];
}
