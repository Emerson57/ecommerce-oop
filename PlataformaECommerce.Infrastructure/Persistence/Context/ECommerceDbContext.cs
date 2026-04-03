using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlataformaECommerce.Infrastructure.Persistence.Entities;

namespace PlataformaECommerce.Infrastructure.Persistence.Context;

/// <summary>
/// Representa el contexto transaccional principal del sistema sobre Entity Framework Core.
/// </summary>
/// <remarks>
/// Este contexto centraliza la configuración de las proyecciones persistentes respaldadas
/// por SQL Server y actúa como frontera técnica para repositorios, unidad de trabajo y
/// demás componentes de infraestructura que operan sobre la base de datos relacional.
/// </remarks>
public sealed class ECommerceDbContext : DbContext, IDataProtectionKeyContext
{
    #region Constructor

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="ECommerceDbContext"/>.
    /// </summary>
    /// <param name="options">Opciones de configuración del contexto.</param>
    public ECommerceDbContext(DbContextOptions<ECommerceDbContext> options)
        : base(options)
    {
    }

    #endregion

    #region DbSets

    /// <summary>
    /// Representa la colección persistente de claves de Data Protection compartidas por la aplicación.
    /// </summary>
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

    /// <summary>
    /// Representa la colección persistente de categorías del sistema.
    /// </summary>
    public DbSet<CategoryEntity> Categories { get; set; } = null!;

    /// <summary>
    /// Representa la colección persistente de productos del sistema.
    /// </summary>
    public DbSet<ProductEntity> Products { get; set; } = null!;

    /// <summary>
    /// Representa la colección persistente de usuarios del sistema.
    /// </summary>
    public DbSet<UserEntity> Users { get; set; } = null!;

    /// <summary>
    /// Representa la colección persistente de carritos del sistema.
    /// </summary>
    public DbSet<CartEntity> Carts { get; set; } = null!;

    /// <summary>
    /// Representa la colección persistente de ítems de carrito del sistema.
    /// </summary>
    public DbSet<CartItemEntity> CartItems { get; set; } = null!;

    /// <summary>
    /// Representa la colección persistente de pedidos del sistema.
    /// </summary>
    public DbSet<OrderEntity> Orders { get; set; } = null!;

    /// <summary>
    /// Representa la colección persistente de detalles de pedido del sistema.
    /// </summary>
    public DbSet<OrderItemEntity> OrderItems { get; set; } = null!;

    #endregion

    #region Configuración del modelo

    /// <summary>
    /// Aplica la configuración del modelo utilizando Fluent API.
    /// </summary>
    /// <param name="modelBuilder">Constructor del modelo relacional.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ECommerceDbContext).Assembly);
    }

    #endregion
}