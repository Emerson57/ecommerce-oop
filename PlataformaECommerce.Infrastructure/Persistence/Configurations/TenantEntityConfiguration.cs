using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlataformaECommerce.Infrastructure.Persistence.Entities;

namespace PlataformaECommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configura la persistencia relacional del tenant SaaS.
/// </summary>
public sealed class TenantEntityConfiguration : IEntityTypeConfiguration<TenantEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<TenantEntity> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(tenant => tenant.TenantId);

        builder.Property(tenant => tenant.TenantId)
            .HasMaxLength(64);

        builder.Property(tenant => tenant.DisplayName)
            .IsRequired()
            .HasMaxLength(160);

        builder.Property(tenant => tenant.Enabled)
            .IsRequired();

        builder.Property(tenant => tenant.StorefrontName)
            .IsRequired()
            .HasMaxLength(160);

        builder.Property(tenant => tenant.BackofficeName)
            .IsRequired()
            .HasMaxLength(160);

        builder.Property(tenant => tenant.StorefrontTagline)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(tenant => tenant.LegalCompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(tenant => tenant.SupportEmail)
            .IsRequired()
            .HasMaxLength(320);

        builder.Property(tenant => tenant.SupportPhone)
            .IsRequired()
            .HasMaxLength(40);

        builder.Property(tenant => tenant.SupportHours)
            .IsRequired()
            .HasMaxLength(160);

        builder.Property(tenant => tenant.SupportSla)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(tenant => tenant.PrimaryColor)
            .IsRequired()
            .HasMaxLength(16);

        builder.Property(tenant => tenant.AccentColor)
            .IsRequired()
            .HasMaxLength(16);

        builder.Property(tenant => tenant.AdminSidebarStartColor)
            .IsRequired()
            .HasMaxLength(16);

        builder.Property(tenant => tenant.AdminSidebarEndColor)
            .IsRequired()
            .HasMaxLength(16);

        builder.Property(tenant => tenant.LogoGlyph)
            .IsRequired()
            .HasMaxLength(16);

        builder.Property(tenant => tenant.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.HasIndex(tenant => tenant.Enabled);
        builder.HasIndex(tenant => tenant.DisplayName);
    }
}
