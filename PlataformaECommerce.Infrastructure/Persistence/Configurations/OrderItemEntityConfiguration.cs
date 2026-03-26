using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlataformaECommerce.Infrastructure.Persistence.Entities;

namespace PlataformaECommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configura la entidad persistente <see cref="OrderItemEntity"/> mediante Fluent API.
/// </summary>
/// <remarks>
/// Esta configuración define la estructura relacional de las líneas del pedido,
/// asegurando restricciones y longitudes coherentes con la instantánea comercial
/// que el agregado necesita preservar a lo largo del tiempo.
/// </remarks>
public sealed class OrderItemEntityConfiguration : IEntityTypeConfiguration<OrderItemEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<OrderItemEntity> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(detail => detail.Id);

        builder.Property(detail => detail.Id)
            .ValueGeneratedNever();

        builder.Property(detail => detail.PedidoId)
            .IsRequired();

        builder.Property(detail => detail.ProductoId)
            .IsRequired();

        builder.Property(detail => detail.NombreProducto)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(detail => detail.SkuProducto)
            .IsRequired()
            .HasMaxLength(40);

        builder.Property(detail => detail.TipoProducto)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(detail => detail.ImagenPrincipalUrl)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(detail => detail.PrecioUnitario)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(detail => detail.Moneda)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(detail => detail.Cantidad)
            .IsRequired();

        builder.Property(detail => detail.FechaCreacionUtc)
            .IsRequired();

        builder.HasIndex(detail => detail.PedidoId);
        builder.HasIndex(detail => detail.ProductoId);
        builder.HasIndex(detail => new { detail.PedidoId, detail.ProductoId });
    }
}
