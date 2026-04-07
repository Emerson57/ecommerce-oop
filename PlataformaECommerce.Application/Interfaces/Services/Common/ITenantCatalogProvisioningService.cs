namespace PlataformaECommerce.Application.Interfaces.Services.Common;

/// <summary>
/// Define las operaciones de provisión controlada del catálogo SaaS persistente.
/// </summary>
public interface ITenantCatalogProvisioningService
{
    /// <summary>
    /// Sincroniza hacia persistencia el catálogo SaaS declarado por configuración para habilitar tenants, planes, features y metadatos iniciales.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    Task SynchronizeConfiguredCatalogAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca el bootstrap del super usuario inicial como provisionado para un tenant específico.
    /// </summary>
    /// <param name="tenantId">Identificador lógico del tenant provisionado.</param>
    /// <param name="email">Correo electrónico del super usuario bootstrappeado.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    Task MarkSuperUserProvisionedAsync(string tenantId, string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca la provisión de categorías base como completada para un tenant específico.
    /// </summary>
    /// <param name="tenantId">Identificador lógico del tenant provisionado.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    Task MarkBaseCategoriesProvisionedAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca la provisión del catálogo demo como completada para un tenant específico.
    /// </summary>
    /// <param name="tenantId">Identificador lógico del tenant provisionado.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    Task MarkDemoCatalogProvisionedAsync(string tenantId, CancellationToken cancellationToken = default);
}
