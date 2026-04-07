namespace PlataformaECommerce.Infrastructure.Persistence.Entities;

/// <summary>
/// Representa el tenant comercial y operativo de la plataforma SaaS.
/// </summary>
public sealed class TenantEntity
{
    /// <summary>
    /// Identificador técnico único del tenant.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Nombre visible del tenant.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Indica si el tenant se encuentra habilitado para operar.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Nombre comercial del storefront del tenant.
    /// </summary>
    public string StorefrontName { get; set; } = string.Empty;

    /// <summary>
    /// Nombre visible del backoffice del tenant.
    /// </summary>
    public string BackofficeName { get; set; } = string.Empty;

    /// <summary>
    /// Mensaje comercial breve visible en la tienda del tenant.
    /// </summary>
    public string StorefrontTagline { get; set; } = string.Empty;

    /// <summary>
    /// Razón social o nombre legal del tenant.
    /// </summary>
    public string LegalCompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Correo principal de soporte del tenant.
    /// </summary>
    public string SupportEmail { get; set; } = string.Empty;

    /// <summary>
    /// Teléfono principal de soporte del tenant.
    /// </summary>
    public string SupportPhone { get; set; } = string.Empty;

    /// <summary>
    /// Horario operativo de soporte del tenant.
    /// </summary>
    public string SupportHours { get; set; } = string.Empty;

    /// <summary>
    /// Compromiso SLA base informado para el tenant.
    /// </summary>
    public string SupportSla { get; set; } = string.Empty;

    /// <summary>
    /// Color primario efectivo del tenant.
    /// </summary>
    public string PrimaryColor { get; set; } = string.Empty;

    /// <summary>
    /// Color de acento efectivo del tenant.
    /// </summary>
    public string AccentColor { get; set; } = string.Empty;

    /// <summary>
    /// Color inicial del sidebar administrativo.
    /// </summary>
    public string AdminSidebarStartColor { get; set; } = string.Empty;

    /// <summary>
    /// Color final del sidebar administrativo.
    /// </summary>
    public string AdminSidebarEndColor { get; set; } = string.Empty;

    /// <summary>
    /// Glifo breve utilizado para branding del tenant.
    /// </summary>
    public string LogoGlyph { get; set; } = string.Empty;

    /// <summary>
    /// Moneda operativa base del tenant.
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Hostnames asociados al tenant.
    /// </summary>
    public ICollection<TenantHostnameEntity> Hostnames { get; set; } = new List<TenantHostnameEntity>();

    /// <summary>
    /// Features habilitados específicamente para el tenant fuera del plan base.
    /// </summary>
    public ICollection<TenantFeatureAssignmentEntity> FeatureAssignments { get; set; } = new List<TenantFeatureAssignmentEntity>();

    /// <summary>
    /// Suscripción efectiva actual del tenant.
    /// </summary>
    public TenantSubscriptionEntity? Subscription { get; set; }

    /// <summary>
    /// Definición operativa de aprovisionamiento inicial del tenant.
    /// </summary>
    public TenantProvisioningEntity? Provisioning { get; set; }
}
