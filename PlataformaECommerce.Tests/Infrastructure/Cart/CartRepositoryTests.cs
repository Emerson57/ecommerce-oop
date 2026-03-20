using Microsoft.EntityFrameworkCore;
using PlataformaECommerce.Domain.Entities.Cart;
using PlataformaECommerce.Infrastructure.Persistence.Context;
using PlataformaECommerce.Infrastructure.Repositories.Cart;
using PlataformaECommerce.Infrastructure.Services.Products;

namespace PlataformaECommerce.Tests.Infrastructure.Cart;

[TestFixture]
public class CartRepositoryTests
{
    [Test]
    public async Task GetByIdAsync_CarritoPersistido_RetornaCantidadDelItemRehidratada()
    {
        await using ECommerceDbContext context = CreateContext();
        CartRepository repository = new(context);
        CarritoCompra cart = new(Guid.NewGuid());
        var product = FabricaEntidades.CrearProductoFisico("Teclado Pro", "Teclado mecánico profesional", 350000m, 10, 1.2m, 4m, 18m, 45m);
        product.Activar();
        cart.AgregarProducto(product, 2);

        await repository.AddAsync(cart);
        await context.SaveChangesAsync();

        CarritoCompra? result = await repository.GetByIdAsync(cart.Id);

        Assert.That(result?.ObtenerItemPorProductoId(product.Id)?.Cantidad, Is.EqualTo(2));
    }

    [Test]
    public async Task GetByCustomerIdAsync_CarritosActivoEInactivo_PriorizaCarritoActivo()
    {
        await using ECommerceDbContext context = CreateContext();
        CartRepository repository = new(context);
        Guid customerId = Guid.NewGuid();
        CarritoCompra inactiveCart = new(customerId);
        inactiveCart.Desactivar();
        CarritoCompra activeCart = new(customerId);

        await repository.AddAsync(inactiveCart);
        await repository.AddAsync(activeCart);
        await context.SaveChangesAsync();

        CarritoCompra? result = await repository.GetByCustomerIdAsync(customerId);

        Assert.That(result?.Id, Is.EqualTo(activeCart.Id));
    }

    [Test]
    public async Task GetAllByCustomerIdAsync_DosCarritosPersistidos_RetornaAmbos()
    {
        await using ECommerceDbContext context = CreateContext();
        CartRepository repository = new(context);
        Guid customerId = Guid.NewGuid();

        await repository.AddAsync(new CarritoCompra(customerId));
        await repository.AddAsync(new CarritoCompra(customerId));
        await context.SaveChangesAsync();

        IReadOnlyCollection<CarritoCompra> result = await repository.GetAllByCustomerIdAsync(customerId);

        Assert.That(result.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task ExistsByCustomerIdAsync_CarritoPersistido_RetornaTrue()
    {
        await using ECommerceDbContext context = CreateContext();
        CartRepository repository = new(context);
        Guid customerId = Guid.NewGuid();

        await repository.AddAsync(new CarritoCompra(customerId));
        await context.SaveChangesAsync();

        bool result = await repository.ExistsByCustomerIdAsync(customerId);

        Assert.That(result, Is.True);
    }

    private static ECommerceDbContext CreateContext()
    {
        DbContextOptions<ECommerceDbContext> options = new DbContextOptionsBuilder<ECommerceDbContext>()
            .UseInMemoryDatabase($"carts-{Guid.NewGuid():N}")
            .Options;

        return new ECommerceDbContext(options);
    }
}
