using DeUygulamaVitrini.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeUygulamaVitrini.Infrastructure.Persistence.Configurations;

public class ProjectDocumentConfiguration : IEntityTypeConfiguration<ProjectDocument>
{
    public void Configure(EntityTypeBuilder<ProjectDocument> builder)
    {
        builder.ToTable("ProjectDocuments");
        builder.HasKey(pd => pd.Id);

        builder.Property(pd => pd.Name).IsRequired().HasMaxLength(200);
        builder.Property(pd => pd.Description).HasMaxLength(500);
        builder.Property(pd => pd.FileName).IsRequired().HasMaxLength(260);
        builder.Property(pd => pd.FileUrl).IsRequired().HasMaxLength(2048);
        builder.Property(pd => pd.DocumentType).HasMaxLength(100);

        builder.HasIndex(pd => pd.ProjectId);

        // DELETE BEHAVIOR: Cascade — Doküman projeye aittir.
        builder.HasOne(pd => pd.Project)
            .WithMany(p => p.ProjectDocuments)
            .HasForeignKey(pd => pd.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(pd => pd.CreatedAt).IsRequired();
    }
}
