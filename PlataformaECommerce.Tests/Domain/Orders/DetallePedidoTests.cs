using PlataformaECommerce.Domain.Entities.Orders;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.Exceptions;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Tests.Domain.Orders;

[TestFixture]
public class DetallePedidoTests
{
    [Test]
    public void Constructor_CantidadMayorAlMaximo_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() =>
            new DetallePedido(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Producto de prueba",
                new Sku("DET-001"),
                TipoProducto.Digital,
                null,
                new Money(15m, "COP"),
                1000));
    }
}