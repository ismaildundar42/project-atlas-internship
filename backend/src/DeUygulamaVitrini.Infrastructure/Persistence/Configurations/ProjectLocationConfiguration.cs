using DeUygulamaVitrini.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeUygulamaVitrini.Infrastructure.Persistence.Configurations;

public class ProjectLocationConfiguration : IEntityTypeConfiguration<ProjectLocation>
{
    public void Configure(EntityTypeBuilder<ProjectLocation> builder)
    {
        builder.ToTable("ProjectLocations");
        builder.HasKey(pl => new { pl.ProjectId, pl.LocationId });

        builder.HasOne(pl => pl.Project)
            .WithMany(p => p.ProjectLocations)
            .HasForeignKey(pl => pl.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // DELETE BEHAVIOR: Restrict — Lokasyon silinince proje kayıtlarından sessizce kalkmamalı.
        builder.HasOne(pl => pl.Location)
            .WithMany(l => l.ProjectLocations)
            .HasForeignKey(pl => pl.LocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
