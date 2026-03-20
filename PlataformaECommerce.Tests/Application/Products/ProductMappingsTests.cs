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

    private static ProductoDigital CrearProducto(Guid? categoriaId, IEnumerable<EtiquetaProducto> etiquetas)
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
            false);
    }
}