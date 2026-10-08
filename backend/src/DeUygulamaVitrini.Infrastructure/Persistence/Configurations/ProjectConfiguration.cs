using DeUygulamaVitrini.Domain.Entities;
using DeUygulamaVitrini.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeUygulamaVitrini.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

        // ─── Primary Key ──────────────────────────────────────────────────────
        builder.HasKey(p => p.Id);

        // ─── Required Fields ──────────────────────────────────────────────────
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Slug)
            .IsRequired()
            .HasMaxLength(220);

        builder.Property(p => p.ShortDescription)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.Description).HasColumnType("nvarchar(max)");
        builder.Property(p => p.Purpose).HasColumnType("nvarchar(max)");
        builder.Property(p => p.ProblemSolved).HasColumnType("nvarchar(max)");
        builder.Property(p => p.NonTechnicalDescription).HasColumnType("nvarchar(max)");
        builder.Property(p => p.TechnicalDescription).HasColumnType("nvarchar(max)");
        builder.Property(p => p.BusinessImpact).HasColumnType("nvarchar(max)");
        builder.Property(p => p.TargetAudience).HasMaxLength(1000);
        builder.Property(p => p.AccessInstructions).HasColumnType("nvarchar(max)");
        builder.Property(p => p.ApplicationUrl).HasMaxLength(500);
        builder.Property(p => p.RepositoryUrl).HasMaxLength(500);
        builder.Property(p => p.CoverImageUrl).HasMaxLength(500);

        // ─── Enum → String (okunabilirlik, magic integer önleme) ─────────────
        builder.Property(p => p.DevelopmentType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // ─── Audit (BaseEntity'den gelen alanlar) ────────────────────────────
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.CreatedBy).HasMaxLength(200);
        builder.Property(p => p.UpdatedBy).HasMaxLength(200);
        builder.Property(p => p.DeletedBy).HasMaxLength(200);

        // ─── Approval Workflow Fields ─────────────────────────────────────────
        builder.Property(p => p.ApprovalStatus)
            .HasConversion<int>()
            .HasDefaultValue(ProjectApprovalStatus.Draft)
            .IsRequired();

        builder.Property(p => p.RejectionReason)
            .HasMaxLength(1000);

        builder.HasOne(p => p.SubmittedForReviewByUser)
            .WithMany()
            .HasForeignKey(p => p.SubmittedForReviewByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ReviewedByUser)
            .WithMany()
            .HasForeignKey(p => p.ReviewedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ─── Foreign Keys ─────────────────────────────────────────────────────
        //
        // DELETE BEHAVIOR KARARLARI:
        //
        // Status/Category → Restrict:
        //   Bir status veya kategori silinmek istenirse önce ona atanmış projeler
        //   güncellenmelidir. Cascade ile tüm projelerin silinmesi felaket olur.
        //
        builder.HasOne(p => p.Status)
            .WithMany(s => s.Projects)
            .HasForeignKey(p => p.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Projects)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.CreatedByUser)
            .WithMany()
            .HasForeignKey(p => p.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // ─── Indexes ─────────────────────────────────────────────────────────
        builder.HasIndex(p => p.Slug).IsUnique();
        builder.HasIndex(p => p.Name);
        builder.HasIndex(p => p.StatusId);
        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => p.CreatedByUserId);
        builder.HasIndex(p => p.ApprovalStatus);
        builder.HasIndex(p => p.IsPublished);
        builder.HasIndex(p => p.IsDeleted);
    }
}
