using PlataformaECommerce.Application.Common.SaaS;

namespace PlataformaECommerce.Application.Interfaces.Services.Common;

/// <summary>
/// Define el contrato de lectura del catálogo SaaS configurado para tenants, planes, features y suscripciones.
/// </summary>
public interface ITenantCatalogService
{
    /// <summary>
    /// Obtiene la definición efectiva del tenant activo para la solicitud actual.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    Task<TenantDefinition> GetCurrentTenantAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la colección completa de tenants configurados en la instancia.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    Task<IReadOnlyCollection<TenantDefinition>> GetConfiguredTenantsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el modo actual de aislamiento de datos declarado por la plataforma.
    /// </summary>
    string DataIsolationMode { get; }
}
