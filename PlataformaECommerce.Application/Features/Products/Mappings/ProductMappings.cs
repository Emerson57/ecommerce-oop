using PlataformaECommerce.Application.Features.Products.DTOs;
using PlataformaECommerce.Domain.Entities.Products;

namespace PlataformaECommerce.Application.Features.Products.Mappings;

/// <summary>
/// Proporciona métodos de extensión para mapear entidades del dominio de productos
/// hacia objetos de transferencia de datos de la capa Application.
/// </summary>
/// <remarks>
/// Esta clase centraliza la lógica de proyección de la entidad <see cref="Producto"/>
/// y sus especializaciones hacia DTOs de lectura y respuesta, evitando duplicación
/// de código en servicios de aplicación, páginas consumidoras y consultas.
///
/// Su propósito es:
/// - mantener consistencia en las proyecciones,
/// - desacoplar la capa Application del detalle de serialización,
/// - facilitar mantenimiento,
/// - y mejorar la legibilidad de los casos de uso.
///
/// La clase soporta tanto productos físicos como productos digitales
/// a través de inspección del tipo concreto del agregado.
/// </remarks>
public static class ProductMappings
{
    #region Mapeos individuales

    /// <summary>
    /// Proyecta una entidad <see cref="Producto"/> hacia un <see cref="ProductDto"/>.
    /// </summary>
    /// <param name="product">Producto a proyectar.</param>
    /// <returns>DTO general del producto.</returns>
    /// <exception cref="ArgumentNullException">
    /// Se produce cuando el producto es nulo.
    /// </exception>
    public static ProductDto ToProductDto(this Producto product)
    {
        ArgumentNullException.ThrowIfNull(product);

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Nombre,
            Description = product.Descripcion,
            Sku = product.Sku.Value,
            Price = product.Precio.Amount,
            BasePrice = product.PrecioBase.Amount,
            PromotionalPrice = product.PrecioPromocionalActual?.Amount,
            Currency = product.Precio.Currency,
            Stock = product.Stock,
            IsActive = product.Activo,
            IsFeatured = product.Destacado,
            HasPromotion = product.TienePromocion,
            CurrentDiscountPercentage = product.DescuentoPromocionalActual,
            Slug = product.Slug,
            MainImageUrl = product.ImagenPrincipalUrl,
            ImageGallery = BuildImageGallery(product),
            ProductType = product.TipoProducto,
            CategoryId = product.CategoriaId,
            SubcategoryId = product.SubcategoriaId,
            CreatedAtUtc = product.FechaCreacionUtc,
            UpdatedAtUtc = product.FechaActualizacionUtc,
            WeightKg = product is ProductoFisico physical ? physical.PesoKg : null,
            HeightCm = product is ProductoFisico physicalHeight ? physicalHeight.AltoCm : null,
            WidthCm = product is ProductoFisico physicalWidth ? physicalWidth.AnchoCm : null,
            LengthCm = product is ProductoFisico physicalLength ? physicalLength.LargoCm : null,
            RequiresShipping = product is ProductoFisico physicalShipping ? physicalShipping.RequiereEnvio : null,
            FileFormat = product is ProductoDigital digital ? digital.FormatoArchivo : null,
            FileSizeMb = product is ProductoDigital digitalSize ? digitalSize.TamanoArchivoMb : null,
            RequiresLicense = product is ProductoDigital digitalLicense ? digitalLicense.RequiereLicencia : null
        };
    }

    /// <summary>
    /// Proyecta una entidad <see cref="Producto"/> hacia un <see cref="ProductDetailDto"/>.
    /// </summary>
    /// <param name="product">Producto a proyectar.</param>
    /// <returns>DTO detallado del producto.</returns>
    /// <exception cref="ArgumentNullException">
    /// Se produce cuando el producto es nulo.
    /// </exception>
    public static ProductDetailDto ToProductDetailDto(this Producto product)
    {
        ArgumentNullException.ThrowIfNull(product);

        return new ProductDetailDto
        {
            Id = product.Id,
            Name = product.Nombre,
            Description = product.Descripcion,
            Sku = product.Sku.Value,
            Slug = product.Slug,
            Price = product.Precio.Amount,
            BasePrice = product.PrecioBase.Amount,
            PromotionalPrice = product.PrecioPromocionalActual?.Amount,
            Currency = product.Precio.Currency,
            Stock = product.Stock,
            IsActive = product.Activo,
            IsFeatured = product.Destacado,
            HasPromotion = product.TienePromocion,
            CurrentDiscountPercentage = product.DescuentoPromocionalActual,
            ProductType = product.TipoProducto,
            CategoryId = product.CategoriaId,
            SubcategoryId = product.SubcategoriaId,
            Tags = product.Etiquetas.Select(tag => tag.Value).ToArray(),
            MainImageUrl = product.ImagenPrincipalUrl,
            ImageGallery = BuildImageGallery(product),
            WeightKg = product is ProductoFisico physical ? physical.PesoKg : null,
            HeightCm = product is ProductoFisico physicalHeight ? physicalHeight.AltoCm : null,
            WidthCm = product is ProductoFisico physicalWidth ? physicalWidth.AnchoCm : null,
            LengthCm = product is ProductoFisico physicalLength ? physicalLength.LargoCm : null,
            RequiresShipping = product is ProductoFisico physicalShipping ? physicalShipping.RequiereEnvio : null,
            FileFormat = product is ProductoDigital digital ? digital.FormatoArchivo : null,
            FileSizeMb = product is ProductoDigital digitalSize ? digitalSize.TamanoArchivoMb : null,
            RequiresLicense = product is ProductoDigital digitalLicense ? digitalLicense.RequiereLicencia : null,
            CreatedAtUtc = product.FechaCreacionUtc,
            UpdatedAtUtc = product.FechaActualizacionUtc,
            CreatedByUserId = null,
            UpdatedByUserId = null
        };
    }

    /// <summary>
    /// Proyecta una entidad <see cref="Producto"/> hacia un <see cref="ProductResponseDto"/>.
    /// </summary>
    /// <param name="product">Producto a proyectar.</param>
    /// <returns>DTO de respuesta del producto.</returns>
    /// <exception cref="ArgumentNullException">
    /// Se produce cuando el producto es nulo.
    /// </exception>
    public static ProductResponseDto ToProductResponseDto(this Producto product)
    {
        ArgumentNullException.ThrowIfNull(product);

        return new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Nombre,
            Description = product.Descripcion,
            Sku = product.Sku.Value,
            Slug = product.Slug,
            Price = product.Precio.Amount,
            BasePrice = product.PrecioBase.Amount,
            PromotionalPrice = product.PrecioPromocionalActual?.Amount,
            Currency = product.Precio.Currency,
            Stock = product.Stock,
            IsActive = product.Activo,
            IsFeatured = product.Destacado,
            HasPromotion = product.TienePromocion,
            CurrentDiscountPercentage = product.DescuentoPromocionalActual,
            ProductType = product.TipoProducto,
            CategoryId = product.CategoriaId,
            SubcategoryId = product.SubcategoriaId,
            Tags = product.Etiquetas.Select(tag => tag.Value).ToArray(),
            MainImageUrl = product.ImagenPrincipalUrl,
            ImageGallery = BuildImageGallery(product),
            WeightKg = product is ProductoFisico physical ? physical.PesoKg : null,
            HeightCm = product is ProductoFisico physicalHeight ? physicalHeight.AltoCm : null,
            WidthCm = product is ProductoFisico physicalWidth ? physicalWidth.AnchoCm : null,
            LengthCm = product is ProductoFisico physicalLength ? physicalLength.LargoCm : null,
            RequiresShipping = product is ProductoFisico physicalShipping ? physicalShipping.RequiereEnvio : null,
            FileFormat = product is ProductoDigital digital ? digital.FormatoArchivo : null,
            FileSizeMb = product is ProductoDigital digitalSize ? digitalSize.TamanoArchivoMb : null,
            RequiresLicense = product is ProductoDigital digitalLicense ? digitalLicense.RequiereLicencia : null,
            CreatedAtUtc = product.FechaCreacionUtc,
            UpdatedAtUtc = product.FechaActualizacionUtc
        };
    }

    #endregion

    #region Mapeos de colecciones

    /// <summary>
    /// Proyecta una colección de entidades <see cref="Producto"/> hacia una colección de <see cref="ProductDto"/>.
    /// </summary>
    /// <param name="products">Colección de productos a proyectar.</param>
    /// <returns>Colección de DTOs de producto.</returns>
    /// <exception cref="ArgumentNullException">
    /// Se produce cuando la colección es nula.
    /// </exception>
    public static IReadOnlyCollection<ProductDto> ToProductDtos(this IEnumerable<Producto> products)
    {
        ArgumentNullException.ThrowIfNull(products);

        return products
            .Select(product => product.ToProductDto())
            .ToArray();
    }

    /// <summary>
    /// Proyecta una colección de entidades <see cref="Producto"/> hacia una colección de <see cref="ProductResponseDto"/>.
    /// </summary>
    /// <param name="products">Colección de productos a proyectar.</param>
    /// <returns>Colección de DTOs de respuesta de producto.</returns>
    /// <exception cref="ArgumentNullException">
    /// Se produce cuando la colección es nula.
    /// </exception>
    public static IReadOnlyCollection<ProductResponseDto> ToProductResponseDtos(this IEnumerable<Producto> products)
    {
        ArgumentNullException.ThrowIfNull(products);

        return products
            .Select(product => product.ToProductResponseDto())
            .ToArray();
    }

    #endregion

    #region Métodos privados auxiliares

    /// <summary>
    /// Construye la galería complementaria persistida para el producto.
    /// </summary>
    /// <param name="product">Producto origen.</param>
    /// <returns>Colección de imágenes complementarias del producto.</returns>
    private static IReadOnlyCollection<string> BuildImageGallery(Producto product)
    {
        return product.GaleriaImagenes.ToArray();
    }

    #endregion
}
