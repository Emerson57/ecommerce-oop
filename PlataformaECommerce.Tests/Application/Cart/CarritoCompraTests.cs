using NUnit.Framework;
using PlataformaECommerce.Domain.Entities.Cart;
using PlataformaECommerce.Domain.Entities.Products;
using PlataformaECommerce.Domain.Exceptions;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Tests.Application.Cart;

[TestFixture]
public class CarritoCompraTests
{
    [Test]
    public void Constructor_ClienteValido_CreaCarritoActivoYVacio()
    {
        CarritoCompra carrito = new(Guid.NewGuid());

        Assert.That(carrito.Activo, Is.True);
        Assert.That(carrito.CantidadItems, Is.EqualTo(0));
        Assert.That(carrito.Total.Amount, Is.EqualTo(0m));
        Assert.That(carrito.Items, Is.Empty);
    }

    [Test]
    public void AgregarProducto_ProductoValido_AgregaCorrectamente()
    {
        CarritoCompra carrito = new(Guid.NewGuid());
        ProductoDigital producto = CrearProductoDigitalValido();

        carrito.AgregarProducto(producto, 1);

        Assert.That(carrito.CantidadItems, Is.EqualTo(1));
        Assert.That(carrito.Total.Amount, Is.EqualTo(producto.Precio.Amount));
        Assert.That(carrito.ContieneProducto(producto.Id), Is.True);
    }

    [Test]
    public void AgregarProducto_MismoProductoDosVeces_IncrementaCantidad()
    {
        CarritoCompra carrito = new(Guid.NewGuid());
        ProductoDigital producto = CrearProductoDigitalValido();

        carrito.AgregarProducto(producto, 1);
        carrito.AgregarProducto(producto, 2);

        Assert.That(carrito.ObtenerCantidadDeProducto(producto.Id), Is.EqualTo(3));
    }

    [Test]
    public void RemoverProducto_ProductoExistente_RemueveCorrectamente()
    {
        CarritoCompra carrito = new(Guid.NewGuid());
        ProductoDigital producto = CrearProductoDigitalValido();
        carrito.AgregarProducto(producto, 1);

        bool resultado = carrito.RemoverProducto(producto.Id);

        Assert.That(resultado, Is.True);
        Assert.That(carrito.CantidadItems, Is.EqualTo(0));
    }

    [Test]
    public void VaciarCarrito_CarritoVacio_LanzaCarritoVacioException()
    {
        CarritoCompra carrito = new(Guid.NewGuid());

        Assert.Throws<CarritoVacioException>(() => carrito.VaciarCarrito());
    }

    [Test]
    public void Desactivar_CarritoActivo_CambiaEstadoAInactivo()
    {
        CarritoCompra carrito = new(Guid.NewGuid());

        carrito.Desactivar();

        Assert.That(carrito.Activo, Is.False);
    }

    private static ProductoDigital CrearProductoDigitalValido()
    {
        ProductoDigital producto = new(
            "Ebook C#",
            "Material digital de aprendizaje.",
            new Sku("EBOOK-001"),
            new Money(50000m, "COP"),
            10,
            "ebook-csharp",
            null,
            null,
            null,
            null,
            "PDF",
            10.5m,
            false);

        producto.Activar();
        return producto;
    }
}