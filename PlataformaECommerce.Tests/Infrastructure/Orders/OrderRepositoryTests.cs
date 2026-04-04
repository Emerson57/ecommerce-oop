using Microsoft.EntityFrameworkCore;
using PlataformaECommerce.Domain.Entities.Cart;
using PlataformaECommerce.Domain.Entities.Orders;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.ValueObjects;
using PlataformaECommerce.Infrastructure.Persistence.Context;
using PlataformaECommerce.Infrastructure.Repositories.Orders;
using PlataformaECommerce.Infrastructure.Services.Products;

namespace PlataformaECommerce.Tests.Infrastructure.Orders;

[TestFixture]
public class OrderRepositoryTests
{
    [Test]
    public async Task GetByIdAsync_PedidoPersistido_RetornaEstadoRehidratado()
    {
        await using ECommerceDbContext context = CreateContext();
        OrderRepository repository = new(context);
        (Pedido order, _) = CreateOrderWithPhysicalItem(Guid.NewGuid());
        order.Confirmar();

        await repository.AddAsync(order);
        await context.SaveChangesAsync();

        Pedido? result = await repository.GetByIdAsync(order.Id);

        Assert.That(result?.Estado, Is.EqualTo(EstadoPedido.Confirmado));
    }

    [Test]
    public async Task GetByIdAsync_PedidoPersistido_RetornaDireccionEnvioRehidratada()
    {
        await using ECommerceDbContext context = CreateContext();
        OrderRepository repository = new(context);
        (Pedido order, _) = CreateOrderWithPhysicalItem(Guid.NewGuid());
        order.AsignarDireccionEnvio(new DireccionEnvio("Calle 123", "Bogotá", "Cundinamarca", "Colombia", "110111"));

        await repository.AddAsync(order);
        await context.SaveChangesAsync();

        Pedido? result = await repository.GetByIdAsync(order.Id);

        Assert.That(result?.TieneDireccionEnvio(), Is.True);
    }

    [Test]
    public async Task GetByIdAsync_PedidoPersistido_RetornaMetodoPagoRehidratado()
    {
        await using ECommerceDbContext context = CreateContext();
        OrderRepository repository = new(context);
        (Pedido order, _) = CreateOrderWithPhysicalItem(Guid.NewGuid());
        order.SeleccionarMetodoPago(MetodoPagoPedido.Tarjeta);

        await repository.AddAsync(order);
        await context.SaveChangesAsync();

        Pedido? result = await repository.GetByIdAsync(order.Id);

        Assert.That(result?.MetodoPagoSeleccionado, Is.EqualTo(MetodoPagoPedido.Tarjeta));
    }

    [Test]
    public async Task GetByIdAsync_PedidoPersistido_RetornaDetalleRehidratado()
    {
        await using ECommerceDbContext context = CreateContext();
        OrderRepository repository = new(context);
        (Pedido order, Guid productId) = CreateOrderWithPhysicalItem(Guid.NewGuid());

        await repository.AddAsync(order);
        await context.SaveChangesAsync();

        Pedido? result = await repository.GetByIdAsync(order.Id);

        Assert.That(result?.ObtenerDetallePorProductoId(productId)?.Cantidad, Is.EqualTo(2));
    }

    [Test]
    public async Task GetByCustomerIdAndStatusAsync_PedidosPersistidos_RetornaSoloCoincidencias()
    {
        await using ECommerceDbContext context = CreateContext();
        OrderRepository repository = new(context);
        Guid customerId = Guid.NewGuid();
        (Pedido confirmedOrder, _) = CreateOrderWithPhysicalItem(customerId);
        (Pedido pendingOrder, _) = CreateOrderWithPhysicalItem(customerId);
        confirmedOrder.Confirmar();

        await repository.AddAsync(confirmedOrder);
        await repository.AddAsync(pendingOrder);
        await context.SaveChangesAsync();

        IReadOnlyCollection<Pedido> result = await repository.GetByCustomerIdAndStatusAsync(customerId, EstadoPedido.Confirmado);

        Assert.That(result.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task ExistsByCustomerIdAsync_PedidoPersistido_RetornaTrue()
    {
        await using ECommerceDbContext context = CreateContext();
        OrderRepository repository = new(context);
        (Pedido order, _) = CreateOrderWithPhysicalItem(Guid.NewGuid());

        await repository.AddAsync(order);
        await context.SaveChangesAsync();

        bool result = await repository.ExistsByCustomerIdAsync(order.ClienteId);

        Assert.That(result, Is.True);
    }

    private static ECommerceDbContext CreateContext()
    {
        DbContextOptions<ECommerceDbContext> options = new DbContextOptionsBuilder<ECommerceDbContext>()
            .UseInMemoryDatabase($"orders-{Guid.NewGuid():N}")
            .Options;

        return new ECommerceDbContext(options);
    }

    private static (Pedido Order, Guid ProductId) CreateOrderWithPhysicalItem(Guid customerId)
    {
        CarritoCompra cart = new(customerId);
        var product = FabricaEntidades.CrearProductoFisico(
            "Portátil Pro",
            "Portátil profesional para trabajo intensivo.",
            4500000m,
            10,
            1.8m,
            2m,
            35m,
            24m,
            sku: "PORTATIL-PRO-001");

        product.Activar();
        cart.AgregarProducto(product, 2);

        return (new Pedido(cart), product.Id);
    }
}
