using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlataformaECommerce.Infrastructure.Persistence.Entities;

namespace PlataformaECommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configura la entidad persistente <see cref="OrderEntity"/> mediante Fluent API.
/// </summary>
/// <remarks>
/// Esta configuración define la estructura relacional del pedido, sus restricciones
/// de integridad, sus índices principales y la relación de composición con los
/// detalles persistentes del agregado.
/// </remarks>
public sealed class OrderEntityConfiguration : IEntityTypeConfiguration<OrderEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<OrderEntity> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(order => order.Id);

        builder.Property(order => order.Id)
            .ValueGeneratedNever();

        builder.Property(order => order.ClienteId)
            .IsRequired();

        builder.Property(order => order.Estado)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(order => order.FechaCreacionUtc)
            .IsRequired();

        builder.Property(order => order.FechaActualizacionUtc)
            .IsRequired(false);

        builder.Property(order => order.FechaConfirmacionUtc)
            .IsRequired(false);

        builder.Property(order => order.FechaPagoUtc)
            .IsRequired(false);

        builder.Property(order => order.FechaEnvioUtc)
            .IsRequired(false);

        builder.Property(order => order.FechaEntregaUtc)
            .IsRequired(false);

        builder.Property(order => order.FechaCancelacionUtc)
            .IsRequired(false);

        builder.Property(order => order.ObservacionCancelacion)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(order => order.DireccionCalle)
            .HasMaxLength(150)
            .IsRequired(false);

        builder.Property(order => order.DireccionCiudad)
            .HasMaxLength(150)
            .IsRequired(false);

        builder.Property(order => order.DireccionDepartamento)
            .HasMaxLength(150)
            .IsRequired(false);

        builder.Property(order => order.DireccionPais)
            .HasMaxLength(150)
            .IsRequired(false);

        builder.Property(order => order.DireccionCodigoPostal)
            .HasMaxLength(150)
            .IsRequired(false);

        builder.HasMany(order => order.Detalles)
            .WithOne(detail => detail.Pedido)
            .HasForeignKey(detail => detail.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(order => order.ClienteId);
        builder.HasIndex(order => order.Estado);
        builder.HasIndex(order => order.FechaCreacionUtc);
        builder.HasIndex(order => new { order.ClienteId, order.Estado });
    }
}
