using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlataformaECommerce.Infrastructure.Persistence.Entities;

namespace PlataformaECommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configura la entidad persistente <see cref="CartItemEntity"/> mediante Fluent API.
/// </summary>
/// <remarks>
/// Esta configuración define la estructura relacional de las líneas de carrito,
/// asegurando longitudes, tipos y restricciones coherentes con la instantánea comercial
/// que el agregado necesita preservar.
/// </remarks>
public sealed class CartItemEntityConfiguration : IEntityTypeConfiguration<CartItemEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CartItemEntity> builder)
    {
        builder.ToTable("CartItems");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id)
            .ValueGeneratedNever();

        builder.Property(item => item.TenantId)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(item => item.CartId)
            .IsRequired();

        builder.Property(item => item.ProductoId)
            .IsRequired();

        builder.Property(item => item.NombreProducto)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(item => item.SkuProducto)
            .IsRequired()
            .HasMaxLength(40);

        builder.Property(item => item.TipoProducto)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(item => item.ImagenPrincipalUrl)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(item => item.PrecioUnitario)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(item => item.Moneda)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(item => item.Cantidad)
            .IsRequired();

        builder.Property(item => item.FechaCreacionUtc)
            .IsRequired();

        builder.Property(item => item.FechaActualizacionUtc)
            .IsRequired(false);

        builder.HasIndex(item => item.TenantId);
        builder.HasIndex(item => item.CartId);
        builder.HasIndex(item => item.ProductoId);
        builder.HasIndex(item => new { item.TenantId, item.CartId, item.ProductoId });
    }
}
