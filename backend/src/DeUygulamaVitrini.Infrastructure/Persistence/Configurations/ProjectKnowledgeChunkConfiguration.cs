using DeUygulamaVitrini.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeUygulamaVitrini.Infrastructure.Persistence.Configurations;

public class ProjectKnowledgeChunkConfiguration : IEntityTypeConfiguration<ProjectKnowledgeChunk>
{
    public void Configure(EntityTypeBuilder<ProjectKnowledgeChunk> builder)
    {
        builder.ToTable("ProjectKnowledgeChunks");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.ChunkKey).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Content).IsRequired();
        builder.Property(c => c.ContentHash).IsRequired().HasMaxLength(64);
        builder.Property(c => c.EmbeddingModel).IsRequired().HasMaxLength(128);
        builder.Property(c => c.EmbeddingDimension).IsRequired();
        builder.Property(c => c.EmbeddingVector).IsRequired();
        builder.Property(c => c.IndexedAtUtc).IsRequired();

        builder.HasIndex(c => c.ProjectId);
        builder.HasIndex(c => new { c.ProjectId, c.ChunkKey }).IsUnique();

        // DELETE BEHAVIOR: Cascade — Proje silindiğinde bilgi parçaları da silinir.
        builder.HasOne(c => c.Project)
            .WithMany(p => p.ProjectKnowledgeChunks)
            .HasForeignKey(c => c.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(c => c.CreatedAt).IsRequired();
    }
}
