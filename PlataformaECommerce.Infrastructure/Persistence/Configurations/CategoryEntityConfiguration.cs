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

        builder.Property(category => category.TenantId)
            .IsRequired()
            .HasMaxLength(64);

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

        builder.HasAlternateKey(category => new { category.TenantId, category.Id });

        builder.HasIndex(category => new { category.TenantId, category.Slug })
            .IsUnique();

        builder.HasIndex(category => category.TenantId);
        builder.HasIndex(category => category.Nombre);
        builder.HasIndex(category => new { category.TenantId, category.ParentCategoryId });

        builder.HasOne<CategoryEntity>()
            .WithMany()
            .HasForeignKey(category => new { category.TenantId, category.ParentCategoryId })
            .HasPrincipalKey(category => new { category.TenantId, category.Id })
            .OnDelete(DeleteBehavior.Restrict);
    }
}
