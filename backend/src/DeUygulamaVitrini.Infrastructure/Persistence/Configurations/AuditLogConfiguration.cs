using DeUygulamaVitrini.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeUygulamaVitrini.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.EntityType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.EntityId)
            .HasMaxLength(100);

        builder.Property(a => a.ActorDisplayNameSnapshot)
            .HasMaxLength(200);

        builder.Property(a => a.ActorEmailSnapshot)
            .HasMaxLength(200);

        builder.Property(a => a.EntityDisplayNameSnapshot)
            .HasMaxLength(300);

        builder.Property(a => a.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(a => a.MetadataJson)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(a => a.OccurredAtUtc);
        builder.HasIndex(a => a.ActorUserId);
        builder.HasIndex(a => a.Action);
        builder.HasIndex(a => a.EntityType);
        builder.HasIndex(a => new { a.EntityType, a.EntityId });
    }
}
