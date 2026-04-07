using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlataformaECommerce.Infrastructure.Persistence.Entities;

namespace PlataformaECommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configura la persistencia relacional del estado de aprovisionamiento inicial por tenant.
/// </summary>
public sealed class TenantProvisioningEntityConfiguration : IEntityTypeConfiguration<TenantProvisioningEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<TenantProvisioningEntity> builder)
    {
        builder.ToTable("TenantProvisionings");

        builder.HasKey(provisioning => provisioning.TenantId);

        builder.Property(provisioning => provisioning.TenantId)
            .HasMaxLength(64);

        builder.Property(provisioning => provisioning.BootstrapSuperUserEmail)
            .IsRequired()
            .HasMaxLength(320);

        builder.Property(provisioning => provisioning.SeedBaseCategories)
            .IsRequired();

        builder.Property(provisioning => provisioning.SeedDemoCatalog)
            .IsRequired();

        builder.Property(provisioning => provisioning.EnablePublicStorefront)
            .IsRequired();

        builder.Property(provisioning => provisioning.Notes)
            .HasMaxLength(2000)
            .IsRequired(false);

        builder.Property(provisioning => provisioning.SuperUserProvisionedAtUtc)
            .IsRequired(false);

        builder.Property(provisioning => provisioning.BaseCategoriesProvisionedAtUtc)
            .IsRequired(false);

        builder.Property(provisioning => provisioning.DemoCatalogProvisionedAtUtc)
            .IsRequired(false);

        builder.Property(provisioning => provisioning.LastSynchronizedAtUtc)
            .IsRequired(false);

        builder.HasOne(provisioning => provisioning.Tenant)
            .WithOne(tenant => tenant.Provisioning)
            .HasForeignKey<TenantProvisioningEntity>(provisioning => provisioning.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
