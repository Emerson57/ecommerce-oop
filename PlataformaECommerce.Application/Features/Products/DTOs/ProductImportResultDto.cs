namespace PlataformaECommerce.Application.Features.Products.DTOs;

/// <summary>
/// Representa el resumen de una importación masiva de productos.
/// </summary>
public sealed class ProductImportResultDto
{
    /// <summary>
    /// Cantidad de productos físicos creados.
    /// </summary>
    public int PhysicalProductsCreated { get; init; }

    /// <summary>
    /// Cantidad de productos digitales creados.
    /// </summary>
    public int DigitalProductsCreated { get; init; }

    /// <summary>
    /// Cantidad total de productos creados.
    /// </summary>
    public int TotalCreated => PhysicalProductsCreated + DigitalProductsCreated;
}
