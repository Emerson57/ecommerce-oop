using Microsoft.EntityFrameworkCore;
using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.Enums;
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

    [Test]
    public async Task AddAsync_AdministradorPersistido_ConservaCamposRelevantesDelAlta()
    {
        await using ECommerceDbContext context = CreateContext();
        UserRepository repository = new(context);
        Administrador administrator = new("Admin Persistencia", new Email("admin.persistencia@plataforma.com"), "hash-admin-prueba-2026", "Operaciones");
        administrator.ConfirmarCorreoElectronico();

        await repository.AddAsync(administrator);
        await context.SaveChangesAsync();

        var persistedUser = await context.Users.SingleAsync(user => user.Id == administrator.Id);

        Assert.Multiple(() =>
        {
            Assert.That(persistedUser.Nombre, Is.EqualTo("Admin Persistencia"));
            Assert.That(persistedUser.CorreoElectronico, Is.EqualTo("admin.persistencia@plataforma.com"));
            Assert.That(persistedUser.ContrasenaHash, Is.EqualTo("hash-admin-prueba-2026"));
            Assert.That(persistedUser.Rol, Is.EqualTo(RolUsuario.Administrador.ToString()));
            Assert.That(persistedUser.Activo, Is.True);
            Assert.That(persistedUser.CorreoConfirmado, Is.True);
            Assert.That(persistedUser.Area, Is.EqualTo("Operaciones"));
            Assert.That(persistedUser.FechaCreacionUtc, Is.Not.EqualTo(default(DateTime)));
            Assert.That(persistedUser.FechaActualizacionUtc, Is.Not.Null);
        });
    }

    [Test]
    public async Task AddAsync_SuperUsuarioPersistido_UsaTablaUsersConRolTextoControlado()
    {
        await using ECommerceDbContext context = CreateContext();
        UserRepository repository = new(context);
        Administrador superUser = new("Root", new Email("root-text@plataforma.com"), "hash-root-prueba-2026", "Plataforma", RolUsuario.SuperUsuario);

        await repository.AddAsync(superUser);
        await context.SaveChangesAsync();

        var persistedUser = await context.Users.SingleAsync(user => user.Id == superUser.Id);

        Assert.That(persistedUser.Rol, Is.EqualTo(RolUsuario.SuperUsuario.ToString()));
    }

    [Test]
    public async Task UpdateAsync_AdministradorActualizado_PersisteAreaActualizada()
    {
        await using ECommerceDbContext context = CreateContext();
        UserRepository repository = new(context);
        Administrador administrator = new("Admin Operaciones", new Email("admin-update-area@plataforma.com"), "hash-admin-prueba-2026", "Operaciones");

        await repository.AddAsync(administrator);
        await context.SaveChangesAsync();

        administrator.ActualizarArea("Finanzas");
        await repository.UpdateAsync(administrator);
        await context.SaveChangesAsync();

        Administrador? result = await repository.GetAdministratorByIdAsync(administrator.Id);

        Assert.That(result?.Area, Is.EqualTo("Finanzas"));
    }

    [Test]
    public async Task UpdateAsync_UsuarioConAccesoRegistrado_PersisteFechaUltimoAccesoUtc()
    {
        await using ECommerceDbContext context = CreateContext();
        UserRepository repository = new(context);
        Administrador administrator = new("Admin Acceso", new Email("admin-update-access@plataforma.com"), "hash-admin-prueba-2026", "Seguridad");

        await repository.AddAsync(administrator);
        await context.SaveChangesAsync();

        administrator.RegistrarAcceso();
        await repository.UpdateAsync(administrator);
        await context.SaveChangesAsync();

        Administrador? result = await repository.GetAdministratorByIdAsync(administrator.Id);

        Assert.That(result?.FechaUltimoAccesoUtc, Is.Not.Null);
    }

    [Test]
    public async Task GetAdministratorByIdAsync_SuperUsuarioPersistido_RetornaCuentaAdministrativa()
    {
        await using ECommerceDbContext context = CreateContext();
        UserRepository repository = new(context);
        Administrador superUser = new("Root Lookup", new Email("root-lookup@plataforma.com"), "hash-root-prueba-2026", "Plataforma", RolUsuario.SuperUsuario);

        await repository.AddAsync(superUser);
        await context.SaveChangesAsync();

        Administrador? result = await repository.GetAdministratorByIdAsync(superUser.Id);

        Assert.That(result?.EsSuperUsuario, Is.True);
    }

    [Test]
    public async Task GetAdministratorsAsync_SuperUsuarioPersistido_LoIncluyeComoCuentaAdministrativa()
    {
        await using ECommerceDbContext context = CreateContext();
        UserRepository repository = new(context);
        Administrador superUser = new("Root", new Email("root@plataforma.com"), "hash-root-prueba-2026", "Plataforma", RolUsuario.SuperUsuario);

        await repository.AddAsync(superUser);
        await context.SaveChangesAsync();

        IReadOnlyCollection<Administrador> result = await repository.GetAdministratorsAsync();

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.Single().EsSuperUsuario, Is.True);
    }

    [Test]
    public async Task ExistsByRoleAsync_SuperUsuarioPersistido_RetornaTrue()
    {
        await using ECommerceDbContext context = CreateContext();
        UserRepository repository = new(context);
        Administrador superUser = new("Root", new Email("root-2@plataforma.com"), "hash-root-prueba-2026", "Plataforma", RolUsuario.SuperUsuario);

        await repository.AddAsync(superUser);
        await context.SaveChangesAsync();

        bool result = await repository.ExistsByRoleAsync(RolUsuario.SuperUsuario);

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
