using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlataformaECommerce.Infrastructure.Persistence.Entities;

namespace PlataformaECommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configura la persistencia relacional de los hostnames asociados a tenants.
/// </summary>
public sealed class TenantHostnameEntityConfiguration : IEntityTypeConfiguration<TenantHostnameEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<TenantHostnameEntity> builder)
    {
        builder.ToTable("TenantHostnames");

        builder.HasKey(hostname => new { hostname.TenantId, hostname.Hostname });

        builder.Property(hostname => hostname.TenantId)
            .HasMaxLength(64);

        builder.Property(hostname => hostname.Hostname)
            .HasMaxLength(255);

        builder.HasOne(hostname => hostname.Tenant)
            .WithMany(tenant => tenant.Hostnames)
            .HasForeignKey(hostname => hostname.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(hostname => hostname.Hostname)
            .IsUnique();
    }
}
