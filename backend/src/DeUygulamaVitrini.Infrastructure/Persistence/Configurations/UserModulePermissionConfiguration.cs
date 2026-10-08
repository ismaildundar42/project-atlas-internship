using DeUygulamaVitrini.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeUygulamaVitrini.Infrastructure.Persistence.Configurations;

public class UserModulePermissionConfiguration : IEntityTypeConfiguration<UserModulePermission>
{
    public void Configure(EntityTypeBuilder<UserModulePermission> builder)
    {
        builder.ToTable("UserModulePermissions");

        builder.HasKey(p => p.Id);

        // Bir kullanıcının aynı modül için yalnızca 1 adet yetki kaydı bulunabilir
        builder.HasIndex(p => new { p.UserId, p.Module })
            .IsUnique();

        builder.Property(p => p.Module)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.GrantedAt)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.GrantedByUser)
            .WithMany()
            .HasForeignKey(p => p.GrantedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
