using Microsoft.EntityFrameworkCore;
using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.ValueObjects;
using PlataformaECommerce.Infrastructure.Persistence.Context;
using PlataformaECommerce.Infrastructure.Repositories.Users;

namespace PlataformaECommerce.Tests.Infrastructure.Users;

[TestFixture]
public class UserRepositoryTests
{
    [Test]
    public async Task GetCustomerByIdAsync_ClientePersistido_RetornaPreferenciasRehidratadas()
    {
        await using ECommerceDbContext context = CreateContext();
        UserRepository repository = new(context);
        Cliente customer = CreateCustomer();
        customer.AgregarPreferencia("tecnologia");

        await repository.AddAsync(customer);
        await context.SaveChangesAsync();

        Cliente? result = await repository.GetCustomerByIdAsync(customer.Id);

        Assert.That(result?.TienePreferencia("tecnologia"), Is.True);
    }

    [Test]
    public async Task GetCustomerByIdAsync_ClientePersistido_RetornaHistorialRehidratado()
    {
        await using ECommerceDbContext context = CreateContext();
        UserRepository repository = new(context);
        Cliente customer = CreateCustomer();
        Guid orderId = Guid.NewGuid();
        customer.RegistrarCompra(orderId);

        await repository.AddAsync(customer);
        await context.SaveChangesAsync();

        Cliente? result = await repository.GetCustomerByIdAsync(customer.Id);

        Assert.That(result?.TieneCompraRegistrada(orderId), Is.True);
    }

    [Test]
    public async Task GetAdministratorByIdAsync_AdministradorPersistido_RetornaAreaRehidratada()
    {
        await using ECommerceDbContext context = CreateContext();
        UserRepository repository = new(context);
        Administrador admin = new("Admin Principal", new Email("admin@plataforma.com"), "hash-admin-prueba-2026", "Inventario");

        await repository.AddAsync(admin);
        await context.SaveChangesAsync();

        Administrador? result = await repository.GetAdministratorByIdAsync(admin.Id);

        Assert.That(result?.Area, Is.EqualTo("Inventario"));
    }

    [Test]
    public async Task ExistsByEmailAsync_UsuarioPersistido_RetornaTrue()
    {
        await using ECommerceDbContext context = CreateContext();
        UserRepository repository = new(context);
        Cliente customer = CreateCustomer();

        await repository.AddAsync(customer);
        await context.SaveChangesAsync();

        bool result = await repository.ExistsByEmailAsync(customer.CorreoElectronico);

        Assert.That(result, Is.True);
    }

    private static ECommerceDbContext CreateContext()
    {
        DbContextOptions<ECommerceDbContext> options = new DbContextOptionsBuilder<ECommerceDbContext>()
            .UseInMemoryDatabase($"users-{Guid.NewGuid():N}")
            .Options;

        return new ECommerceDbContext(options);
    }

    private static Cliente CreateCustomer()
    {
        return new Cliente(
            "Cliente Persistencia",
            new Email($"cliente-{Guid.NewGuid():N}@plataforma.com"),
            "hash-cliente-prueba-2026");
    }
}
