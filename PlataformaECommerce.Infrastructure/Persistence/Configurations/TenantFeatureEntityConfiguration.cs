using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlataformaECommerce.Infrastructure.Persistence.Entities;

namespace PlataformaECommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configura la persistencia relacional del catálogo de features SaaS.
/// </summary>
public sealed class TenantFeatureEntityConfiguration : IEntityTypeConfiguration<TenantFeatureEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<TenantFeatureEntity> builder)
    {
        builder.ToTable("TenantFeatures");

        builder.HasKey(feature => feature.FeatureId);

        builder.Property(feature => feature.FeatureId)
            .HasMaxLength(64);

        builder.Property(feature => feature.DisplayName)
            .IsRequired()
            .HasMaxLength(160);

        builder.Property(feature => feature.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(feature => feature.Category)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(feature => feature.Enabled)
            .IsRequired();

        builder.HasIndex(feature => feature.Enabled);
        builder.HasIndex(feature => feature.Category);
    }
}
