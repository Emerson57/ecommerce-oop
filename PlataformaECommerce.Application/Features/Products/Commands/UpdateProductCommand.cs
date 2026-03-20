using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Products.DTOs;
using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Application.Features.Products.Commands;

/// <summary>
/// Representa el comando de aplicación para actualizar un producto existente dentro del sistema.
/// </summary>
/// <remarks>
/// Este comando modela una intención explícita de escritura sobre el sistema,
/// correspondiente al caso de uso de modificación de un producto existente.
///
/// Su responsabilidad es transportar los datos necesarios desde la capa superior
/// hacia el caso de uso correspondiente, sin contener lógica de negocio ni reglas
/// de validación complejas, las cuales deben resolverse en:
/// - validadores de Application,
/// - servicios de aplicación,
/// - y entidades del dominio.
///
/// El resultado esperado de la operación es un <see cref="Result{TValue}"/>
/// que contiene la representación actualizada del producto cuando la ejecución es exitosa.
/// </remarks>
public sealed class UpdateProductCommand
{
    #region Identificación

    /// <summary>
    /// Identificador único del producto que será actualizado.
    /// </summary>
    public Guid Id { get; init; }

    #endregion

    #region Información comercial base

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
    /// Stock actual o nuevo stock del producto.
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

    #endregion

    #region Estado del producto

    /// <summary>
    /// Indica si el producto debe permanecer activo después de la actualización.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Indica si el producto debe permanecer destacado después de la actualización.
    /// </summary>
    public bool IsFeatured { get; init; }

    #endregion

    #region Clasificación

    /// <summary>
    /// Tipo funcional del producto.
    /// </summary>
    public TipoProducto ProductType { get; init; }

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
    /// Devuelve una representación resumida del comando.
    /// </summary>
    /// <returns>Cadena representativa del comando de actualización.</returns>
    public override string ToString()
    {
        return $"UpdateProductCommand | Id: {Id} | Name: {Name} | Sku: {Sku} | Price: {Currency} {Price:N2} | Stock: {Stock} | Type: {ProductType} | Active: {IsActive}";
    }

    #endregion
}