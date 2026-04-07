using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlataformaECommerce.Infrastructure.Persistence.Entities;

namespace PlataformaECommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configura la persistencia relacional de los features habilitados por tenant.
/// </summary>
public sealed class TenantFeatureAssignmentEntityConfiguration : IEntityTypeConfiguration<TenantFeatureAssignmentEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<TenantFeatureAssignmentEntity> builder)
    {
        builder.ToTable("TenantFeatureAssignments");

        builder.HasKey(assignment => new { assignment.TenantId, assignment.FeatureId });

        builder.Property(assignment => assignment.TenantId)
            .HasMaxLength(64);

        builder.Property(assignment => assignment.FeatureId)
            .HasMaxLength(64);

        builder.HasOne(assignment => assignment.Tenant)
            .WithMany(tenant => tenant.FeatureAssignments)
            .HasForeignKey(assignment => assignment.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(assignment => assignment.Feature)
            .WithMany(feature => feature.TenantAssignments)
            .HasForeignKey(assignment => assignment.FeatureId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(assignment => assignment.FeatureId);
    }
}
