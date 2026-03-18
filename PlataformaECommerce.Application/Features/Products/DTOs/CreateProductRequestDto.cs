using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Application.Features.Products.DTOs;

/// <summary>
/// Representa la solicitud de creación de un producto dentro de la capa de aplicación.
/// </summary>
/// <remarks>
/// Este DTO se utiliza para transportar la información necesaria para registrar
/// un nuevo producto dentro del sistema, desacoplando la entrada externa
/// respecto de las entidades del dominio.
///
/// Su propósito es servir como contrato de entrada para:
/// - endpoints HTTP,
/// - handlers de comandos,
/// - servicios de aplicación,
/// - flujos administrativos de alta de productos.
///
/// La estructura admite tanto productos físicos como digitales,
/// por lo que algunas propiedades solo aplican según el valor de
/// <see cref="ProductType"/>.
/// 
/// Las reglas de obligatoriedad, consistencia y validación detallada
/// deben aplicarse en la capa Application mediante validadores especializados.
/// </remarks>
public sealed class CreateProductRequestDto
{
    #region Información base

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
    /// Precio unitario del producto.
    /// </summary>
    public decimal Price { get; init; }

    /// <summary>
    /// Código de moneda asociado al precio del producto.
    /// </summary>
    /// <remarks>
    /// Ejemplos comunes:
    /// - COP
    /// - USD
    /// - EUR
    /// </remarks>
    public string Currency { get; init; } = "COP";

    /// <summary>
    /// Stock inicial del producto.
    /// </summary>
    public int Stock { get; init; }

    /// <summary>
    /// Identificador amigable para URL y navegación pública.
    /// </summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>
    /// URL o ruta de la imagen principal del producto.
    /// </summary>
    public string? MainImageUrl { get; init; }

    /// <summary>
    /// Tipo funcional del producto a crear.
    /// </summary>
    public TipoProducto ProductType { get; init; }

    #endregion

    #region Estado inicial

    /// <summary>
    /// Indica si el producto debe crearse inicialmente como activo.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Indica si el producto debe crearse inicialmente como destacado.
    /// </summary>
    public bool IsFeatured { get; init; }

    #endregion

    #region Clasificación

    /// <summary>
    /// Identificador de la categoría a la que pertenece el producto.
    /// </summary>
    public Guid? CategoryId { get; init; }

    /// <summary>
    /// Colección de etiquetas asociadas al producto.
    /// </summary>
    public IReadOnlyCollection<string> Tags { get; init; } = Array.Empty<string>();

    #endregion

    #region Propiedades específicas de productos físicos

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

    #region Propiedades específicas de productos digitales

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

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida de la solicitud de creación de producto.
    /// </summary>
    /// <returns>Cadena representativa de la solicitud.</returns>
    public override string ToString()
    {
        return $"CreateProductRequestDto | Name: {Name} | Sku: {Sku} | Price: {Currency} {Price:N2} | Stock: {Stock} | Type: {ProductType}";
    }

    #endregion
}