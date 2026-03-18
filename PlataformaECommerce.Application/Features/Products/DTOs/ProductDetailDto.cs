using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Application.Features.Products.DTOs;

/// <summary>
/// Representa la vista detallada de un producto dentro de la capa de aplicación.
/// </summary>
/// <remarks>
/// Este DTO es utilizado para transportar información completa de un producto
/// hacia capas superiores del sistema como:
/// 
/// - API REST
/// - Interfaces administrativas
/// - Catálogo público
/// - Sistemas de recomendación
/// 
/// A diferencia de <see cref="ProductDto"/>, esta clase contiene información
/// ampliada necesaria para representar la ficha completa del producto.
///
/// Este objeto de transferencia evita exponer directamente la entidad
/// de dominio <c>Producto</c>, manteniendo el desacoplamiento entre
/// la capa Application y el modelo interno del dominio.
///
/// Permite representar tanto productos físicos como digitales dentro
/// de una misma estructura de datos.
/// </remarks>
public sealed class ProductDetailDto
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
    /// Descripción completa del producto.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// SKU del producto.
    /// </summary>
    public string Sku { get; init; } = string.Empty;

    /// <summary>
    /// Identificador amigable para URL.
    /// </summary>
    public string Slug { get; init; } = string.Empty;

    #endregion

    #region Información comercial

    /// <summary>
    /// Precio unitario del producto.
    /// </summary>
    public decimal Price { get; init; }

    /// <summary>
    /// Código de moneda del precio del producto.
    /// </summary>
    public string Currency { get; init; } = string.Empty;

    /// <summary>
    /// Cantidad de unidades disponibles en inventario.
    /// </summary>
    public int Stock { get; init; }

    /// <summary>
    /// Indica si el producto se encuentra activo dentro del catálogo.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Indica si el producto se encuentra marcado como destacado.
    /// </summary>
    public bool IsFeatured { get; init; }

    #endregion

    #region Clasificación

    /// <summary>
    /// Tipo funcional del producto.
    /// </summary>
    public TipoProducto ProductType { get; init; }

    /// <summary>
    /// Identificador de la categoría principal del producto.
    /// </summary>
    public Guid? CategoryId { get; init; }

    /// <summary>
    /// Nombre de la categoría del producto.
    /// </summary>
    public string? CategoryName { get; init; }

    #endregion

    #region Información multimedia

    /// <summary>
    /// URL de la imagen principal del producto.
    /// </summary>
    public string? MainImageUrl { get; init; }

    /// <summary>
    /// Colección de imágenes asociadas al producto.
    /// </summary>
    public IReadOnlyCollection<string> ImageGallery { get; init; } = Array.Empty<string>();

    #endregion

    #region Propiedades específicas de productos físicos

    /// <summary>
    /// Peso del producto en kilogramos.
    /// </summary>
    public decimal? WeightKg { get; init; }

    /// <summary>
    /// Alto del producto en centímetros.
    /// </summary>
    public decimal? HeightCm { get; init; }

    /// <summary>
    /// Ancho del producto en centímetros.
    /// </summary>
    public decimal? WidthCm { get; init; }

    /// <summary>
    /// Largo del producto en centímetros.
    /// </summary>
    public decimal? LengthCm { get; init; }

    /// <summary>
    /// Indica si el producto requiere envío físico.
    /// </summary>
    public bool? RequiresShipping { get; init; }

    #endregion

    #region Propiedades específicas de productos digitales

    /// <summary>
    /// Formato del archivo digital asociado al producto.
    /// </summary>
    public string? FileFormat { get; init; }

    /// <summary>
    /// Tamaño del archivo digital en megabytes.
    /// </summary>
    public decimal? FileSizeMb { get; init; }

    /// <summary>
    /// Indica si el producto requiere activación mediante licencia.
    /// </summary>
    public bool? RequiresLicense { get; init; }

    #endregion

    #region Metadatos del sistema

    /// <summary>
    /// Fecha y hora UTC de creación del producto.
    /// </summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>
    /// Fecha y hora UTC de la última actualización del producto.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; init; }

    /// <summary>
    /// Identificador del usuario que creó el producto.
    /// </summary>
    public Guid? CreatedByUserId { get; init; }

    /// <summary>
    /// Identificador del usuario que realizó la última actualización.
    /// </summary>
    public Guid? UpdatedByUserId { get; init; }

    #endregion

    #region Propiedades calculadas

    /// <summary>
    /// Indica si el producto tiene inventario disponible.
    /// </summary>
    public bool HasStock => Stock > 0;

    /// <summary>
    /// Indica si el producto está disponible para compra.
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
    /// Devuelve una representación resumida del producto.
    /// </summary>
    /// <returns>Cadena representativa del producto.</returns>
    public override string ToString()
    {
        return $"ProductDetailDto | Id: {Id} | Name: {Name} | SKU: {Sku} | Price: {Currency} {Price:N2} | Stock: {Stock} | Active: {IsActive}";
    }

    #endregion
}