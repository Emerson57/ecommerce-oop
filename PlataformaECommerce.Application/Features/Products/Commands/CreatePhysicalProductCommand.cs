using PlataformaECommerce.Application.Abstractions;
using PlataformaECommerce.Application.Common.Results;

namespace PlataformaECommerce.Application.Features.Products.Commands;

/// <summary>
/// Representa el comando de aplicación para registrar un nuevo producto físico.
/// </summary>
/// <remarks>
/// Este comando transporta la información necesaria desde la capa de entrada
/// hacia el caso de uso de creación de productos físicos.
///
/// Su responsabilidad es exclusivamente descriptiva:
/// - no contiene lógica de dominio,
/// - no ejecuta validaciones complejas,
/// - no transforma objetos de valor.
///
/// La validación detallada debe resolverse mediante validadores de Application,
/// y la conversión a objetos del dominio debe realizarse en el servicio o handler correspondiente.
/// </remarks>
public sealed class CreatePhysicalProductCommand : ICommand<Result<Guid>>
{
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

    #endregion

    #region Estado inicial

    /// <summary>
    /// Indica si el producto debe activarse tras su creación.
    /// </summary>
    /// <remarks>
    /// Aunque el dominio puede decidir un estado inicial por defecto,
    /// este contrato permite a la capa de aplicación controlar explícitamente
    /// el estado de publicación inicial cuando el caso de uso lo requiera.
    /// </remarks>
    public bool IsActive { get; init; }

    /// <summary>
    /// Indica si el producto debe marcarse como destacado tras su creación.
    /// </summary>
    public bool IsFeatured { get; init; }

    #endregion

    #region Clasificación comercial

    /// <summary>
    /// Identificador de la categoría principal del producto.
    /// </summary>
    public Guid? CategoryId { get; init; }

    /// <summary>
    /// Identificador de la subcategoría del producto.
    /// </summary>
    public Guid? SubcategoryId { get; init; }

    /// <summary>
    /// Colección de etiquetas asociadas al producto.
    /// </summary>
    /// <remarks>
    /// En la capa Application las etiquetas se modelan como texto simple.
    /// La conversión a <c>EtiquetaProducto</c> debe realizarse en el servicio de aplicación.
    /// </remarks>
    public IReadOnlyCollection<string> Tags { get; init; } = Array.Empty<string>();

    #endregion

    #region Información logística del producto físico

    /// <summary>
    /// Peso del producto en kilogramos.
    /// </summary>
    public decimal WeightKg { get; init; }

    /// <summary>
    /// Alto del producto en centímetros.
    /// </summary>
    public decimal HeightCm { get; init; }

    /// <summary>
    /// Ancho del producto en centímetros.
    /// </summary>
    public decimal WidthCm { get; init; }

    /// <summary>
    /// Largo del producto en centímetros.
    /// </summary>
    public decimal LengthCm { get; init; }

    /// <summary>
    /// Indica si el producto requiere envío físico.
    /// </summary>
    public bool RequiresShipping { get; init; } = true;

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del comando.
    /// </summary>
    /// <returns>Cadena representativa del comando de creación de producto físico.</returns>
    public override string ToString()
    {
        return $"CreatePhysicalProductCommand | Name: {Name} | Sku: {Sku} | Price: {Currency} {Price:N2} | Stock: {Stock} | CategoryId: {CategoryId} | SubcategoryId: {SubcategoryId}";
    }

    #endregion
}