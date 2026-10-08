using DeUygulamaVitrini.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeUygulamaVitrini.Infrastructure.Persistence.Configurations;

public class ProjectStatusConfiguration : IEntityTypeConfiguration<ProjectStatus>
{
    public void Configure(EntityTypeBuilder<ProjectStatus> builder)
    {
        builder.ToTable("ProjectStatuses");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Code).IsRequired().HasMaxLength(50);
        builder.Property(s => s.Description).HasMaxLength(500);

        builder.HasIndex(s => s.Code).IsUnique();
        builder.HasIndex(s => s.DisplayOrder);

        // ─── Seed Data ───────────────────────────────────────────────────────
        // ID'ler sabit tutulmuştur — migration'lar arası kararlılık için.
        // CreatedAt sabit bir değer verilmiştir; interceptor seed sırasında çalışmaz.
        builder.HasData(
            new ProjectStatus
            {
                Id = 1,
                Name = "Planlama",
                Code = "PLANNING",
                Description = "Proje henüz planlama aşamasındadır.",
                DisplayOrder = 10,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProjectStatus
            {
                Id = 2,
                Name = "Kavram Kanıtı",
                Code = "PROOF_OF_CONCEPT",
                Description = "Projenin fizibilitesi araştırılmaktadır.",
                DisplayOrder = 20,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProjectStatus
            {
                Id = 3,
                Name = "Pilot",
                Code = "PILOT",
                Description = "Proje sınırlı kapsamda pilot olarak uygulanmaktadır.",
                DisplayOrder = 30,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProjectStatus
            {
                Id = 4,
                Name = "Aktif Geliştirme",
                Code = "ACTIVE_DEVELOPMENT",
                Description = "Proje aktif olarak geliştirilmektedir.",
                DisplayOrder = 40,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProjectStatus
            {
                Id = 5,
                Name = "Aktif",
                Code = "ACTIVE",
                Description = "Proje yayında ve aktif kullanımdadır.",
                DisplayOrder = 50,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProjectStatus
            {
                Id = 6,
                Name = "Beklemede",
                Code = "ON_HOLD",
                Description = "Proje geçici olarak durdurulmuştur.",
                DisplayOrder = 60,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProjectStatus
            {
                Id = 7,
                Name = "Tamamlandı",
                Code = "COMPLETED",
                Description = "Proje başarıyla tamamlanmıştır.",
                DisplayOrder = 70,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProjectStatus
            {
                Id = 8,
                Name = "Arşivlendi",
                Code = "ARCHIVED",
                Description = "Proje arşivlenmiştir, aktif kullanımda değildir.",
                DisplayOrder = 80,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
