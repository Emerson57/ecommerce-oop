using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlataformaECommerce.Application.Interfaces.Services.Common;
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
    private const string DefaultTenantId = "platform-default";
    private readonly ITenantContextAccessor? _tenantContextAccessor;

    #region Constructor

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="ECommerceDbContext"/>.
    /// </summary>
    /// <param name="options">Opciones de configuración del contexto.</param>
    public ECommerceDbContext(DbContextOptions<ECommerceDbContext> options, ITenantContextAccessor? tenantContextAccessor = null)
        : base(options)
    {
        _tenantContextAccessor = tenantContextAccessor;
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
    /// Representa la colección persistente de tenants SaaS configurados para la plataforma.
    /// </summary>
    public DbSet<TenantEntity> Tenants { get; set; } = null!;

    /// <summary>
    /// Representa la colección persistente de hostnames asociados a tenants.
    /// </summary>
    public DbSet<TenantHostnameEntity> TenantHostnames { get; set; } = null!;

    /// <summary>
    /// Representa la colección persistente del catálogo comercial de planes SaaS.
    /// </summary>
    public DbSet<TenantPlanEntity> TenantPlans { get; set; } = null!;

    /// <summary>
    /// Representa la colección persistente del catálogo de features SaaS.
    /// </summary>
    public DbSet<TenantFeatureEntity> TenantFeatures { get; set; } = null!;

    /// <summary>
    /// Representa la colección persistente de asociaciones entre planes y features SaaS.
    /// </summary>
    public DbSet<TenantPlanFeatureEntity> TenantPlanFeatures { get; set; } = null!;

    /// <summary>
    /// Representa la colección persistente de asignaciones explícitas de features por tenant.
    /// </summary>
    public DbSet<TenantFeatureAssignmentEntity> TenantFeatureAssignments { get; set; } = null!;

    /// <summary>
    /// Representa la colección persistente de suscripciones efectivas por tenant.
    /// </summary>
    public DbSet<TenantSubscriptionEntity> TenantSubscriptions { get; set; } = null!;

    /// <summary>
    /// Representa la colección persistente del estado de aprovisionamiento inicial por tenant.
    /// </summary>
    public DbSet<TenantProvisioningEntity> TenantProvisionings { get; set; } = null!;

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

    internal string CurrentTenantId => _tenantContextAccessor?.IsAvailable == true
        ? _tenantContextAccessor.TenantId
        : DefaultTenantId;

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

        modelBuilder.Entity<UserEntity>().HasQueryFilter(entity => entity.TenantId == CurrentTenantId);
        modelBuilder.Entity<ProductEntity>().HasQueryFilter(entity => entity.TenantId == CurrentTenantId);
        modelBuilder.Entity<CategoryEntity>().HasQueryFilter(entity => entity.TenantId == CurrentTenantId);
        modelBuilder.Entity<CartEntity>().HasQueryFilter(entity => entity.TenantId == CurrentTenantId);
        modelBuilder.Entity<CartItemEntity>().HasQueryFilter(entity => entity.TenantId == CurrentTenantId);
        modelBuilder.Entity<OrderEntity>().HasQueryFilter(entity => entity.TenantId == CurrentTenantId);
        modelBuilder.Entity<OrderItemEntity>().HasQueryFilter(entity => entity.TenantId == CurrentTenantId);
    }

    /// <inheritdoc />
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyTenantIsolation();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    /// <inheritdoc />
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyTenantIsolation();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyTenantIsolation()
    {
        string tenantId = CurrentTenantId;

        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry in ChangeTracker.Entries())
        {
            if (entry.Entity is not ITenantOwnedEntity tenantOwnedEntity)
            {
                continue;
            }

            if (entry.State == EntityState.Added)
            {
                tenantOwnedEntity.TenantId = tenantId;
                continue;
            }

            if (entry.State is EntityState.Modified or EntityState.Unchanged or EntityState.Deleted)
            {
                if (string.IsNullOrWhiteSpace(tenantOwnedEntity.TenantId))
                {
                    throw new InvalidOperationException("No se puede persistir una entidad aislada por tenant sin identificador de tenant asignado.");
                }

                if (!string.Equals(tenantOwnedEntity.TenantId, tenantId, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"Se intentó persistir una entidad del tenant '{tenantOwnedEntity.TenantId}' dentro del contexto activo '{tenantId}'.");
                }

                if (entry.State is EntityState.Modified or EntityState.Unchanged)
                {
                    entry.Property(nameof(ITenantOwnedEntity.TenantId)).IsModified = false;
                }
            }
        }
    }

    #endregion
}