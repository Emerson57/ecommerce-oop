using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlataformaECommerce.Infrastructure.Persistence.Entities;

namespace PlataformaECommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configura la entidad persistente <see cref="UserEntity"/> mediante Fluent API.
/// </summary>
/// <remarks>
/// Esta configuración define las restricciones, longitudes, índices y convenciones
/// necesarias para almacenar usuarios de forma consistente en la base de datos
/// transaccional principal del sistema.
/// </remarks>
public sealed class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .ValueGeneratedNever();

        builder.Property(user => user.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(user => user.CorreoElectronico)
            .IsRequired()
            .HasMaxLength(320);

        builder.Property(user => user.ContrasenaHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(user => user.Rol)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(user => user.Activo)
            .IsRequired();

        builder.Property(user => user.CorreoConfirmado)
            .IsRequired();

        builder.Property(user => user.FechaCreacionUtc)
            .IsRequired();

        builder.Property(user => user.FechaActualizacionUtc)
            .IsRequired(false);

        builder.Property(user => user.FechaUltimoAccesoUtc)
            .IsRequired(false);

        builder.Property(user => user.Area)
            .HasMaxLength(60)
            .IsRequired(false);

        builder.Property(user => user.HistorialComprasSerializado)
            .IsRequired(false);

        builder.Property(user => user.PreferenciasSerializadas)
            .IsRequired(false);

        builder.HasIndex(user => user.CorreoElectronico)
            .IsUnique();

        builder.HasIndex(user => user.Rol);
        builder.HasIndex(user => user.Activo);
    }
}
