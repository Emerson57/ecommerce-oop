using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.ValueObjects;
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
        builder.ToTable("Users", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_Users_Rol",
                "[Rol] IN ('Cliente', 'Administrador', 'SuperUsuario')");

            tableBuilder.HasCheckConstraint(
                "CK_Users_CoreText",
                $"LEN(LTRIM(RTRIM([Nombre]))) BETWEEN {Usuario.LongitudMinimaNombre} AND {Usuario.LongitudMaximaNombre} AND LEN(LTRIM(RTRIM([CorreoElectronico]))) BETWEEN 3 AND {Email.MaxLength} AND LEN(LTRIM(RTRIM([ContrasenaHash]))) BETWEEN {Usuario.LongitudMinimaHashContrasena} AND {Usuario.LongitudMaximaHashContrasena}");

            tableBuilder.HasCheckConstraint(
                "CK_Users_Area_ByRole",
                $"([Rol] = 'Cliente' AND [Area] IS NULL) OR ([Rol] IN ('Administrador', 'SuperUsuario') AND [Area] IS NOT NULL AND LEN(LTRIM(RTRIM([Area]))) BETWEEN {Administrador.LongitudMinimaArea} AND {Administrador.LongitudMaximaArea})");
        });

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .ValueGeneratedNever();

        builder.Property(user => user.Nombre)
            .IsRequired()
            .HasMaxLength(Usuario.LongitudMaximaNombre);

        builder.Property(user => user.CorreoElectronico)
            .IsRequired()
            .HasMaxLength(Email.MaxLength);

        builder.Property(user => user.ContrasenaHash)
            .IsRequired()
            .HasMaxLength(Usuario.LongitudMaximaHashContrasena);

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
            .HasMaxLength(Administrador.LongitudMaximaArea)
            .IsRequired(false);

        builder.Property(user => user.HistorialComprasSerializado)
            .IsRequired(false);

        builder.Property(user => user.PreferenciasSerializadas)
            .IsRequired(false);

        builder.HasIndex(user => user.CorreoElectronico)
            .IsUnique();

        builder.HasIndex(user => user.Rol);
        builder.HasIndex(user => user.Activo);
        builder.HasIndex(user => new { user.Rol, user.Activo });
    }
}
