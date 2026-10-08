using DeUygulamaVitrini.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeUygulamaVitrini.Infrastructure.Persistence.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("Members");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(m => m.LastName).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Title).HasMaxLength(200);
        builder.Property(m => m.Email).HasMaxLength(254); // RFC 5321 max email length

        builder.HasIndex(m => m.Email);
        builder.Property(m => m.CreatedAt).IsRequired();

        // ─── Identity User Link (Phase 13.2) ─────────────────────────────────
        builder.HasOne(m => m.User)
            .WithOne()
            .HasForeignKey<Member>(m => m.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(m => m.UserId)
            .IsUnique();

        // ─── Team / Organization Link (Phase 19.7) ───────────────────────────
        builder.HasOne(m => m.Team)
            .WithMany()
            .HasForeignKey(m => m.TeamId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(m => m.TeamId);
    }
}

