using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlataformaECommerce.Infrastructure.Persistence.Entities;

namespace PlataformaECommerce.Infrastructure.Persistence.Configurations
{
    public sealed class ProductEntityConfiguration : IEntityTypeConfiguration<ProductEntity>
    {
        /// Configura la entidad ProductEntity mediante Fluent API.
        public void Configure(EntityTypeBuilder<ProductEntity> builder)
        {
            #region Tabla

            builder.ToTable("Products");

            #endregion

            #region Clave primaria

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .ValueGeneratedNever();

            #endregion

            #region Propiedades comunes obligatorias

            builder.Property(p => p.Nombre)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(p => p.Descripcion)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(p => p.Precio)
                   .IsRequired()
                   .HasPrecision(18, 2);

            builder.Property(p => p.Stock)
                   .IsRequired();

            builder.Property(p => p.Activo)
                   .IsRequired()
                   .HasDefaultValue(true);

            builder.Property(p => p.TipoProducto)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(p => p.FechaCreacion)
                   .IsRequired();

            builder.Property(p => p.FechaActualizacion)
                   .IsRequired();

            #endregion

            #region Propiedades opcionales para productos digitales

            builder.Property(p => p.FormatoArchivo)
                   .HasMaxLength(20)
                   .IsRequired(false);

            builder.Property(p => p.TamanoMB)
                   .HasPrecision(18, 2)
                   .IsRequired(false);

            #endregion

            #region Propiedades opcionales para productos físicos

            builder.Property(p => p.PesoKg)
                   .HasPrecision(18, 2)
                   .IsRequired(false);

            builder.Property(p => p.AltoCm)
                   .HasPrecision(18, 2)
                   .IsRequired(false);

            builder.Property(p => p.AnchoCm)
                   .HasPrecision(18, 2)
                   .IsRequired(false);

            builder.Property(p => p.LargoCm)
                   .HasPrecision(18, 2)
                   .IsRequired(false);

            #endregion

            #region Índices

            /// Índice útil para consultas por tipo de producto.
            builder.HasIndex(p => p.TipoProducto);

            /// Índice útil para consultas por estado activo/inactivo.
            builder.HasIndex(p => p.Activo);

            /// Índice útil para búsquedas por nombre.
            builder.HasIndex(p => p.Nombre);

            #endregion
        }
    }
}