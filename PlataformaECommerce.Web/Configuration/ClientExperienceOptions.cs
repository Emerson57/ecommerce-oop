using System.ComponentModel.DataAnnotations;

namespace PlataformaECommerce.Web.Configuration;

/// <summary>
/// Define la configuración comercial y operativa de una única marca cliente activa en la plataforma.
/// </summary>
public sealed class ClientExperienceOptions
{
    /// <summary>
    /// Nombre de la sección de configuración.
    /// </summary>
    public const string SectionName = "ClientExperience";

    /// <summary>
    /// Identificador lógico del cliente configurado en la instancia actual.
    /// </summary>
    [Required]
    [MaxLength(80)]
    public string ClientId { get; set; } = "novashop-default";

    /// <summary>
    /// Nombre visible del storefront.
    /// </summary>
    [Required]
    [MaxLength(120)]
    public string StorefrontName { get; set; } = "NovaShop";

    /// <summary>
    /// Nombre visible del backoffice.
    /// </summary>
    [Required]
    [MaxLength(120)]
    public string BackofficeName { get; set; } = "NovaShop Backoffice";

    /// <summary>
    /// Mensaje corto de posicionamiento comercial para la marca.
    /// </summary>
    [Required]
    [MaxLength(240)]
    public string StorefrontTagline { get; set; } = "Compra con una experiencia segura, trazable y preparada para crecer.";

    /// <summary>
    /// Badge principal visible en la portada comercial.
    /// </summary>
    [Required]
    [MaxLength(80)]
    public string HomeHeroBadge { get; set; } = "Temporada de ofertas";

    /// <summary>
    /// Título principal de la portada comercial.
    /// </summary>
    [Required]
    [MaxLength(180)]
    public string HomeHeroTitle { get; set; } = "Descubre productos que elevan tu estilo y tu día a día";

    /// <summary>
    /// Título promocional visible en la franja inferior de la portada.
    /// </summary>
    [Required]
    [MaxLength(180)]
    public string HomePromoTitle { get; set; } = "Compra con envío rápido y promociones semanales";

    /// <summary>
    /// Nombre legal o comercial que debe mostrarse en el footer y documentación operativa.
    /// </summary>
    [Required]
    [MaxLength(160)]
    public string LegalCompanyName { get; set; } = "NovaShop Commerce";

    /// <summary>
    /// Correo electrónico de contacto para soporte funcional y técnico.
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(160)]
    public string SupportEmail { get; set; } = "support@example.com";

    /// <summary>
    /// Teléfono principal de soporte.
    /// </summary>
    [Required]
    [MaxLength(40)]
    public string SupportPhone { get; set; } = "+57 300 000 0000";

    /// <summary>
    /// Horario operativo informado al cliente para atención.
    /// </summary>
    [Required]
    [MaxLength(120)]
    public string SupportHours { get; set; } = "Lunes a viernes, 08:00 a 18:00 UTC-5";

    /// <summary>
    /// Compromiso base de tiempo de respuesta para soporte.
    /// </summary>
    [Required]
    [MaxLength(120)]
    public string SupportSla { get; set; } = "Respuesta inicial en menos de 8 horas hábiles.";

    /// <summary>
    /// Color primario de la marca en formato hexadecimal CSS.
    /// </summary>
    [Required]
    [MaxLength(16)]
    public string PrimaryColor { get; set; } = "#111827";

    /// <summary>
    /// Color de acento de la marca en formato hexadecimal CSS.
    /// </summary>
    [Required]
    [MaxLength(16)]
    public string AccentColor { get; set; } = "#2563eb";

    /// <summary>
    /// Color inicial del gradiente del sidebar administrativo.
    /// </summary>
    [Required]
    [MaxLength(16)]
    public string AdminSidebarStartColor { get; set; } = "#0f172a";

    /// <summary>
    /// Color final del gradiente del sidebar administrativo.
    /// </summary>
    [Required]
    [MaxLength(16)]
    public string AdminSidebarEndColor { get; set; } = "#1e293b";

    /// <summary>
    /// Inicial o glifo breve visible en los identificadores gráficos de la marca.
    /// </summary>
    [Required]
    [MaxLength(4)]
    public string LogoGlyph { get; set; } = "N";
}
