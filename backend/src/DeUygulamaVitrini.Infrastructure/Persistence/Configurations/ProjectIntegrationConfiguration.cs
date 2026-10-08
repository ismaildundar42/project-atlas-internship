using DeUygulamaVitrini.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeUygulamaVitrini.Infrastructure.Persistence.Configurations;

public class ProjectIntegrationConfiguration : IEntityTypeConfiguration<ProjectIntegration>
{
    public void Configure(EntityTypeBuilder<ProjectIntegration> builder)
    {
        builder.ToTable("ProjectIntegrations");
        builder.HasKey(pi => pi.Id);

        builder.Property(pi => pi.Name).IsRequired().HasMaxLength(200);
        builder.Property(pi => pi.Description).HasMaxLength(1000);

        builder.Property(pi => pi.IntegrationType)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(pi => pi.ProjectId);

        // DELETE BEHAVIOR: Cascade — Proje silindiğinde entegrasyon kayıtları da silinir.
        // Bu mantıklıdır çünkü entegrasyon kaydı projeye özgüdür; bağımsız değeri yoktur.
        builder.HasOne(pi => pi.Project)
            .WithMany(p => p.ProjectIntegrations)
            .HasForeignKey(pi => pi.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(pi => pi.CreatedAt).IsRequired();
    }
}
