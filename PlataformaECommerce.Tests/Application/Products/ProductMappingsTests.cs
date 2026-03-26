using PlataformaECommerce.Application.Features.Products.Mappings;
using PlataformaECommerce.Domain.Entities.Products;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Tests.Application.Products;

[TestFixture]
public class ProductMappingsTests
{
    [Test]
    public void ToProductDetailDto_ProductoClasificado_ProyectaCategoryId()
    {
        Guid categoriaId = Guid.NewGuid();
        ProductoDigital producto = CrearProducto(categoriaId, new[] { new EtiquetaProducto("nuevo"), new EtiquetaProducto("oferta") });

        var dto = producto.ToProductDetailDto();

        Assert.That(dto.CategoryId, Is.EqualTo(categoriaId));
    }

    [Test]
    public void ToProductResponseDto_ProductoConEtiquetas_ProyectaTags()
    {
        ProductoDigital producto = CrearProducto(Guid.NewGuid(), new[] { new EtiquetaProducto("nuevo"), new EtiquetaProducto("oferta") });

        var dto = producto.ToProductResponseDto();

        Assert.That(dto.Tags, Is.EqualTo(new[] { "nuevo", "oferta" }));
    }

    [Test]
    public void ToProductDto_ProductoConGaleria_ProyectaImageGallery()
    {
        ProductoDigital producto = CrearProducto(
            Guid.NewGuid(),
            new[] { new EtiquetaProducto("nuevo") },
            ["https://cdn.novashop.com/products/curso-csharp-1.webp", "/images/products/curso-csharp-2.webp"]);

        var dto = producto.ToProductDto();

        Assert.That(dto.ImageGallery, Is.EqualTo(new[]
        {
            "https://cdn.novashop.com/products/curso-csharp-1.webp",
            "/images/products/curso-csharp-2.webp"
        }));
    }

    private static ProductoDigital CrearProducto(
        Guid? categoriaId,
        IEnumerable<EtiquetaProducto> etiquetas,
        IEnumerable<string>? imageGallery = null)
    {
        return new ProductoDigital(
            "Curso C#",
            "Curso de prueba.",
            new Sku("MAP-001"),
            new Money(100m, "COP"),
            10,
            "curso-csharp",
            null,
            categoriaId,
            null,
            etiquetas,
            "PDF",
            5m,
            false,
            imageGallery);
    }
}