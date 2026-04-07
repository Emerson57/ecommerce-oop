namespace PlataformaECommerce.Infrastructure.Persistence.Entities;

/// <summary>
/// Define el contrato mínimo de una entidad persistente aislada por tenant.
/// </summary>
public interface ITenantOwnedEntity
{
    /// <summary>
    /// Obtiene o establece el identificador lógico del tenant propietario del registro.
    /// </summary>
    string TenantId { get; set; }
}
