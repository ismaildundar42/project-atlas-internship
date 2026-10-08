using DeUygulamaVitrini.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeUygulamaVitrini.Infrastructure.Persistence.Configurations;

public class ProjectMediaConfiguration : IEntityTypeConfiguration<ProjectMedia>
{
    public void Configure(EntityTypeBuilder<ProjectMedia> builder)
    {
        builder.ToTable("ProjectMediaItems");
        builder.HasKey(pm => pm.Id);

        builder.Property(pm => pm.FileName).IsRequired().HasMaxLength(260);
        builder.Property(pm => pm.FileUrl).IsRequired().HasMaxLength(2048);
        builder.Property(pm => pm.AltText).HasMaxLength(500);
        builder.Property(pm => pm.Caption).HasMaxLength(500);

        builder.Property(pm => pm.MediaType)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.HasIndex(pm => pm.ProjectId);
        builder.HasIndex(pm => pm.DisplayOrder);

        // DELETE BEHAVIOR: Cascade — Medya kaydı projeye aittir; proje gittiğinde medya da gider.
        builder.HasOne(pm => pm.Project)
            .WithMany(p => p.ProjectMediaItems)
            .HasForeignKey(pm => pm.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(pm => pm.CreatedAt).IsRequired();
    }
}
