using PlataformaECommerce.Domain.Exceptions;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Tests.Domain.ValueObjects;

[TestFixture]
public class DireccionEnvioTests
{
    [Test]
    public void Constructor_DatosValidos_CreaDireccionCorrectamente()
    {
        DireccionEnvio direccion = new("Calle 123", "Bogotá", "Cundinamarca", "Colombia", "110111");

        Assert.That(direccion.Ciudad, Is.EqualTo("Bogotá"));
    }

    [Test]
    public void Constructor_CalleVacia_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => new DireccionEnvio(string.Empty, "Bogotá", "Cundinamarca", "Colombia", "110111"));
    }

    [Test]
    public void Equals_MismosValores_RetornaTrue()
    {
        DireccionEnvio left = new("Calle 123", "Bogotá", "Cundinamarca", "Colombia", "110111");
        DireccionEnvio right = new("Calle 123", "Bogotá", "Cundinamarca", "Colombia", "110111");

        Assert.That(left.Equals(right), Is.True);
    }
}
