namespace PlataformaECommerce.Infrastructure.Persistence.Entities;

/// <summary>
/// Representa un hostname asociado a un tenant SaaS.
/// </summary>
public sealed class TenantHostnameEntity
{
    /// <summary>
    /// Identificador técnico del tenant propietario del hostname.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Hostname asociado al tenant.
    /// </summary>
    public string Hostname { get; set; } = string.Empty;

    /// <summary>
    /// Navegación hacia el tenant propietario del hostname.
    /// </summary>
    public TenantEntity Tenant { get; set; } = null!;
}
