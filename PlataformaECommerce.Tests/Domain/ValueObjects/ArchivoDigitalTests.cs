using PlataformaECommerce.Domain.Exceptions;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Tests.Domain.ValueObjects;

[TestFixture]
public class ArchivoDigitalTests
{
    [Test]
    public void Constructor_DatosValidos_NormalizaFormato()
    {
        ArchivoDigital archivo = new("pdf", 80m);

        Assert.That(archivo.Formato, Is.EqualTo("PDF"));
    }

    [Test]
    public void Constructor_FormatoVacio_LanzaProductException()
    {
        Assert.Throws<ProductException>(() => new ArchivoDigital(string.Empty, 80m));
    }

    [Test]
    public void EsLiviano_TamanoMenorOIgualAlUmbral_RetornaTrue()
    {
        ArchivoDigital archivo = new("PDF", 80m);

        Assert.That(archivo.EsLiviano(100m), Is.True);
    }
}
