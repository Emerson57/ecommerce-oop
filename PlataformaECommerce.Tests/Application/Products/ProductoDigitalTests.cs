using PlataformaECommerce.Domain.Entities.Products;
using PlataformaECommerce.Domain.Exceptions;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Tests.Application.Products;

[TestFixture]
public class ProductoDigitalTests
{
    [Test]
    public void Constructor_DatosValidos_CreaProductoDigitalCorrectamente()
    {
        ProductoDigital producto = CrearProductoDigital();

        Assert.That(producto.FormatoArchivo, Is.EqualTo("MP4"));
    }

    [Test]
    public void Constructor_FormatoArchivoVacio_LanzaProductException()
    {
        Assert.Throws<ProductException>(() => CrearProductoDigital(formatoArchivo: string.Empty));
    }

    [Test]
    public void ActualizarInformacionDigital_DatosValidos_ActualizaFormato()
    {
        ProductoDigital producto = CrearProductoDigital();

        producto.ActualizarInformacionDigital("PDF", 25.4m, false);

        Assert.That(producto.FormatoArchivo, Is.EqualTo("PDF"));
    }

    [Test]
    public void EsArchivoLiviano_TamanoMenorOIgualA100_RetornaTrue()
    {
        ProductoDigital producto = CrearProductoDigital(tamanoArchivoMb: 80m);

        Assert.That(producto.EsArchivoLiviano(), Is.True);
    }

    [Test]
    public void EstaListoParaEntregaInmediata_ProductoSinLicencia_RetornaTrue()
    {
        ProductoDigital producto = CrearProductoDigital(requiereLicencia: false);

        Assert.That(producto.EstaListoParaEntregaInmediata(), Is.True);
    }

    private static ProductoDigital CrearProductoDigital(
        string formatoArchivo = "MP4",
        decimal? tamanoArchivoMb = 850.75m,
        bool requiereLicencia = true)
    {
        return new ProductoDigital(
            "Curso C# Avanzado",
            "Curso en video con contenido técnico avanzado.",
            new Sku("CURSO-001"),
            new Money(120000m, "COP"),
            100,
            "curso-csharp-avanzado",
            null,
            null,
            null,
            null,
            formatoArchivo,
            tamanoArchivoMb,
            requiereLicencia);
    }
}
