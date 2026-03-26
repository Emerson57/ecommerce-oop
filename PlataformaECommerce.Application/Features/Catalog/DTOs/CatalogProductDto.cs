using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Application.Features.Catalog.DTOs;

/// <summary>
/// Representa un producto del catálogo expuesto por la capa de aplicación
/// para escenarios de consulta, navegación y visualización comercial.
/// </summary>
/// <remarks>
/// Este DTO se utiliza para proyectar la información pública o semipública
/// de un producto dentro de listados de catálogo, búsquedas, vitrinas,
/// resultados filtrados y páginas de exploración del e-Commerce.
///
/// Su propósito es desacoplar la representación de lectura utilizada por
/// la capa superior respecto de la entidad de dominio <c>Producto</c>,
/// permitiendo exponer únicamente la información relevante para consumo
/// funcional, comercial y de experiencia de usuario.
///
/// Esta clase no debe contener lógica de negocio, reglas de validación
/// complejas ni comportamiento del dominio. Dichas responsabilidades deben
/// permanecer en la capa Domain y, cuando aplique, en validadores o servicios
/// especializados de Application.
/// </remarks>
public sealed class CatalogProductDto
{
    #region Identificación principal

    /// <summary>
    /// Identificador único del producto dentro del sistema.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Código SKU del producto.
    /// </summary>
    /// <remarks>
    /// Este valor representa el identificador comercial o logístico
    /// utilizado para distinguir el producto dentro del catálogo.
    /// </remarks>
    public string Sku { get; init; } = string.Empty;

    /// <summary>
    /// Nombre comercial del producto.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Nombre corto del producto, cuando se requiera una versión resumida
    /// para tarjetas, listados compactos o interfaces móviles.
    /// </summary>
    public string? ShortName { get; init; }

    /// <summary>
    /// Slug público o identificador amigable para URL del producto.
    /// </summary>
    public string? Slug { get; init; }

    #endregion

    #region Información descriptiva

    /// <summary>
    /// Descripción comercial principal del producto.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Descripción resumida del producto para escenarios de listado,
    /// búsqueda o presentación compacta.
    /// </summary>
    public string? ShortDescription { get; init; }

    /// <summary>
    /// Marca comercial asociada al producto, cuando aplique.
    /// </summary>
    public string? Brand { get; init; }

    /// <summary>
    /// Categoría principal del producto.
    /// </summary>
    public string? CategoryName { get; init; }

    /// <summary>
    /// Subcategoría o clasificación secundaria del producto, cuando aplique.
    /// </summary>
    public string? SubcategoryName { get; init; }

    /// <summary>
    /// Etiquetas o palabras clave asociadas al producto para búsqueda,
    /// segmentación o analítica.
    /// </summary>
    public IReadOnlyCollection<string> Tags { get; init; } = Array.Empty<string>();

    #endregion

    #region Tipología y disponibilidad

    /// <summary>
    /// Tipo funcional del producto dentro del dominio.
    /// </summary>
    public TipoProducto ProductType { get; init; }

    /// <summary>
    /// Indica si el producto se encuentra disponible para venta.
    /// </summary>
    public bool IsAvailable { get; init; }

    /// <summary>
    /// Indica si el producto se encuentra activo dentro del catálogo.
    /// </summary>
    /// <remarks>
    /// Un producto activo puede ser visible o consumible según las reglas
    /// propias del negocio y de la estrategia de publicación.
    /// </remarks>
    public bool IsActive { get; init; }

    /// <summary>
    /// Indica si el producto se encuentra destacado dentro del catálogo.
    /// </summary>
    public bool IsFeatured { get; init; }

    /// <summary>
    /// Indica si el producto es nuevo dentro del catálogo.
    /// </summary>
    public bool IsNew { get; init; }

    /// <summary>
    /// Indica si el producto está marcado como recomendado.
    /// </summary>
    public bool IsRecommended { get; init; }

    /// <summary>
    /// Indica si el producto posee disponibilidad inmediata de inventario.
    /// </summary>
    public bool HasStock { get; init; }

    /// <summary>
    /// Cantidad disponible en inventario, cuando aplique exponerla.
    /// </summary>
    /// <remarks>
    /// Este valor puede utilizarse para interfaces administrativas,
    /// motores de recomendación o reglas de visualización controlada.
    /// </remarks>
    public int? AvailableStock { get; init; }

    #endregion

    #region Información comercial y monetaria

    /// <summary>
    /// Precio base o precio actual de venta del producto.
    /// </summary>
    public decimal Price { get; init; }

    /// <summary>
    /// Código de moneda asociado al precio del producto.
    /// </summary>
    public string Currency { get; init; } = "COP";

    /// <summary>
    /// Precio anterior del producto, cuando se requiera mostrar
    /// una referencia de descuento o promoción.
    /// </summary>
    public decimal? PreviousPrice { get; init; }

    /// <summary>
    /// Valor del descuento aplicado, cuando exista.
    /// </summary>
    public decimal? DiscountAmount { get; init; }

    /// <summary>
    /// Porcentaje de descuento aplicado al producto, cuando exista.
    /// </summary>
    public decimal? DiscountPercentage { get; init; }

    /// <summary>
    /// Indica si el producto se encuentra actualmente en promoción.
    /// </summary>
    public bool IsOnSale { get; init; }

    #endregion

    #region Recursos visuales

    /// <summary>
    /// URL de la imagen principal del producto.
    /// </summary>
    public string? MainImageUrl { get; init; }

    /// <summary>
    /// URL de una imagen secundaria del producto, cuando esté disponible.
    /// </summary>
    public string? SecondaryImageUrl { get; init; }

    /// <summary>
    /// Conjunto de imágenes asociadas al producto.
    /// </summary>
    public IReadOnlyCollection<string> ImageUrls { get; init; } = Array.Empty<string>();

    #endregion

    #region Métricas y percepción comercial

    /// <summary>
    /// Calificación promedio del producto, cuando el sistema
    /// soporte reputación o valoraciones.
    /// </summary>
    public decimal? AverageRating { get; init; }

    /// <summary>
    /// Cantidad total de valoraciones recibidas por el producto.
    /// </summary>
    public int ReviewCount { get; init; }

    /// <summary>
    /// Cantidad total de ventas asociadas al producto, cuando se desee exponer
    /// una métrica comercial resumida.
    /// </summary>
    public int? SalesCount { get; init; }

    #endregion

    #region Fechas y trazabilidad

    /// <summary>
    /// Fecha y hora UTC de creación del producto.
    /// </summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>
    /// Fecha y hora UTC de la última actualización del producto.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; init; }

    /// <summary>
    /// Fecha y hora UTC desde la cual el producto está publicado,
    /// cuando la estrategia de publicación lo requiera.
    /// </summary>
    public DateTime? PublishedAtUtc { get; init; }

    #endregion

    #region Propiedades calculadas

    /// <summary>
    /// Indica si el producto posee descuento informado.
    /// </summary>
    public bool HasDiscount =>
        DiscountAmount.HasValue && DiscountAmount.Value > 0 ||
        DiscountPercentage.HasValue && DiscountPercentage.Value > 0;

    /// <summary>
    /// Indica si el producto contiene al menos una imagen asociada.
    /// </summary>
    public bool HasImages =>
        !string.IsNullOrWhiteSpace(MainImageUrl) ||
        !string.IsNullOrWhiteSpace(SecondaryImageUrl) ||
        ImageUrls.Count > 0;

    /// <summary>
    /// Indica si el producto tiene calificación disponible.
    /// </summary>
    public bool HasRating => AverageRating.HasValue;

    /// <summary>
    /// Obtiene el nombre visible más apropiado para presentar el producto.
    /// </summary>
    public string DisplayName =>
        !string.IsNullOrWhiteSpace(ShortName)
            ? ShortName
            : Name;

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del producto de catálogo.
    /// </summary>
    /// <returns>Cadena representativa del DTO.</returns>
    public override string ToString()
    {
        return $"CatalogProductDto | Id: {Id} | Sku: {Sku} | Name: {Name} | Price: {Currency} {Price:N2} | IsAvailable: {IsAvailable} | ProductType: {ProductType}";
    }

    #endregion
}