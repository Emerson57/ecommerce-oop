using PlataformaECommerce.Domain.Entities.Orders;
using PlataformaECommerce.Domain.Entities.Products;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.Rules;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Tests.Domain.Rules;

[TestFixture]
public class RulesTests
{
    [Test]
    public void ProductoDisponibleRule_ProductoActivoConStock_RetornaTrue()
    {
        ProductoDigital producto = new(
            "Curso C#",
            "Curso técnico.",
            new Sku("CURSO-002"),
            new Money(100m, "COP"),
            5,
            "curso-csharp",
            null,
            null,
            null,
            null,
            "MP4",
            50m,
            false);
        producto.Activar();

        Assert.That(ProductoDisponibleRule.IsSatisfiedBy(producto), Is.True);
    }

    [Test]
    public void StockDisponibleRule_StockSuficiente_RetornaTrue()
    {
        Assert.That(StockDisponibleRule.IsSatisfiedBy(10, 4), Is.True);
    }

    [Test]
    public void ProductoDisponibleRule_ProductoInactivo_RetornaFalse()
    {
        ProductoDigital producto = new(
            "Curso C#",
            "Curso técnico.",
            new Sku("CURSO-003"),
            new Money(100m, "COP"),
            5,
            "curso-csharp-inactivo",
            null,
            null,
            null,
            null,
            "MP4",
            50m,
            false);

        Assert.That(ProductoDisponibleRule.IsSatisfiedBy(producto), Is.False);
    }

    [Test]
    public void StockDisponibleRule_CantidadInvalida_RetornaFalse()
    {
        Assert.That(StockDisponibleRule.IsSatisfiedBy(10, 0), Is.False);
    }

    [Test]
    public void PedidoCancelableRule_PedidoEnviado_RetornaFalse()
    {
        Pedido pedido = new(Guid.NewGuid());
        DetallePedido detalle = new(
            pedido.Id,
            Guid.NewGuid(),
            "Producto físico",
            new Sku("PED-002"),
            TipoProducto.Fisico,
            null,
            new Money(50m, "COP"),
            1);

        pedido.AgregarDetalle(detalle);
        pedido.AsignarDireccionEnvio(new DireccionEnvio("Calle 123", "Bogotá", "Cundinamarca", "Colombia", "110111"));
        pedido.Confirmar();
        pedido.RegistrarPago();
        pedido.MarcarEnProceso();
        pedido.MarcarEnviado();

        Assert.That(PedidoCancelableRule.IsSatisfiedBy(pedido), Is.False);
    }
}
