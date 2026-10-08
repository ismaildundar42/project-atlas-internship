using DeUygulamaVitrini.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeUygulamaVitrini.Infrastructure.Persistence.Configurations;

public class ModuleAccessRequestConfiguration : IEntityTypeConfiguration<ModuleAccessRequest>
{
    public void Configure(EntityTypeBuilder<ModuleAccessRequest> builder)
    {
        builder.ToTable("ModuleAccessRequests");

        builder.HasKey(r => r.Id);

        builder.HasIndex(r => new { r.RequestedByUserId, r.Module, r.Status });
        builder.HasIndex(r => r.Status);

        builder.Property(r => r.Module)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(r => r.Reason)
            .HasMaxLength(1000);

        builder.Property(r => r.ReviewNote)
            .HasMaxLength(1000);

        builder.Property(r => r.RequestedAt)
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.HasOne(r => r.RequestedByUser)
            .WithMany()
            .HasForeignKey(r => r.RequestedByUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.ReviewedByUser)
            .WithMany()
            .HasForeignKey(r => r.ReviewedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
