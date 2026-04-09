using System.ComponentModel.DataAnnotations;

namespace PlataformaECommerce.Web.Configuration;

/// <summary>
/// Define la configuración antiforgery aplicada por la capa web para formularios y endpoints protegidos por cookies.
/// </summary>
public sealed class WebAntiforgeryOptions
{
    /// <summary>
    /// Nombre de la sección de configuración.
    /// </summary>
    public const string SectionName = "Antiforgery";

    /// <summary>
    /// Nombre de la cookie antiforgery.
    /// </summary>
    [Required]
    public string CookieName { get; set; } = "__Host-PlataformaECommerce.Antiforgery";

    /// <summary>
    /// Nombre del campo oculto usado en formularios HTML.
    /// </summary>
    [Required]
    public string FormFieldName { get; set; } = "__RequestVerificationToken";

    /// <summary>
    /// Nombre del header aceptado para solicitudes AJAX o endpoints JSON protegidos.
    /// </summary>
    [Required]
    public string HeaderName { get; set; } = "RequestVerificationToken";

    /// <summary>
    /// Indica si debe suprimirse el header legado `X-Frame-Options` generado por antiforgery.
    /// </summary>
    public bool SuppressXFrameOptionsHeader { get; set; } = true;
}
