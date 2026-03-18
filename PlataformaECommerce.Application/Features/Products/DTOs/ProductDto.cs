using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Application.Features.Products.DTOs;

/// <summary>
/// Representa el objeto de transferencia de datos de un producto
/// dentro de la capa de aplicación.
/// </summary>
/// <remarks>
/// Este DTO se utiliza para transportar información de productos
/// desde la capa Application hacia capas superiores como:
/// - Web API,
/// - interfaces administrativas,
/// - catálogo público,
/// - consultas internas,
/// - respuestas de casos de uso.
///
/// Su propósito es desacoplar la representación expuesta del producto
/// respecto de la entidad de dominio <c>Producto</c>, evitando filtrar
/// directamente detalles internos del modelo.
///
/// Este DTO está diseñado para soportar tanto productos físicos
/// como productos digitales dentro de una sola representación
/// de lectura general.
/// </remarks>
public sealed class ProductDto
{
    /// <summary>
    /// Identificador único del producto.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Nombre comercial del producto.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Descripción general del producto.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// SKU del producto.
    /// </summary>
    public string Sku { get; init; } = string.Empty;

    /// <summary>
    /// Precio unitario del producto.
    /// </summary>
    public decimal Price { get; init; }

    /// <summary>
    /// Código de moneda asociado al precio del producto.
    /// </summary>
    public string Currency { get; init; } = string.Empty;

    /// <summary>
    /// Stock disponible del producto.
    /// </summary>
    public int Stock { get; init; }

    /// <summary>
    /// Indica si el producto se encuentra activo para operación comercial.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Indica si el producto está marcado como destacado dentro del catálogo.
    /// </summary>
    public bool IsFeatured { get; init; }

    /// <summary>
    /// Identificador amigable para URL y navegación pública.
    /// </summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>
    /// URL o ruta de la imagen principal del producto.
    /// </summary>
    public string? MainImageUrl { get; init; }

    /// <summary>
    /// Tipo funcional del producto.
    /// </summary>
    public TipoProducto ProductType { get; init; }

    /// <summary>
    /// Fecha y hora UTC en que fue creado el producto.
    /// </summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>
    /// Fecha y hora UTC de la última actualización relevante del producto.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; init; }

    #region Propiedades específicas de producto físico

    /// <summary>
    /// Peso del producto en kilogramos cuando corresponde a un producto físico.
    /// </summary>
    public decimal? WeightKg { get; init; }

    /// <summary>
    /// Alto del producto en centímetros cuando corresponde a un producto físico.
    /// </summary>
    public decimal? HeightCm { get; init; }

    /// <summary>
    /// Ancho del producto en centímetros cuando corresponde a un producto físico.
    /// </summary>
    public decimal? WidthCm { get; init; }

    /// <summary>
    /// Largo del producto en centímetros cuando corresponde a un producto físico.
    /// </summary>
    public decimal? LengthCm { get; init; }

    /// <summary>
    /// Indica si el producto físico requiere envío.
    /// </summary>
    public bool? RequiresShipping { get; init; }

    #endregion

    #region Propiedades específicas de producto digital

    /// <summary>
    /// Formato principal del archivo cuando corresponde a un producto digital.
    /// </summary>
    public string? FileFormat { get; init; }

    /// <summary>
    /// Tamaño del archivo en megabytes cuando corresponde a un producto digital.
    /// </summary>
    public decimal? FileSizeMb { get; init; }

    /// <summary>
    /// Indica si el producto digital requiere licencia o activación adicional.
    /// </summary>
    public bool? RequiresLicense { get; init; }

    #endregion

    #region Propiedades calculadas de apoyo

    /// <summary>
    /// Indica si el producto tiene stock disponible.
    /// </summary>
    public bool HasStock => Stock > 0;

    /// <summary>
    /// Indica si el producto está disponible comercialmente.
    /// </summary>
    public bool IsAvailable => IsActive && HasStock;

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del DTO de producto.
    /// </summary>
    /// <returns>Cadena representativa del producto.</returns>
    public override string ToString()
    {
        return $"ProductDto | Id: {Id} | Name: {Name} | Sku: {Sku} | Price: {Currency} {Price:N2} | Stock: {Stock} | Active: {IsActive} | Type: {ProductType}";
    }

    #endregion
}