namespace PlataformaECommerce.Application.Common.SaaS;

/// <summary>
/// Representa la definición efectiva de un tenant configurado en la plataforma SaaS.
/// </summary>
public sealed record TenantDefinition
{
    /// <summary>
    /// Identificador técnico único del tenant.
    /// </summary>
    public string TenantId { get; init; } = string.Empty;

    /// <summary>
    /// Nombre visible del tenant.
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Nombre comercial visible del storefront del tenant.
    /// </summary>
    public string StorefrontName { get; init; } = string.Empty;

    /// <summary>
    /// Nombre visible del backoffice del tenant.
    /// </summary>
    public string BackofficeName { get; init; } = string.Empty;

    /// <summary>
    /// Mensaje comercial breve del tenant.
    /// </summary>
    public string StorefrontTagline { get; init; } = string.Empty;

    /// <summary>
    /// Nombre legal o razón comercial visible del tenant.
    /// </summary>
    public string LegalCompanyName { get; init; } = string.Empty;

    /// <summary>
    /// Correo principal de soporte del tenant.
    /// </summary>
    public string SupportEmail { get; init; } = string.Empty;

    /// <summary>
    /// Teléfono principal de soporte del tenant.
    /// </summary>
    public string SupportPhone { get; init; } = string.Empty;

    /// <summary>
    /// Horario operativo de soporte del tenant.
    /// </summary>
    public string SupportHours { get; init; } = string.Empty;

    /// <summary>
    /// SLA base de soporte comprometido para el tenant.
    /// </summary>
    public string SupportSla { get; init; } = string.Empty;

    /// <summary>
    /// Color primario efectivo del tenant.
    /// </summary>
    public string PrimaryColor { get; init; } = string.Empty;

    /// <summary>
    /// Color de acento efectivo del tenant.
    /// </summary>
    public string AccentColor { get; init; } = string.Empty;

    /// <summary>
    /// Color inicial del sidebar administrativo del tenant.
    /// </summary>
    public string AdminSidebarStartColor { get; init; } = string.Empty;

    /// <summary>
    /// Color final del sidebar administrativo del tenant.
    /// </summary>
    public string AdminSidebarEndColor { get; init; } = string.Empty;

    /// <summary>
    /// Glifo visual breve del tenant.
    /// </summary>
    public string LogoGlyph { get; init; } = string.Empty;

    /// <summary>
    /// Moneda base operativa del tenant.
    /// </summary>
    public string Currency { get; init; } = string.Empty;

    /// <summary>
    /// Hostnames asociados al tenant cuando existen.
    /// </summary>
    public IReadOnlyCollection<string> Hostnames { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Plan efectivo actualmente asociado al tenant.
    /// </summary>
    public TenantPlanDefinition? CurrentPlan { get; init; }

    /// <summary>
    /// Estado de suscripción actual del tenant.
    /// </summary>
    public TenantSubscriptionDefinition Subscription { get; init; } = new();

    /// <summary>
    /// Features habilitados efectivamente para el tenant.
    /// </summary>
    public IReadOnlyCollection<TenantFeatureDefinition> Features { get; init; } = Array.Empty<TenantFeatureDefinition>();

    /// <summary>
    /// Definición de aprovisionamiento inicial sugerida para el tenant.
    /// </summary>
    public TenantProvisioningDefinition Provisioning { get; init; } = new();
}
