namespace PlataformaECommerce.Application.Common.SaaS;

/// <summary>
/// Representa un plan comercial disponible para tenants de la plataforma.
/// </summary>
public sealed record TenantPlanDefinition
{
    /// <summary>
    /// Identificador técnico único del plan.
    /// </summary>
    public string PlanId { get; init; } = string.Empty;

    /// <summary>
    /// Nombre visible del plan.
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Descripción comercial breve del plan.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Precio mensual de referencia del plan.
    /// </summary>
    public decimal MonthlyPrice { get; init; }

    /// <summary>
    /// Moneda del precio de referencia del plan.
    /// </summary>
    public string Currency { get; init; } = "USD";

    /// <summary>
    /// Cantidad máxima de administradores incluidos base en el plan.
    /// </summary>
    public int IncludedAdministrators { get; init; }

    /// <summary>
    /// Cantidad máxima de productos incluida base en el plan.
    /// </summary>
    public int IncludedProducts { get; init; }

    /// <summary>
    /// Identificadores de features incluidos por el plan.
    /// </summary>
    public IReadOnlyCollection<string> IncludedFeatureIds { get; init; } = Array.Empty<string>();
}
