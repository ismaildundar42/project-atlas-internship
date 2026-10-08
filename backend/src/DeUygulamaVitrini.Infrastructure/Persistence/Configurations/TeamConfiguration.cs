using DeUygulamaVitrini.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeUygulamaVitrini.Infrastructure.Persistence.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("Teams");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name).IsRequired().HasMaxLength(150);
        builder.Property(t => t.Description).HasMaxLength(500);

        builder.HasIndex(t => t.Name);
        builder.HasIndex(t => t.DepartmentId);

        // ─── Department İlişkisi ──────────────────────────────────────────────
        // DELETE BEHAVIOR: Restrict
        // Bir departman silinmek istendiğinde, önce altındaki ekipler başka
        // departmana taşınmalı veya silinmelidir. Cascade ile departman silinince
        // ekipler ve dolaylı olarak proje-ekip bağlantıları yanlışlıkla yok olabilir.
        builder.HasOne(t => t.Department)
            .WithMany(d => d.Teams)
            .HasForeignKey(t => t.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(t => t.CreatedAt).IsRequired();
    }
}
