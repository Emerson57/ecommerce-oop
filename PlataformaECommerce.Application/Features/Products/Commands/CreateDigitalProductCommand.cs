using PlataformaECommerce.Application.Common.Results;

namespace PlataformaECommerce.Application.Features.Products.Commands;

/// <summary>
/// Representa el comando de aplicación para crear un producto digital dentro del sistema.
/// </summary>
/// <remarks>
/// Este comando modela una intención explícita de escritura sobre el sistema,
/// correspondiente al caso de uso de registro de un nuevo producto digital.
///
/// Su responsabilidad es transportar los datos necesarios desde la capa superior
/// hacia el caso de uso correspondiente, sin contener lógica de negocio ni reglas
/// de validación complejas, las cuales deben resolverse en:
/// - validadores de Application,
/// - servicios de aplicación,
/// - y entidades del dominio.
///
/// El resultado esperado de la operación es un <see cref="Result{TValue}"/>
/// que contiene el identificador del producto creado cuando la ejecución es exitosa.
/// </remarks>
public sealed class CreateDigitalProductCommand
{
    #region Información comercial base

    /// <summary>
    /// Nombre comercial del producto digital.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Descripción funcional o comercial del producto digital.
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
    /// <remarks>
    /// Aunque el producto sea digital, el dominio actual conserva control de stock,
    /// lo cual puede representar licencias, cupos, activaciones o unidades vendibles.
    /// </remarks>
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
    /// Identificador de la categoría a la que pertenecerá el producto.
    /// </summary>
    public Guid? CategoryId { get; init; }

    /// <summary>
    /// Colección de etiquetas asociadas al producto.
    /// </summary>
    public IReadOnlyCollection<string> Tags { get; init; } = Array.Empty<string>();

    #endregion

    #region Información técnica del producto digital

    /// <summary>
    /// Formato principal del archivo digital.
    /// </summary>
    /// <remarks>
    /// Ejemplos comunes:
    /// - PDF
    /// - MP4
    /// - ZIP
    /// - EPUB
    /// - MP3
    /// </remarks>
    public string FileFormat { get; init; } = string.Empty;

    /// <summary>
    /// Tamaño estimado del archivo digital en megabytes.
    /// </summary>
    /// <remarks>
    /// Puede ser nulo en escenarios donde aún no se dispone del tamaño final
    /// o cuando no aplica directamente al modelo comercial.
    /// </remarks>
    public decimal? FileSizeMb { get; init; }

    /// <summary>
    /// Indica si el producto digital requiere licencia, activación
    /// o habilitación adicional posterior a la compra.
    /// </summary>
    public bool RequiresLicense { get; init; }

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del comando.
    /// </summary>
    /// <returns>Cadena representativa del comando de creación.</returns>
    public override string ToString()
    {
        return $"CreateDigitalProductCommand | Name: {Name} | Sku: {Sku} | Price: {Currency} {Price:N2} | Stock: {Stock} | FileFormat: {FileFormat}";
    }

    #endregion
}