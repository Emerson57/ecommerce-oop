using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlataformaECommerce.Infrastructure.Persistence.Entities;

namespace PlataformaECommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configura la entidad persistente <see cref="CartEntity"/> mediante Fluent API.
/// </summary>
/// <remarks>
/// Esta configuración define la estructura relacional del encabezado del carrito,
/// sus restricciones de integridad y la relación de composición con los ítems
/// persistidos del agregado.
/// </remarks>
public sealed class CartEntityConfiguration : IEntityTypeConfiguration<CartEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CartEntity> builder)
    {
        builder.ToTable("Carts");

        builder.HasKey(cart => cart.Id);

        builder.HasAlternateKey(cart => new { cart.TenantId, cart.Id });

        builder.Property(cart => cart.Id)
            .ValueGeneratedNever();

        builder.Property(cart => cart.TenantId)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(cart => cart.ClienteId)
            .IsRequired();

        builder.Property(cart => cart.Activo)
            .IsRequired();

        builder.Property(cart => cart.FechaCreacionUtc)
            .IsRequired();

        builder.Property(cart => cart.FechaActualizacionUtc)
            .IsRequired(false);

        builder.HasMany(cart => cart.Items)
            .WithOne(item => item.Cart)
            .HasForeignKey(item => new { item.TenantId, item.CartId })
            .HasPrincipalKey(cart => new { cart.TenantId, cart.Id })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(cart => cart.TenantId);
        builder.HasIndex(cart => cart.ClienteId);
        builder.HasIndex(cart => cart.Activo);
        builder.HasIndex(cart => new { cart.TenantId, cart.ClienteId, cart.Activo });
    }
}
