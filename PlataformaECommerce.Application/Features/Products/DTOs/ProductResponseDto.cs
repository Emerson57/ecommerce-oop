using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Application.Features.Products.DTOs;

/// <summary>
/// Representa la respuesta estándar de un producto dentro de la capa de aplicación.
/// </summary>
/// <remarks>
/// Este DTO se utiliza para retornar información de producto desde casos de uso,
/// servicios de aplicación o endpoints HTTP, sin exponer directamente
/// la entidad de dominio.
///
/// Su propósito es ofrecer una representación de salida consistente,
/// reutilizable y desacoplada del modelo interno del dominio.
///
/// Esta clase puede emplearse en escenarios como:
/// - creación de productos,
/// - actualización de productos,
/// - consulta por identificador,
/// - respuestas administrativas,
/// - respuestas de catálogo interno.
///
/// El DTO admite tanto productos físicos como digitales dentro de una sola
/// estructura de respuesta.
/// </remarks>
public sealed class ProductResponseDto
{
    #region Identificación básica

    /// <summary>
    /// Identificador único del producto.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Nombre comercial del producto.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Descripción funcional o comercial del producto.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// SKU del producto.
    /// </summary>
    public string Sku { get; init; } = string.Empty;

    /// <summary>
    /// Identificador amigable para URL y navegación pública.
    /// </summary>
    public string Slug { get; init; } = string.Empty;

    #endregion

    #region Información comercial

    /// <summary>
    /// Precio unitario del producto.
    /// </summary>
    public decimal Price { get; init; }

    /// <summary>
    /// Precio base del producto antes de promociones.
    /// </summary>
    public decimal BasePrice { get; init; }

    /// <summary>
    /// Precio promocional vigente del producto cuando existe una promoción activa.
    /// </summary>
    public decimal? PromotionalPrice { get; init; }

    /// <summary>
    /// Código de moneda asociado al precio del producto.
    /// </summary>
    public string Currency { get; init; } = string.Empty;

    /// <summary>
    /// Stock disponible del producto.
    /// </summary>
    public int Stock { get; init; }

    /// <summary>
    /// Indica si el producto se encuentra activo dentro del sistema.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Indica si el producto está marcado como destacado dentro del catálogo.
    /// </summary>
    public bool IsFeatured { get; init; }

    /// <summary>
    /// Indica si el producto tiene una promoción activa.
    /// </summary>
    public bool HasPromotion { get; init; }

    /// <summary>
    /// Porcentaje de descuento promocional actualmente aplicado.
    /// </summary>
    public decimal? CurrentDiscountPercentage { get; init; }

    #endregion

    #region Clasificación

    /// <summary>
    /// Tipo funcional del producto.
    /// </summary>
    public TipoProducto ProductType { get; init; }

    /// <summary>
    /// Identificador de la categoría asociada al producto.
    /// </summary>
    public Guid? CategoryId { get; init; }

    /// <summary>
    /// Identificador de la subcategoría asociada al producto.
    /// </summary>
    public Guid? SubcategoryId { get; init; }

    /// <summary>
    /// Colección de etiquetas asociadas al producto.
    /// </summary>
    public IReadOnlyCollection<string> Tags { get; init; } = Array.Empty<string>();

    #endregion

    #region Información multimedia

    /// <summary>
    /// URL o ruta de la imagen principal del producto.
    /// </summary>
    public string? MainImageUrl { get; init; }

    /// <summary>
    /// Colección de imágenes complementarias del producto.
    /// </summary>
    public IReadOnlyCollection<string> ImageGallery { get; init; } = Array.Empty<string>();

    #endregion

    #region Información específica de productos físicos

    /// <summary>
    /// Peso del producto en kilogramos cuando se trata de un producto físico.
    /// </summary>
    public decimal? WeightKg { get; init; }

    /// <summary>
    /// Alto del producto en centímetros cuando se trata de un producto físico.
    /// </summary>
    public decimal? HeightCm { get; init; }

    /// <summary>
    /// Ancho del producto en centímetros cuando se trata de un producto físico.
    /// </summary>
    public decimal? WidthCm { get; init; }

    /// <summary>
    /// Largo del producto en centímetros cuando se trata de un producto físico.
    /// </summary>
    public decimal? LengthCm { get; init; }

    /// <summary>
    /// Indica si el producto físico requiere envío.
    /// </summary>
    public bool? RequiresShipping { get; init; }

    #endregion

    #region Información específica de productos digitales

    /// <summary>
    /// Formato principal del archivo cuando se trata de un producto digital.
    /// </summary>
    public string? FileFormat { get; init; }

    /// <summary>
    /// Tamaño del archivo digital en megabytes cuando aplica.
    /// </summary>
    public decimal? FileSizeMb { get; init; }

    /// <summary>
    /// Indica si el producto digital requiere licencia o activación adicional.
    /// </summary>
    public bool? RequiresLicense { get; init; }

    #endregion

    #region Metadatos

    /// <summary>
    /// Fecha y hora UTC de creación del producto.
    /// </summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>
    /// Fecha y hora UTC de la última actualización del producto.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; init; }

    #endregion

    #region Propiedades calculadas

    /// <summary>
    /// Indica si el producto tiene inventario disponible.
    /// </summary>
    public bool HasStock => Stock > 0;

    /// <summary>
    /// Indica si el producto está disponible comercialmente.
    /// </summary>
    public bool IsAvailable => IsActive && HasStock;

    /// <summary>
    /// Indica si el producto corresponde a un producto físico.
    /// </summary>
    public bool IsPhysicalProduct => ProductType == TipoProducto.Fisico;

    /// <summary>
    /// Indica si el producto corresponde a un producto digital.
    /// </summary>
    public bool IsDigitalProduct => ProductType == TipoProducto.Digital;

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del DTO de respuesta de producto.
    /// </summary>
    /// <returns>Cadena representativa del producto.</returns>
    public override string ToString()
    {
        return $"ProductResponseDto | Id: {Id} | Name: {Name} | Sku: {Sku} | Price: {Currency} {Price:N2} | Stock: {Stock} | Active: {IsActive} | Type: {ProductType}";
    }

    #endregion
}