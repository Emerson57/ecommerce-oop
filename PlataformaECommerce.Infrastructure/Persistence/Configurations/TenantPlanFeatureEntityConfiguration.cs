using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlataformaECommerce.Infrastructure.Persistence.Entities;

namespace PlataformaECommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configura la persistencia relacional de la asociación entre planes y features SaaS.
/// </summary>
public sealed class TenantPlanFeatureEntityConfiguration : IEntityTypeConfiguration<TenantPlanFeatureEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<TenantPlanFeatureEntity> builder)
    {
        builder.ToTable("TenantPlanFeatures");

        builder.HasKey(planFeature => new { planFeature.PlanId, planFeature.FeatureId });

        builder.Property(planFeature => planFeature.PlanId)
            .HasMaxLength(64);

        builder.Property(planFeature => planFeature.FeatureId)
            .HasMaxLength(64);

        builder.HasOne(planFeature => planFeature.Plan)
            .WithMany(plan => plan.PlanFeatures)
            .HasForeignKey(planFeature => planFeature.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(planFeature => planFeature.Feature)
            .WithMany(feature => feature.PlanFeatures)
            .HasForeignKey(planFeature => planFeature.FeatureId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(planFeature => planFeature.FeatureId);
    }
}
