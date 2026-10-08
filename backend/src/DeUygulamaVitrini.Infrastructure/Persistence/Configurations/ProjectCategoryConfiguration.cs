using DeUygulamaVitrini.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeUygulamaVitrini.Infrastructure.Persistence.Configurations;

public class ProjectCategoryConfiguration : IEntityTypeConfiguration<ProjectCategory>
{
    public void Configure(EntityTypeBuilder<ProjectCategory> builder)
    {
        builder.ToTable("ProjectCategories");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Code).IsRequired().HasMaxLength(50);
        builder.Property(c => c.Description).HasMaxLength(500);

        builder.HasIndex(c => c.Code).IsUnique();
        builder.HasIndex(c => c.DisplayOrder);

        // ─── Seed Data ───────────────────────────────────────────────────────
        builder.HasData(
            new ProjectCategory
            {
                Id = 1,
                Name = "Yazılım",
                Code = "SOFTWARE",
                Description = "Yazılım geliştirme projeleri.",
                DisplayOrder = 10,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProjectCategory
            {
                Id = 2,
                Name = "Yapay Zeka",
                Code = "ARTIFICIAL_INTELLIGENCE",
                Description = "Makine öğrenimi, derin öğrenme ve yapay zeka projeleri.",
                DisplayOrder = 20,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProjectCategory
            {
                Id = 3,
                Name = "Veri Analitiği",
                Code = "DATA_ANALYTICS",
                Description = "Veri işleme, raporlama ve analitik projeleri.",
                DisplayOrder = 30,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProjectCategory
            {
                Id = 4,
                Name = "IoT",
                Code = "IOT",
                Description = "Nesnelerin İnterneti ve sensör tabanlı projeler.",
                DisplayOrder = 40,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProjectCategory
            {
                Id = 5,
                Name = "Otomasyon",
                Code = "AUTOMATION",
                Description = "Süreç otomasyonu ve robotik projeler.",
                DisplayOrder = 50,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProjectCategory
            {
                Id = 6,
                Name = "Madencilik Teknolojileri",
                Code = "MINING_TECHNOLOGY",
                Description = "Maden sahası operasyonlarına yönelik teknoloji projeleri.",
                DisplayOrder = 60,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProjectCategory
            {
                Id = 7,
                Name = "Ar-Ge",
                Code = "RD",
                Description = "Araştırma ve geliştirme odaklı projeler.",
                DisplayOrder = 70,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProjectCategory
            {
                Id = 8,
                Name = "Diğer",
                Code = "OTHER",
                Description = "Diğer kategori projeleri.",
                DisplayOrder = 99,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
