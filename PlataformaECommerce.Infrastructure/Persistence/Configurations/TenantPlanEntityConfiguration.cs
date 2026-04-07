using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlataformaECommerce.Infrastructure.Persistence.Entities;

namespace PlataformaECommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configura la persistencia relacional del catálogo de planes SaaS.
/// </summary>
public sealed class TenantPlanEntityConfiguration : IEntityTypeConfiguration<TenantPlanEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<TenantPlanEntity> builder)
    {
        builder.ToTable("TenantPlans");

        builder.HasKey(plan => plan.PlanId);

        builder.Property(plan => plan.PlanId)
            .HasMaxLength(64);

        builder.Property(plan => plan.DisplayName)
            .IsRequired()
            .HasMaxLength(160);

        builder.Property(plan => plan.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(plan => plan.MonthlyPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(plan => plan.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(plan => plan.IncludedAdministrators)
            .IsRequired();

        builder.Property(plan => plan.IncludedProducts)
            .IsRequired();

        builder.Property(plan => plan.Enabled)
            .IsRequired();

        builder.HasIndex(plan => plan.Enabled);
        builder.HasIndex(plan => plan.DisplayName);
    }
}
