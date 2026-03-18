using PlataformaECommerce.Domain.Exceptions;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Tests.Domain.ValueObjects;

[TestFixture]
public class MoneyTests
{
    [Test]
    public void Constructor_DatosValidos_NormalizaMoneda()
    {
        Money money = new(100m, "usd");

        Assert.That(money.Currency, Is.EqualTo("USD"));
    }

    [Test]
    public void Constructor_MontoNegativo_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => new Money(-1m, "COP"));
    }

    [Test]
    public void Suma_MismaMoneda_RetornaMontoAcumulado()
    {
        Money left = new(100m, "COP");
        Money right = new(50m, "COP");

        Money result = left + right;

        Assert.That(result.Amount, Is.EqualTo(150m));
    }

    [Test]
    public void HasSameCurrency_MismaMoneda_RetornaTrue()
    {
        Money left = new(100m, "COP");
        Money right = new(50m, "COP");

        Assert.That(left.HasSameCurrency(right), Is.True);
    }

    [Test]
    public void CompareTo_DistintaMoneda_LanzaDomainException()
    {
        Money left = new(100m, "COP");
        Money right = new(50m, "USD");

        Assert.Throws<DomainException>(() => left.CompareTo(right));
    }

    [Test]
    public void HasSameCurrency_MonedasDistintas_RetornaFalse()
    {
        Money left = new(100m, "COP");
        Money right = new(50m, "USD");

        Assert.That(left.HasSameCurrency(right), Is.False);
    }

    [Test]
    public void MultiplicacionPorDecimalNegativo_LanzaDomainException()
    {
        Money money = new(100m, "COP");

        Assert.Throws<DomainException>(() => _ = money * -1m);
    }
}
