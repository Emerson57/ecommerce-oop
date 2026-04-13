using System.ComponentModel.DataAnnotations;

namespace PlataformaECommerce.Web.Configuration;

/// <summary>
/// Define la exposición operativa de OpenAPI por ambiente y sus controles de acceso.
/// </summary>
public sealed class WebOpenApiSecurityOptions
{
    /// <summary>
    /// Nombre de la sección de configuración.
    /// </summary>
    public const string SectionName = "OpenApiSecurity";

    /// <summary>
    /// Indica si OpenAPI puede exponerse en Development.
    /// </summary>
    public bool EnabledInDevelopment { get; set; } = true;

    /// <summary>
    /// Indica si OpenAPI puede exponerse en entornos tipo QA o Staging.
    /// </summary>
    public bool EnabledInQualityAssurance { get; set; }

    /// <summary>
    /// Indica si OpenAPI puede exponerse en Production.
    /// </summary>
    public bool EnabledInProduction { get; set; }

    /// <summary>
    /// Indica si OpenAPI debe requerir autorización fuera de Development.
    /// </summary>
    public bool RequireAuthorizationOutsideDevelopment { get; set; } = true;

    /// <summary>
    /// Política de autorización requerida para OpenAPI fuera de Development.
    /// </summary>
    [Required]
    public string RequiredPolicy { get; set; } = "SuperUserOnly";
}
