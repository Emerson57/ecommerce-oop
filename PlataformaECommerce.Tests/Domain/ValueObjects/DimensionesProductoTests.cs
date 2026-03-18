using PlataformaECommerce.Domain.Exceptions;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Tests.Domain.ValueObjects;

[TestFixture]
public class DimensionesProductoTests
{
    [Test]
    public void Constructor_DimensionesValidas_CalculaVolumen()
    {
        DimensionesProducto dimensiones = new(10m, 20m, 30m);

        Assert.That(dimensiones.VolumenCm3, Is.EqualTo(6000m));
    }

    [Test]
    public void Constructor_AltoInvalido_LanzaProductException()
    {
        Assert.Throws<ProductException>(() => new DimensionesProducto(0m, 20m, 30m));
    }

    [Test]
    public void EsVoluminosa_VolumenMayorAlUmbral_RetornaTrue()
    {
        DimensionesProducto dimensiones = new(100m, 50m, 30m);

        Assert.That(dimensiones.EsVoluminosa(100000m), Is.True);
    }
}
