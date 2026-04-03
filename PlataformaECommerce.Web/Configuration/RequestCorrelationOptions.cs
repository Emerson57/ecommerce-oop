using System.ComponentModel.DataAnnotations;

namespace PlataformaECommerce.Web.Configuration;

/// <summary>
/// Define la configuración de correlación de solicitudes HTTP para trazabilidad operativa.
/// </summary>
public sealed class RequestCorrelationOptions
{
    /// <summary>
    /// Nombre de la sección de configuración.
    /// </summary>
    public const string SectionName = "Observability";

    /// <summary>
    /// Nombre del header entrante y saliente utilizado para correlación.
    /// </summary>
    [Required]
    public string CorrelationHeaderName { get; set; } = "X-Correlation-ID";

    /// <summary>
    /// Indica si la aplicación debe devolver el identificador de correlación en la respuesta.
    /// </summary>
    public bool EmitResponseHeader { get; set; } = true;

    /// <summary>
    /// Longitud máxima aceptada para un identificador de correlación proporcionado externamente.
    /// </summary>
    [Range(16, 256)]
    public int MaxCorrelationIdLength { get; set; } = 128;
}
