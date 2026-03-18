using PlataformaECommerce.Domain.Entities.Cart;
using PlataformaECommerce.Domain.Entities.Products;
using PlataformaECommerce.Domain.Exceptions;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Tests.Domain.Cart;

[TestFixture]
public class ItemCarritoTests
{
    [Test]
    public void Constructor_CantidadMayorAlMaximo_LanzaCartException()
    {
        ProductoDigital producto = CrearProductoDigital();

        Assert.Throws<CartException>(() => new ItemCarrito(producto, 1000));
    }

    private static ProductoDigital CrearProductoDigital()
    {
        ProductoDigital producto = new(
            "Producto digital",
            "Producto digital para pruebas.",
            new Sku("ITEM-001"),
            new Money(20m, "COP"),
            5000,
            "producto-digital",
            null,
            null,
            null,
            null,
            "PDF",
            5m,
            false);

        producto.Activar();
        return producto;
    }
}