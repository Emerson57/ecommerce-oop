using PlataformaECommerce.Application.Features.Products.Commands;
using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Web.Contracts.Products;

/// <summary>
/// Representa la solicitud HTTP para crear un producto físico desde la API administrativa.
/// </summary>
public sealed record CreatePhysicalProductRequest
{
    /// <summary>
    /// Nombre comercial del producto.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Descripción comercial o funcional del producto.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// SKU del producto.
    /// </summary>
    public string Sku { get; init; } = string.Empty;

    /// <summary>
    /// Precio base del producto.
    /// </summary>
    public decimal Price { get; init; }

    /// <summary>
    /// Código de moneda asociado al precio.
    /// </summary>
    public string Currency { get; init; } = "COP";

    /// <summary>
    /// Stock inicial del producto.
    /// </summary>
    public int Stock { get; init; }

    /// <summary>
    /// Identificador amigable para URL.
    /// </summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>
    /// URL o ruta de la imagen principal.
    /// </summary>
    public string? MainImageUrl { get; init; }

    /// <summary>
    /// Indica si el producto debe quedar activo.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Indica si el producto debe quedar destacado.
    /// </summary>
    public bool IsFeatured { get; init; }

    /// <summary>
    /// Categoría principal del producto.
    /// </summary>
    public Guid? CategoryId { get; init; }

    /// <summary>
    /// Subcategoría del producto.
    /// </summary>
    public Guid? SubcategoryId { get; init; }

    /// <summary>
    /// Etiquetas comerciales del producto.
    /// </summary>
    public IReadOnlyCollection<string> Tags { get; init; } = Array.Empty<string>();

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
}

/// <summary>
/// Representa la solicitud HTTP para crear un producto digital desde la API administrativa.
/// </summary>
public sealed record CreateDigitalProductRequest
{
    /// <summary>
    /// Nombre comercial del producto.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Descripción comercial o funcional del producto.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// SKU del producto.
    /// </summary>
    public string Sku { get; init; } = string.Empty;

    /// <summary>
    /// Precio base del producto.
    /// </summary>
    public decimal Price { get; init; }

    /// <summary>
    /// Código de moneda asociado al precio.
    /// </summary>
    public string Currency { get; init; } = "COP";

    /// <summary>
    /// Stock inicial del producto.
    /// </summary>
    public int Stock { get; init; }

    /// <summary>
    /// Identificador amigable para URL.
    /// </summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>
    /// URL o ruta de la imagen principal.
    /// </summary>
    public string? MainImageUrl { get; init; }

    /// <summary>
    /// Indica si el producto debe quedar activo.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Indica si el producto debe quedar destacado.
    /// </summary>
    public bool IsFeatured { get; init; }

    /// <summary>
    /// Categoría principal del producto.
    /// </summary>
    public Guid? CategoryId { get; init; }

    /// <summary>
    /// Etiquetas comerciales del producto.
    /// </summary>
    public IReadOnlyCollection<string> Tags { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Formato principal del archivo digital.
    /// </summary>
    public string FileFormat { get; init; } = string.Empty;

    /// <summary>
    /// Tamaño del archivo digital en megabytes.
    /// </summary>
    public decimal? FileSizeMb { get; init; }

    /// <summary>
    /// Indica si el producto digital requiere licencia.
    /// </summary>
    public bool RequiresLicense { get; init; }
}

/// <summary>
/// Representa la solicitud HTTP para actualizar integralmente un producto existente.
/// </summary>
public sealed record UpdateProductRequest
{
    /// <summary>
    /// Nombre comercial del producto.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Descripción comercial o funcional del producto.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// SKU del producto.
    /// </summary>
    public string Sku { get; init; } = string.Empty;

    /// <summary>
    /// Precio base del producto.
    /// </summary>
    public decimal Price { get; init; }

    /// <summary>
    /// Código de moneda asociado al precio.
    /// </summary>
    public string Currency { get; init; } = "COP";

    /// <summary>
    /// Stock actual del producto.
    /// </summary>
    public int Stock { get; init; }

    /// <summary>
    /// Identificador amigable para URL.
    /// </summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>
    /// URL o ruta de la imagen principal.
    /// </summary>
    public string? MainImageUrl { get; init; }

    /// <summary>
    /// Indica si el producto está activo.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Indica si el producto está destacado.
    /// </summary>
    public bool IsFeatured { get; init; }

    /// <summary>
    /// Tipo funcional del producto.
    /// </summary>
    public TipoProducto ProductType { get; init; }

    /// <summary>
    /// Categoría principal del producto.
    /// </summary>
    public Guid? CategoryId { get; init; }

    /// <summary>
    /// Etiquetas comerciales del producto.
    /// </summary>
    public IReadOnlyCollection<string> Tags { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Peso del producto físico.
    /// </summary>
    public decimal? WeightKg { get; init; }

    /// <summary>
    /// Alto del producto físico.
    /// </summary>
    public decimal? HeightCm { get; init; }

    /// <summary>
    /// Ancho del producto físico.
    /// </summary>
    public decimal? WidthCm { get; init; }

    /// <summary>
    /// Largo del producto físico.
    /// </summary>
    public decimal? LengthCm { get; init; }

    /// <summary>
    /// Indica si el producto físico requiere envío.
    /// </summary>
    public bool? RequiresShipping { get; init; }

    /// <summary>
    /// Formato principal del archivo digital.
    /// </summary>
    public string? FileFormat { get; init; }

    /// <summary>
    /// Tamaño del archivo digital en megabytes.
    /// </summary>
    public decimal? FileSizeMb { get; init; }

    /// <summary>
    /// Indica si el producto digital requiere licencia.
    /// </summary>
    public bool? RequiresLicense { get; init; }
}

/// <summary>
/// Representa la solicitud HTTP para activar un producto.
/// </summary>
public sealed record ActivateProductRequest
{
    /// <summary>
    /// Identificador opcional del usuario que solicita la activación.
    /// </summary>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>
    /// Motivo funcional asociado a la activación.
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// Referencia externa asociada a la activación.
    /// </summary>
    public string? ExternalReference { get; init; }
}

/// <summary>
/// Representa la solicitud HTTP para desactivar un producto.
/// </summary>
public sealed record DeactivateProductRequest
{
    /// <summary>
    /// Identificador opcional del usuario que solicita la desactivación.
    /// </summary>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>
    /// Motivo funcional asociado a la desactivación.
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// Referencia externa asociada a la desactivación.
    /// </summary>
    public string? ExternalReference { get; init; }
}

/// <summary>
/// Representa la solicitud HTTP para destacar un producto.
/// </summary>
public sealed record FeatureProductRequest
{
    /// <summary>
    /// Identificador opcional del usuario que solicita el destacado.
    /// </summary>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>
    /// Motivo funcional asociado al destacado.
    /// </summary>
    public string? Reason { get; init; }
}

/// <summary>
/// Representa la solicitud HTTP para retirar el destacado de un producto.
/// </summary>
public sealed record UnfeatureProductRequest
{
    /// <summary>
    /// Identificador opcional del usuario que solicita retirar el destacado.
    /// </summary>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>
    /// Motivo funcional asociado al retiro del destacado.
    /// </summary>
    public string? Reason { get; init; }
}

/// <summary>
/// Representa la solicitud HTTP para ajustar el inventario de un producto.
/// </summary>
public sealed record UpdateProductStockRequest
{
    /// <summary>
    /// Tipo de ajuste de inventario.
    /// </summary>
    public StockUpdateType UpdateType { get; init; }

    /// <summary>
    /// Cantidad involucrada en el ajuste.
    /// </summary>
    public int Quantity { get; init; }

    /// <summary>
    /// Motivo funcional del ajuste.
    /// </summary>
    public string Reason { get; init; } = string.Empty;

    /// <summary>
    /// Identificador opcional del usuario que solicita el ajuste.
    /// </summary>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>
    /// Referencia externa asociada al ajuste.
    /// </summary>
    public string? ExternalReference { get; init; }
}

/// <summary>
/// Representa la solicitud HTTP para aplicar una promoción a un producto.
/// </summary>
public sealed record ApplyProductPromotionRequest
{
    /// <summary>
    /// Porcentaje de descuento solicitado.
    /// </summary>
    public decimal DiscountPercentage { get; init; }

    /// <summary>
    /// Identificador opcional del usuario que solicita la promoción.
    /// </summary>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>
    /// Motivo funcional asociado a la promoción.
    /// </summary>
    public string? Reason { get; init; }
}

/// <summary>
/// Representa la solicitud HTTP para retirar una promoción activa.
/// </summary>
public sealed record RemoveProductPromotionRequest
{
    /// <summary>
    /// Identificador opcional del usuario que solicita la restauración del precio base.
    /// </summary>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>
    /// Motivo funcional asociado al retiro de la promoción.
    /// </summary>
    public string? Reason { get; init; }
}