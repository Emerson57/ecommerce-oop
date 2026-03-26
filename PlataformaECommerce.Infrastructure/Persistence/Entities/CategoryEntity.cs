namespace PlataformaECommerce.Infrastructure.Persistence.Entities;

/// <summary>
/// Representa la proyección persistente de una categoría de producto.
/// </summary>
public sealed class CategoryEntity
{
    /// <summary>
    /// Identificador único de la categoría.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre visible de la categoría.
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Slug normalizado de la categoría.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Descripción opcional de la categoría.
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Indica si la categoría está activa.
    /// </summary>
    public bool Activa { get; set; }

    /// <summary>
    /// Identificador de la categoría padre cuando corresponde a una subcategoría.
    /// </summary>
    public Guid? ParentCategoryId { get; set; }

    /// <summary>
    /// Fecha de creación en UTC.
    /// </summary>
    public DateTime FechaCreacionUtc { get; set; }

    /// <summary>
    /// Fecha de última actualización en UTC.
    /// </summary>
    public DateTime? FechaActualizacionUtc { get; set; }
}
