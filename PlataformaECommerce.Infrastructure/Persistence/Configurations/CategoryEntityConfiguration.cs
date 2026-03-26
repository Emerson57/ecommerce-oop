using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlataformaECommerce.Infrastructure.Persistence.Entities;

namespace PlataformaECommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configura la persistencia relacional de <see cref="CategoryEntity"/>.
/// </summary>
public sealed class CategoryEntityConfiguration : IEntityTypeConfiguration<CategoryEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CategoryEntity> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(category => category.Id);

        builder.Property(category => category.Id)
            .ValueGeneratedNever();

        builder.Property(category => category.Nombre)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(category => category.Slug)
            .IsRequired()
            .HasMaxLength(140);

        builder.Property(category => category.Descripcion)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(category => category.Activa)
            .IsRequired();

        builder.Property(category => category.FechaCreacionUtc)
            .IsRequired();

        builder.Property(category => category.FechaActualizacionUtc)
            .IsRequired(false);

        builder.HasIndex(category => category.Slug)
            .IsUnique();

        builder.HasIndex(category => category.Nombre);
        builder.HasIndex(category => category.ParentCategoryId);

        builder.HasOne<CategoryEntity>()
            .WithMany()
            .HasForeignKey(category => category.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
