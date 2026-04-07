using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlataformaECommerce.Infrastructure.Persistence.Entities;

namespace PlataformaECommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configura la persistencia relacional de la suscripción efectiva por tenant.
/// </summary>
public sealed class TenantSubscriptionEntityConfiguration : IEntityTypeConfiguration<TenantSubscriptionEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<TenantSubscriptionEntity> builder)
    {
        builder.ToTable("TenantSubscriptions");

        builder.HasKey(subscription => subscription.TenantId);

        builder.Property(subscription => subscription.TenantId)
            .HasMaxLength(64);

        builder.Property(subscription => subscription.PlanId)
            .HasMaxLength(64)
            .IsRequired(false);

        builder.Property(subscription => subscription.Status)
            .IsRequired()
            .HasMaxLength(40);

        builder.Property(subscription => subscription.StartedAtUtc)
            .IsRequired(false);

        builder.Property(subscription => subscription.TrialEndsAtUtc)
            .IsRequired(false);

        builder.Property(subscription => subscription.RenewalAtUtc)
            .IsRequired(false);

        builder.Property(subscription => subscription.AutoRenew)
            .IsRequired();

        builder.Property(subscription => subscription.SeatsPurchased)
            .IsRequired();

        builder.HasOne(subscription => subscription.Tenant)
            .WithOne(tenant => tenant.Subscription)
            .HasForeignKey<TenantSubscriptionEntity>(subscription => subscription.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(subscription => subscription.Plan)
            .WithMany()
            .HasForeignKey(subscription => subscription.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(subscription => subscription.PlanId);
        builder.HasIndex(subscription => subscription.Status);
    }
}
