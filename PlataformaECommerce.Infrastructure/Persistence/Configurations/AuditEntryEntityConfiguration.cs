using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlataformaECommerce.Infrastructure.Persistence.Entities;

namespace PlataformaECommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configura la entidad persistente <see cref="AuditEntryEntity"/> mediante Fluent API.
/// </summary>
public sealed class AuditEntryEntityConfiguration : IEntityTypeConfiguration<AuditEntryEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AuditEntryEntity> builder)
    {
        builder.ToTable("AuditEntries");

        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Id)
            .ValueGeneratedNever();

        builder.Property(entry => entry.TenantId)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(entry => entry.AggregateId)
            .IsRequired();

        builder.Property(entry => entry.AggregateType)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(entry => entry.Module)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(entry => entry.Action)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(entry => entry.Detail)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(entry => entry.PerformedBy)
            .IsRequired()
            .HasMaxLength(320);

        builder.Property(entry => entry.PerformedByUserId)
            .HasMaxLength(64)
            .IsRequired(false);

        builder.Property(entry => entry.OccurredAtUtc)
            .IsRequired();

        builder.Property(entry => entry.CorrelationId)
            .HasMaxLength(128)
            .IsRequired(false);

        builder.Property(entry => entry.Source)
            .HasMaxLength(256)
            .IsRequired(false);

        builder.Property(entry => entry.MetadataJson)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(entry => entry.TenantId);
        builder.HasIndex(entry => entry.OccurredAtUtc);
        builder.HasIndex(entry => new { entry.TenantId, entry.OccurredAtUtc });
        builder.HasIndex(entry => new { entry.TenantId, entry.AggregateType, entry.AggregateId, entry.OccurredAtUtc });
        builder.HasIndex(entry => entry.Module);
        builder.HasIndex(entry => entry.Action);
        builder.HasIndex(entry => entry.CorrelationId);
        builder.HasIndex(entry => entry.PerformedBy);
    }
}
