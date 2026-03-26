using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Application.Features.Products.Commands;

/// <summary>
/// Representa el comando para importar productos desde una plantilla tabular validada.
/// </summary>
public sealed class ImportProductsCommand
{
    /// <summary>
    /// Filas de productos proyectadas desde la plantilla cargada por el backoffice.
    /// </summary>
    public IReadOnlyCollection<ImportProductRowCommand> Rows { get; init; } = Array.Empty<ImportProductRowCommand>();

    /// <summary>
    /// Identificador del usuario administrativo que solicita la importación cuando está disponible.
    /// </summary>
    public Guid? RequestedByUserId { get; init; }
}

/// <summary>
/// Representa una fila normalizada de importación de productos.
/// </summary>
public sealed class ImportProductRowCommand
{
    /// <summary>
    /// Número de fila original dentro de la plantilla cargada.
    /// </summary>
    public int RowNumber { get; init; }

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
    /// Código de moneda asociado al precio.
    /// </summary>
    public string Currency { get; init; } = string.Empty;

    /// <summary>
    /// Stock inicial del producto.
    /// </summary>
    public int Stock { get; init; }

    /// <summary>
    /// Indica si el producto debe quedar activo tras la importación.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Tipo funcional del producto.
    /// </summary>
    public TipoProducto ProductType { get; init; }

    /// <summary>
    /// Slug comercial del producto.
    /// </summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>
    /// Nombre visible de la categoría principal a resolver.
    /// </summary>
    public string CategoryName { get; init; } = string.Empty;

    /// <summary>
    /// Nombre visible de la subcategoría a resolver cuando aplica.
    /// </summary>
    public string? SubcategoryName { get; init; }

    /// <summary>
    /// Representación serializada de etiquetas suministrada por el archivo.
    /// </summary>
    public string? SerializedTags { get; init; }

    /// <summary>
    /// Formato principal del archivo para productos digitales.
    /// </summary>
    public string? FileFormat { get; init; }

    /// <summary>
    /// Tamaño del archivo digital en megabytes cuando aplica.
    /// </summary>
    public decimal? FileSizeMb { get; init; }

    /// <summary>
    /// Indica si el producto digital requiere licencia.
    /// </summary>
    public bool? RequiresLicense { get; init; }

    /// <summary>
    /// Peso en kilogramos para productos físicos.
    /// </summary>
    public decimal? WeightKg { get; init; }

    /// <summary>
    /// Alto en centímetros para productos físicos.
    /// </summary>
    public decimal? HeightCm { get; init; }

    /// <summary>
    /// Ancho en centímetros para productos físicos.
    /// </summary>
    public decimal? WidthCm { get; init; }

    /// <summary>
    /// Largo en centímetros para productos físicos.
    /// </summary>
    public decimal? LengthCm { get; init; }

    /// <summary>
    /// Indica si el producto físico requiere envío.
    /// </summary>
    public bool? RequiresShipping { get; init; }
}
