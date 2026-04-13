namespace PlataformaECommerce.Web.Initialization;

/// <summary>
/// Representa el resultado no destructivo de inspeccionar filas legacy sin tenant antes de una corrección explícita.
/// </summary>
public sealed record LegacyTenantNormalizationInspectionResult(
    string? TenantId,
    bool EnvironmentAllowsExecution,
    bool TenantResolved,
    int Products,
    int Categories,
    int Users,
    int Orders,
    int OrderItems,
    int Carts,
    int CartItems)
{
    /// <summary>
    /// Indica si existen filas legacy pendientes de corrección para el tenant inspeccionado.
    /// </summary>
    public bool HasLegacyRows => Products > 0
        || Categories > 0
        || Users > 0
        || Orders > 0
        || OrderItems > 0
        || Carts > 0
        || CartItems > 0;

    /// <summary>
    /// Obtiene el total agregado de filas legacy detectadas durante la inspección.
    /// </summary>
    public int TotalRows => Products + Categories + Users + Orders + OrderItems + Carts + CartItems;
}
