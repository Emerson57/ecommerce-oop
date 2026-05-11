namespace PlataformaECommerce.Infrastructure.Configurations;

/// <summary>
/// Define la configuración SaaS de tenants, catálogo comercial y resolución del tenant activo.
/// </summary>
public sealed class SaaSPlatformOptions
{
    /// <summary>
    /// Nombre de la sección de configuración.
    /// </summary>
    public const string SectionName = "SaaS";

    /// <summary>
    /// Identificador del tenant activo por defecto cuando no se resuelve otro contexto.
    /// </summary>
    public string ActiveTenantId { get; set; } = string.Empty;

    /// <summary>
    /// Header opcional utilizado para resolver el tenant activo por solicitud.
    /// </summary>
    public string ResolutionHeaderName { get; set; } = "X-Tenant-ID";

    /// <summary>
    /// Indica si la resolución del tenant puede usar el host de la solicitud.
    /// </summary>
    public bool ResolveTenantFromHost { get; set; } = true;

    /// <summary>
    /// Describe el modo de aislamiento actualmente implementado por la plataforma.
    /// </summary>
    public string DataIsolationMode { get; set; } = "SharedDatabaseSharedSchema";

    /// <summary>
    /// Catálogo global de features comercializables.
    /// </summary>
    public IList<FeatureOptions> Features { get; set; } = [];

    /// <summary>
    /// Catálogo global de planes comercializables.
    /// </summary>
    public IList<PlanOptions> Plans { get; set; } = [];

    /// <summary>
    /// Tenants configurados en la instancia actual.
    /// </summary>
    public IList<TenantOptions> Tenants { get; set; } = [];

    /// <summary>
    /// Representa un feature comercializable de la plataforma.
    /// </summary>
    public sealed class FeatureOptions
    {
        public string FeatureId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    /// <summary>
    /// Representa un plan comercial de la plataforma.
    /// </summary>
    public sealed class PlanOptions
    {
        public string PlanId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal MonthlyPrice { get; set; }
        public string Currency { get; set; } = "USD";
        public int IncludedAdministrators { get; set; }
        public int IncludedProducts { get; set; }
        public IList<string> IncludedFeatureIds { get; set; } = [];
    }

    /// <summary>
    /// Representa un tenant configurado dentro de la plataforma.
    /// </summary>
    public sealed class TenantOptions
    {
        public string TenantId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;
        public string StorefrontName { get; set; } = string.Empty;
        public string BackofficeName { get; set; } = string.Empty;
        public string StorefrontTagline { get; set; } = string.Empty;
        public string LegalCompanyName { get; set; } = string.Empty;
        public string SupportEmail { get; set; } = string.Empty;
        public string SupportPhone { get; set; } = string.Empty;
        public string SupportHours { get; set; } = string.Empty;
        public string SupportSla { get; set; } = string.Empty;
        public string PrimaryColor { get; set; } = string.Empty;
        public string AccentColor { get; set; } = string.Empty;
        public string AdminSidebarStartColor { get; set; } = string.Empty;
        public string AdminSidebarEndColor { get; set; } = string.Empty;
        public string LogoGlyph { get; set; } = string.Empty;
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// Código de país ISO 3166-1 alpha-2 (p. ej. CO) para operación y cumplimiento comercial.
        /// </summary>
        public string Country { get; set; } = string.Empty;

        public string? PlanId { get; set; }
        public IList<string> EnabledFeatureIds { get; set; } = [];
        public IList<string> Hostnames { get; set; } = [];
        public SubscriptionOptions Subscription { get; set; } = new();
        public ProvisioningOptions Provisioning { get; set; } = new();
    }

    /// <summary>
    /// Representa la configuración contractual de suscripción del tenant.
    /// </summary>
    public sealed class SubscriptionOptions
    {
        public string Status { get; set; } = string.Empty;
        public DateTime? StartedAtUtc { get; set; }
        public DateTime? TrialEndsAtUtc { get; set; }
        public DateTime? RenewalAtUtc { get; set; }
        public bool AutoRenew { get; set; }
        public int SeatsPurchased { get; set; }
    }

    /// <summary>
    /// Representa la configuración de aprovisionamiento inicial sugerida para el tenant.
    /// </summary>
    public sealed class ProvisioningOptions
    {
        public string BootstrapSuperUserEmail { get; set; } = string.Empty;
        public bool SeedBaseCategories { get; set; }
        public bool SeedDemoCatalog { get; set; }
        public bool EnablePublicStorefront { get; set; } = true;
        public string? Notes { get; set; }
    }
}
