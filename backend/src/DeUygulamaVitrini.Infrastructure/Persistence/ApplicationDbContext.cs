using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Domain.Common;
using DeUygulamaVitrini.Domain.Entities;
using DeUygulamaVitrini.Domain.Entities.Identity;
using DeUygulamaVitrini.Infrastructure.Persistence.Interceptors;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DeUygulamaVitrini.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core veritabanı bağlamı (ASP.NET Core Identity ile genişletilmiş).
/// IApplicationDbContext arayüzünü implement ederek Application katmanının
/// Infrastructure'a doğrudan bağımlı olmasını engeller.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>, IApplicationDbContext
{
    private readonly AuditableEntityInterceptor _auditInterceptor;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        AuditableEntityInterceptor auditInterceptor)
        : base(options)
    {
        _auditInterceptor = auditInterceptor;
    }

    // ─── DbSets ──────────────────────────────────────────────────────────────

    // Ana Aggregate
    public DbSet<Project> Projects => Set<Project>();

    // Reference / Master Data
    public DbSet<ProjectStatus> ProjectStatuses => Set<ProjectStatus>();
    public DbSet<ProjectCategory> ProjectCategories => Set<ProjectCategory>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Technology> Technologies => Set<Technology>();
    public DbSet<Tag> Tags => Set<Tag>();

    // Join Entities
    public DbSet<ProjectTeam> ProjectTeams => Set<ProjectTeam>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<ProjectLocation> ProjectLocations => Set<ProjectLocation>();
    public DbSet<ProjectTechnology> ProjectTechnologies => Set<ProjectTechnology>();
    public DbSet<ProjectTag> ProjectTags => Set<ProjectTag>();

    // Owned / Child Entities
    public DbSet<ProjectIntegration> ProjectIntegrations => Set<ProjectIntegration>();
    public DbSet<ProjectMedia> ProjectMediaItems => Set<ProjectMedia>();
    public DbSet<ProjectDocument> ProjectDocuments => Set<ProjectDocument>();

    // Audit Log & Notifications
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Notification> Notifications => Set<Notification>();

    // Module Access & Permissions
    public DbSet<UserModulePermission> UserModulePermissions => Set<UserModulePermission>();
    public DbSet<ModuleAccessRequest> ModuleAccessRequests => Set<ModuleAccessRequest>();

    // AI Knowledge Index & Vectors
    public DbSet<ProjectKnowledgeChunk> ProjectKnowledgeChunks => Set<ProjectKnowledgeChunk>();

    // ─── Configuration ───────────────────────────────────────────────────────

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditInterceptor);
        optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Tüm IEntityTypeConfiguration implementasyonlarını bu assembly'den otomatik yükle.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // ─── Global Query Filter — Soft Delete ───────────────────────────────
        // SoftDeletableEntity türevlerinde IsDeleted = true olan kayıtlar
        // sorgulardan otomatik olarak hariç tutulur.
        // IgnoreQueryFilters() ile gerektiğinde devre dışı bırakılabilir.
        modelBuilder.Entity<Project>().HasQueryFilter(p => !p.IsDeleted);

        // ─── Matching Query Filters (EF Core Warning 10622) ──────────────────
        // Project'e FK ile bağlı child entity'ler de aynı filtreden geçirilir.
        // Böylece soft-deleted bir projenin child kayıtlarına erişim tutarlı olur.
        modelBuilder.Entity<ProjectIntegration>()
            .HasQueryFilter(pi => !pi.Project.IsDeleted);
        modelBuilder.Entity<ProjectMedia>()
            .HasQueryFilter(pm => !pm.Project.IsDeleted);
        modelBuilder.Entity<ProjectDocument>()
            .HasQueryFilter(pd => !pd.Project.IsDeleted);
        modelBuilder.Entity<ProjectTeam>()
            .HasQueryFilter(pt => !pt.Project.IsDeleted);
        modelBuilder.Entity<ProjectMember>()
            .HasQueryFilter(pm => !pm.Project.IsDeleted);
        modelBuilder.Entity<ProjectLocation>()
            .HasQueryFilter(pl => !pl.Project.IsDeleted);
        modelBuilder.Entity<ProjectTag>()
            .HasQueryFilter(pt => !pt.Project.IsDeleted);
        modelBuilder.Entity<ProjectTechnology>()
            .HasQueryFilter(pt => !pt.Project.IsDeleted);
        modelBuilder.Entity<ProjectKnowledgeChunk>()
            .HasQueryFilter(pkc => !pkc.Project.IsDeleted);

        base.OnModelCreating(modelBuilder);
    }
}
