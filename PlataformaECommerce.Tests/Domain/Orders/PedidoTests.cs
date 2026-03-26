using PlataformaECommerce.Domain.Entities.Orders;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.Exceptions;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Tests.Domain.Orders;

[TestFixture]
public class PedidoTests
{
    [Test]
    public void Confirmar_ConDetalle_ActualizaEstadoAConfirmado()
    {
        Pedido pedido = CrearPedidoConDetalle();

        pedido.Confirmar();

        Assert.That(pedido.Estado, Is.EqualTo(EstadoPedido.Confirmado));
    }

    [Test]
    public void Constructor_AlCrearPedido_RegistraEventoDeCreacion()
    {
        Pedido pedido = new(Guid.NewGuid());

        Assert.That(pedido.DomainEvents.OfType<PlataformaECommerce.Domain.Events.PedidoCreadoEvent>().ToArray(), Has.Length.EqualTo(1));
    }

    [Test]
    public void AsignarDireccionEnvio_DireccionValida_LaAsociaAlPedido()
    {
        Pedido pedido = CrearPedidoConDetalle();
        DireccionEnvio direccionEnvio = new("Calle 123", "Bogotá", "Cundinamarca", "Colombia", "110111");

        pedido.AsignarDireccionEnvio(direccionEnvio);

        Assert.That(pedido.DireccionEnvio, Is.EqualTo(direccionEnvio));
    }

    [Test]
    public void ContieneProductosFisicos_ConDetalleFisico_RetornaTrue()
    {
        Pedido pedido = CrearPedidoConDetalle(TipoProducto.Fisico);

        Assert.That(pedido.ContieneProductosFisicos(), Is.True);
    }

    [Test]
    public void ContieneProductosDigitales_ConDetalleDigital_RetornaTrue()
    {
        Pedido pedido = CrearPedidoConDetalle(TipoProducto.Digital);

        Assert.That(pedido.ContieneProductosDigitales(), Is.True);
    }

    [Test]
    public void MarcarEnviado_DesdePendiente_LanzaDomainException()
    {
        Pedido pedido = CrearPedidoConDetalle();

        Assert.Throws<DomainException>(() => pedido.MarcarEnviado());
    }

    [Test]
    public void MarcarEnProceso_DesdeConfirmado_LanzaDomainException()
    {
        Pedido pedido = CrearPedidoConDetalle();
        pedido.Confirmar();

        Assert.Throws<DomainException>(() => pedido.MarcarEnProceso());
    }

    [Test]
    public void MarcarEnviado_ConProductoFisicoSinDireccion_LanzaDomainException()
    {
        Pedido pedido = CrearPedidoConDetalle(TipoProducto.Fisico);
        pedido.Confirmar();
        pedido.RegistrarPago();
        pedido.MarcarEnProceso();

        Assert.Throws<DomainException>(() => pedido.MarcarEnviado());
    }

    [Test]
    public void Cancelar_DesdeEnviado_LanzaDomainException()
    {
        Pedido pedido = CrearPedidoConDetalle(TipoProducto.Fisico);
        pedido.AsignarDireccionEnvio(new DireccionEnvio("Calle 123", "Bogotá", "Cundinamarca", "Colombia", "110111"));
        pedido.Confirmar();
        pedido.RegistrarPago();
        pedido.MarcarEnProceso();
        pedido.MarcarEnviado();

        Assert.Throws<DomainException>(() => pedido.Cancelar("Cancelación tardía"));
    }

    [Test]
    public void FlujoCompleto_ConDireccionYEstadosValidos_LlegaAEntregado()
    {
        Pedido pedido = CrearPedidoConDetalle(TipoProducto.Fisico);
        pedido.AsignarDireccionEnvio(new DireccionEnvio("Calle 123", "Bogotá", "Cundinamarca", "Colombia", "110111"));

        pedido.Confirmar();
        pedido.RegistrarPago();
        pedido.MarcarEnProceso();
        pedido.MarcarEnviado();
        pedido.MarcarEntregado();

        Assert.That(pedido.Estado, Is.EqualTo(EstadoPedido.Entregado));
    }

    [Test]
    public void RegistrarPago_DesdeConfirmado_RegistraEventoDePago()
    {
        Pedido pedido = CrearPedidoConDetalle();
        pedido.ClearDomainEvents();
        pedido.Confirmar();

        pedido.RegistrarPago();

        Assert.That(pedido.DomainEvents.OfType<PlataformaECommerce.Domain.Events.PedidoPagadoEvent>().ToArray(), Has.Length.EqualTo(1));
    }

    [Test]
    public void Cancelar_DesdePagado_RegistraEventoDeCancelacion()
    {
        Pedido pedido = CrearPedidoConDetalle();
        pedido.ClearDomainEvents();
        pedido.Confirmar();
        pedido.RegistrarPago();

        pedido.Cancelar("Solicitud del cliente");

        Assert.That(pedido.DomainEvents.OfType<PlataformaECommerce.Domain.Events.PedidoCanceladoEvent>().ToArray(), Has.Length.EqualTo(1));
    }

    [Test]
    public void AgregarDetalle_ConMonedaDistinta_LanzaDomainException()
    {
        Pedido pedido = CrearPedidoConDetalle();
        DetallePedido detalleUsd = new(
            pedido.Id,
            Guid.NewGuid(),
            "Producto USD",
            new Sku("PED-USD"),
            TipoProducto.Digital,
            null,
            new Money(10m, "USD"),
            1);

        Assert.Throws<DomainException>(() => pedido.AgregarDetalle(detalleUsd));
    }

    private static Pedido CrearPedidoConDetalle(TipoProducto tipoProducto = TipoProducto.Fisico)
    {
        Pedido pedido = new(Guid.NewGuid());
        DetallePedido detalle = new(
            pedido.Id,
            Guid.NewGuid(),
            "Producto de prueba",
            new Sku("PED-001"),
            tipoProducto,
            null,
            new Money(100m, "COP"),
            2);

        pedido.AgregarDetalle(detalle);
        return pedido;
    }
}
