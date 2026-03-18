using PlataformaECommerce.Domain.Entities.Products;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.Exceptions;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Tests.Application.Products;

[TestFixture]
public class ProductoTests
{
    [Test]
    public void ActualizarPrecio_ValorValido_ActualizaPrecio()
    {
        ProductoDigital producto = CrearProducto();

        producto.ActualizarPrecio(new Money(65000m, "COP"));

        Assert.That(producto.Precio.Amount, Is.EqualTo(65000m));
    }

    [Test]
    public void ActualizarStock_ValorNegativo_LanzaProductException()
    {
        ProductoDigital producto = CrearProducto();

        Assert.Throws<ProductException>(() => producto.ActualizarStock(-1));
    }

    [Test]
    public void DisminuirStock_CantidadMayorAlStock_LanzaInventarioInsuficienteException()
    {
        ProductoDigital producto = CrearProducto();

        Assert.Throws<InventarioInsuficienteException>(() => producto.DisminuirStock(50));
    }

    [Test]
    public void EstaDisponible_ProductoActivoYConStock_RetornaTrue()
    {
        ProductoDigital producto = CrearProducto();
        producto.Activar();

        Assert.That(producto.EstaDisponible(), Is.True);
    }

    [Test]
    public void QuitarClasificacion_ProductoClasificado_LimpiaCategoriaYSubcategoria()
    {
        ProductoDigital producto = CrearProducto(categoriaId: Guid.NewGuid(), subcategoriaId: Guid.NewGuid());

        producto.QuitarClasificacion();

        Assert.That(producto.CategoriaId, Is.Null);
    }

    [Test]
    public void Constructor_ProductoDigital_AsignaTipoDigital()
    {
        ProductoDigital producto = CrearProducto();

        Assert.That(producto.TipoProducto, Is.EqualTo(TipoProducto.Digital));
    }

    [Test]
    public void ActualizarPrecio_ConMonedaDistinta_LanzaProductException()
    {
        ProductoDigital producto = CrearProducto();

        Assert.Throws<ProductException>(() => producto.ActualizarPrecio(new Money(65000m, "USD")));
    }

    [Test]
    public void ReemplazarEtiquetas_SuperaMaximoPermitido_LanzaProductException()
    {
        ProductoDigital producto = CrearProducto();
        EtiquetaProducto[] etiquetas = Enumerable.Range(1, 21)
            .Select(index => new EtiquetaProducto($"tag-{index}"))
            .ToArray();

        Assert.Throws<ProductException>(() => producto.ReemplazarEtiquetas(etiquetas));
    }

    private static ProductoDigital CrearProducto(Guid? categoriaId = null, Guid? subcategoriaId = null)
    {
        return new ProductoDigital(
            "Ebook C#",
            "Guía completa de C# para principiantes.",
            new Sku("EBOOK-001"),
            new Money(50000m, "COP"),
            10,
            "ebook-csharp",
            null,
            categoriaId,
            subcategoriaId,
            null,
            "PDF",
            15.5m,
            false);
    }
}
