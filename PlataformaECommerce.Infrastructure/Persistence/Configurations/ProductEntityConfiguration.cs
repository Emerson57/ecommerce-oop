using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlataformaECommerce.Infrastructure.Persistence.Entities;

namespace PlataformaECommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configura la entidad persistente <see cref="ProductEntity"/> mediante Fluent API.
/// </summary>
public sealed class ProductEntityConfiguration : IEntityTypeConfiguration<ProductEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProductEntity> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(product => product.Id);
        builder.Property(product => product.Id)
            .ValueGeneratedNever();

        builder.Property(product => product.Nombre)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(product => product.Descripcion)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(product => product.Sku)
            .IsRequired()
            .HasMaxLength(40);

        builder.Property(product => product.Precio)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(product => product.PrecioBase)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(product => product.PrecioPromocionalActual)
            .HasPrecision(18, 2)
            .IsRequired(false);

        builder.Property(product => product.DescuentoPromocionalActual)
            .HasPrecision(5, 2)
            .IsRequired(false);

        builder.Property(product => product.Moneda)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(product => product.Stock)
            .IsRequired();

        builder.Property(product => product.Activo)
            .IsRequired();

        builder.Property(product => product.Destacado)
            .IsRequired();

        builder.Property(product => product.TipoProducto)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(product => product.Slug)
            .IsRequired()
            .HasMaxLength(160);

        builder.Property(product => product.ImagenPrincipalUrl)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(product => product.EtiquetasSerializadas)
            .HasMaxLength(4000)
            .IsRequired(false);

        builder.Property(product => product.FechaCreacionUtc)
            .IsRequired();

        builder.Property(product => product.FechaActualizacionUtc)
            .IsRequired(false);

        builder.Property(product => product.FormatoArchivo)
            .HasMaxLength(20)
            .IsRequired(false);

        builder.Property(product => product.TamanoMB)
            .HasPrecision(18, 2)
            .IsRequired(false);

        builder.Property(product => product.RequiereLicencia)
            .IsRequired(false);

        builder.Property(product => product.PesoKg)
            .HasPrecision(18, 3)
            .IsRequired(false);

        builder.Property(product => product.AltoCm)
            .HasPrecision(18, 2)
            .IsRequired(false);

        builder.Property(product => product.AnchoCm)
            .HasPrecision(18, 2)
            .IsRequired(false);

        builder.Property(product => product.LargoCm)
            .HasPrecision(18, 2)
            .IsRequired(false);

        builder.Property(product => product.RequiereEnvio)
            .IsRequired(false);

        builder.HasIndex(product => product.Sku)
            .IsUnique();

        builder.HasIndex(product => product.TipoProducto);
        builder.HasIndex(product => product.Activo);
        builder.HasIndex(product => product.Nombre);
        builder.HasIndex(product => product.CategoriaId);
    }
}