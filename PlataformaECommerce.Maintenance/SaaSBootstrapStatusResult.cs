namespace PlataformaECommerce.Maintenance;

internal sealed record SaaSBootstrapStatusResult(
    string TenantId,
    string DisplayName,
    bool PersistedTenantExists,
    bool ProvisioningStateExists,
    DateTime? CatalogSynchronizedAtUtc,
    bool BaseCategoriesRequired,
    bool BaseCategoriesProvisioned,
    DateTime? BaseCategoriesProvisionedAtUtc,
    bool DemoCatalogRequired,
    bool DemoCatalogProvisioned,
    DateTime? DemoCatalogProvisionedAtUtc,
    string ExpectedSuperUserEmail,
    bool SuperUserProvisioned,
    DateTime? SuperUserProvisionedAtUtc)
{
    public bool BootstrapReady => PersistedTenantExists
        && CatalogSynchronizedAtUtc.HasValue
        && (!BaseCategoriesRequired || BaseCategoriesProvisioned)
        && (!DemoCatalogRequired || DemoCatalogProvisioned)
        && SuperUserProvisioned;
}
