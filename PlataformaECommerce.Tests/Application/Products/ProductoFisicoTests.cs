using PlataformaECommerce.Domain.Entities.Products;
using PlataformaECommerce.Domain.Exceptions;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Tests.Application.Products;

[TestFixture]
public class ProductoFisicoTests
{
    [Test]
    public void Constructor_DatosValidos_CreaProductoFisicoCorrectamente()
    {
        ProductoFisico producto = CrearProductoFisico();

        Assert.That(producto.TipoProducto, Is.EqualTo(PlataformaECommerce.Domain.Enums.TipoProducto.Fisico));
    }

    [Test]
    public void Constructor_PesoInvalido_LanzaProductException()
    {
        Assert.Throws<ProductException>(() => CrearProductoFisico(pesoKg: 0m));
    }

    [Test]
    public void ActualizarInformacionFisica_DatosValidos_ActualizaPeso()
    {
        ProductoFisico producto = CrearProductoFisico();

        producto.ActualizarInformacionFisica(2.5m, 10m, 20m, 50m, true);

        Assert.That(producto.PesoKg, Is.EqualTo(2.5m));
    }

    [Test]
    public void EsVoluminoso_VolumenMayorAlUmbral_RetornaTrue()
    {
        ProductoFisico producto = CrearProductoFisico(altoCm: 200m, anchoCm: 100m, largoCm: 80m, pesoKg: 80m);

        Assert.That(producto.EsVoluminoso(), Is.True);
    }

    [Test]
    public void RequiereManejoEspecial_ProductoVoluminosoYConEnvio_RetornaTrue()
    {
        ProductoFisico producto = CrearProductoFisico(altoCm: 200m, anchoCm: 100m, largoCm: 80m, pesoKg: 80m, requiereEnvio: true);

        Assert.That(producto.RequiereManejoEspecial(), Is.True);
    }

    private static ProductoFisico CrearProductoFisico(
        decimal pesoKg = 1.2m,
        decimal altoCm = 4.5m,
        decimal anchoCm = 18m,
        decimal largoCm = 45m,
        bool requiereEnvio = true)
    {
        return new ProductoFisico(
            "Teclado Mecánico",
            "Teclado mecánico con iluminación RGB.",
            new Sku("TEC-001"),
            new Money(350000m, "COP"),
            20,
            "teclado-mecanico",
            null,
            null,
            null,
            null,
            pesoKg,
            altoCm,
            anchoCm,
            largoCm,
            requiereEnvio);
    }
}
