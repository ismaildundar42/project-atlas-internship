using DeUygulamaVitrini.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeUygulamaVitrini.Application.Common.Interfaces;

/// <summary>
/// Application katmanının veritabanı bağlamına erişim arayüzü.
/// Infrastructure bağımlılığını soyutlayarak Application katmanını
/// EF Core implementasyonundan ayırır ve birim testi kolaylaştırır.
///
/// TASARIM KARARI:
/// Yalnızca Application katmanındaki use-case'lerin gerçekten ihtiyaç duyduğu
/// DbSet'ler buraya dahil edilmiştir. Interface'i şişirmemek için
/// join table DbSet'leri ve yalnızca Infrastructure'ın kullandığı set'ler
/// doğrudan ApplicationDbContext'te bırakılmıştır.
/// </summary>
public interface IApplicationDbContext
{
    // ─── Ana Aggregate ────────────────────────────────────────────────────────
    DbSet<Project> Projects { get; }

    // ─── Reference Data ───────────────────────────────────────────────────────
    DbSet<ProjectStatus> ProjectStatuses { get; }
    DbSet<ProjectCategory> ProjectCategories { get; }
    DbSet<Department> Departments { get; }
    DbSet<Team> Teams { get; }
    DbSet<Member> Members { get; }
    DbSet<Location> Locations { get; }
    DbSet<Technology> Technologies { get; }
    DbSet<Tag> Tags { get; }

    // ─── Join Entities ───────────────────────────────────────────────────────
    DbSet<ProjectTeam> ProjectTeams { get; }
    DbSet<ProjectMember> ProjectMembers { get; }

    // ─── Audit & Notifications ──────────────────────────────────────────────
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Notification> Notifications { get; }

    // ─── AI Knowledge Index ──────────────────────────────────────────────────
    DbSet<ProjectKnowledgeChunk> ProjectKnowledgeChunks { get; }

    // ─── Module Access & Permissions ─────────────────────────────────────────
    DbSet<UserModulePermission> UserModulePermissions { get; }
    DbSet<ModuleAccessRequest> ModuleAccessRequests { get; }

    // ─── Persistence ──────────────────────────────────────────────────────────
    /// <summary>Bekleyen tüm değişiklikleri veritabanına kayıt eder.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
