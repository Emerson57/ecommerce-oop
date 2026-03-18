using PlataformaECommerce.Domain.Entities.Cart;
using PlataformaECommerce.Domain.Entities.Products;
using PlataformaECommerce.Domain.Exceptions;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Tests.Domain.Cart;

[TestFixture]
public class CarritoCompraAggregateTests
{
    [Test]
    public void Constructor_ClienteValido_AsignaIdentidadYFechaCreacion()
    {
        CarritoCompra carrito = new(Guid.NewGuid());

        Assert.That(carrito.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(carrito.FechaCreacionUtc, Is.Not.EqualTo(default(DateTime)));
    }

    [Test]
    public void AgregarProducto_MonedaDistinta_LanzaCartException()
    {
        CarritoCompra carrito = new(Guid.NewGuid());
        ProductoDigital productoCop = CrearProductoDigital("SKU-COP", new Money(10m, "COP"));
        ProductoDigital productoUsd = CrearProductoDigital("SKU-USD", new Money(20m, "USD"));

        carrito.AgregarProducto(productoCop, 1);

        Assert.Throws<CartException>(() => carrito.AgregarProducto(productoUsd, 1));
    }

    private static ProductoDigital CrearProductoDigital(string sku, Money precio)
    {
        ProductoDigital producto = new(
            "Producto digital",
            "Producto digital de prueba.",
            new Sku(sku),
            precio,
            10,
            sku.ToLowerInvariant(),
            null,
            null,
            null,
            null,
            "PDF",
            10m,
            false);

        producto.Activar();
        return producto;
    }
}