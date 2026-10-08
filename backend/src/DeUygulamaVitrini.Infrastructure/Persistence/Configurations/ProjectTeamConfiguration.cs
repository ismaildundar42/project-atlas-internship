using DeUygulamaVitrini.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeUygulamaVitrini.Infrastructure.Persistence.Configurations;

public class ProjectTeamConfiguration : IEntityTypeConfiguration<ProjectTeam>
{
    public void Configure(EntityTypeBuilder<ProjectTeam> builder)
    {
        builder.ToTable("ProjectTeams");

        // Composite primary key
        builder.HasKey(pt => new { pt.ProjectId, pt.TeamId });

        // ─── Project tarafı ───────────────────────────────────────────────────
        // DELETE BEHAVIOR: Cascade (proje silinince proje-ekip bağlantısı da silinir)
        // Ekibin kendisi silinmez; yalnızca ilişki kaydı silinir.
        builder.HasOne(pt => pt.Project)
            .WithMany(p => p.ProjectTeams)
            .HasForeignKey(pt => pt.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // ─── Team tarafı ──────────────────────────────────────────────────────
        // DELETE BEHAVIOR: Restrict
        // Bir ekip silinmek istendiğinde önce proje bağlantıları temizlenmelidir.
        // Sessiz cascade ile proje geçmişi kaybolabilir.
        builder.HasOne(pt => pt.Team)
            .WithMany(t => t.ProjectTeams)
            .HasForeignKey(pt => pt.TeamId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
