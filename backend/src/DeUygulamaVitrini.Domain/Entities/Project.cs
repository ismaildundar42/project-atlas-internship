using DeUygulamaVitrini.Domain.Common;
using DeUygulamaVitrini.Domain.Enums;

namespace DeUygulamaVitrini.Domain.Entities;

/// <summary>
/// Sistemin ana aggregate entity'si — bir Ar-Ge projesini temsil eder.
///
/// SoftDeletableEntity'den türetilmiştir çünkü silinen bir proje kaydının
/// fiziksel olarak yok edilmesi proje geçmişini ve ilişkili kayıtları bozar.
/// Silme işlemi mantıksal olarak gerçekleşir; EF Core Global Query Filter
/// aracılığıyla sorgular otomatik olarak IsDeleted = false filtresi alır.
/// </summary>
public class Project : SoftDeletableEntity
{
    // ─── Temel Bilgiler ────────────────────────────────────────────────────────

    /// <summary>Projenin resmi adı.</summary>
    public required string Name { get; set; }

    /// <summary>
    /// URL-dostu benzersiz tanımlayıcı.
    /// Ör: "saha-veri-takip-sistemi"
    /// Proje detay sayfasının URL'sinde kullanılır.
    /// </summary>
    public required string Slug { get; set; }

    /// <summary>Tek cümlelik kısa açıklama — kart görünümü, arama sonuçları vb. için.</summary>
    public required string ShortDescription { get; set; }

    /// <summary>Genel proje açıklaması (markdown desteklenebilir).</summary>
    public string? Description { get; set; }

    // ─── Amaç & Problem ───────────────────────────────────────────────────────

    /// <summary>Projenin geliştirilme amacı.</summary>
    public string? Purpose { get; set; }

    /// <summary>Projenin çözdüğü iş veya operasyonel problem.</summary>
    public string? ProblemSolved { get; set; }

    // ─── Hedef Kitle & İş Etkisi ─────────────────────────────────────────────

    /// <summary>Teknik bilgisi olmayan saha çalışanları için sade açıklama.</summary>
    public string? NonTechnicalDescription { get; set; }

    /// <summary>Teknik kullanıcılar için detaylı mimari ve implementasyon açıklaması.</summary>
    public string? TechnicalDescription { get; set; }

    /// <summary>Projenin sağladığı iş kazancı, verimlilik artışı veya ROI etkisi.</summary>
    public string? BusinessImpact { get; set; }

    /// <summary>Projenin hedeflenen kullanıcı kitlesi / personeli (ör. "Saha Operatörleri", "Jeoloji Mühendisleri").</summary>
    public string? TargetAudience { get; set; }

    /// <summary>Saha ve genel kullanım / erişim talimatları veya notları.</summary>
    public string? AccessInstructions { get; set; }

    /// <summary>Canlı uygulamaya doğrudan erişim URL'si (opsiyonel).</summary>
    public string? ApplicationUrl { get; set; }

    /// <summary>Proje kaynak kod deposu URL'si (opsiyonel).</summary>
    public string? RepositoryUrl { get; set; }

    /// <summary>Projenin özel kapak görseli URL'si (opsiyonel).</summary>
    public string? CoverImageUrl { get; set; }

    // ─── Sınıflandırma ────────────────────────────────────────────────────────

    /// <summary>Projenin geliştirme durumu (Foreign Key → ProjectStatus).</summary>
    public int StatusId { get; set; }

    /// <summary>Projenin teknik kategorisi (Foreign Key → ProjectCategory).</summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Geliştirme tipi: Internal, External, Hybrid.
    /// Enum olarak saklanır; database'e string değer yazılır (magic integer önleme).
    /// </summary>
    public DevelopmentType DevelopmentType { get; set; }

    // ─── Tarihler ─────────────────────────────────────────────────────────────

    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    // ─── Görünürlük & Yayın ───────────────────────────────────────────────────

    /// <summary>Proje ana sayfada / öne çıkanlar bölümünde gösterilsin mi?</summary>
    public bool IsFeatured { get; set; } = false;

    /// <summary>Proje kütüphanede listelensin mi? (false = taslak/gizli)</summary>
    public bool IsPublished { get; set; } = false;

    // ─── Onay Süreci (Approval Workflow) ────────────────────────────────────

    /// <summary>Projenin yönetim inceleme/onay durumu (Draft, PendingReview, Approved, Rejected).</summary>
    public ProjectApprovalStatus ApprovalStatus { get; set; } = ProjectApprovalStatus.Draft;

    /// <summary>İncelemeye gönderildiği UTC tarihi.</summary>
    public DateTime? SubmittedForReviewAt { get; set; }

    /// <summary>İncelemeye gönderen kullanıcı ID (Foreign Key → ApplicationUser).</summary>
    public int? SubmittedForReviewByUserId { get; set; }
    public Identity.ApplicationUser? SubmittedForReviewByUser { get; set; }

    /// <summary>İncelendiği / Onaylandığı / Reddedildiği UTC tarihi.</summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>İnceleyen yönetici ID (Foreign Key → ApplicationUser).</summary>
    public int? ReviewedByUserId { get; set; }
    public Identity.ApplicationUser? ReviewedByUser { get; set; }

    /// <summary>Reddetme gerekçesi (Yönetici notu).</summary>
    public string? RejectionReason { get; set; }

    // ─── Sahiplik & Oluşturan Bilgisi ────────────────────────────────────────

    /// <summary>Projeyi oluşturan kullanıcı (Foreign Key → ApplicationUser).</summary>
    public int? CreatedByUserId { get; set; }
    public Identity.ApplicationUser? CreatedByUser { get; set; }

    // ─── Navigation Properties ────────────────────────────────────────────────

    public ProjectStatus Status { get; set; } = null!;
    public ProjectCategory Category { get; set; } = null!;

    public ICollection<ProjectTeam> ProjectTeams { get; set; } = new List<ProjectTeam>();
    public ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
    public ICollection<ProjectLocation> ProjectLocations { get; set; } = new List<ProjectLocation>();
    public ICollection<ProjectTechnology> ProjectTechnologies { get; set; } = new List<ProjectTechnology>();
    public ICollection<ProjectTag> ProjectTags { get; set; } = new List<ProjectTag>();
    public ICollection<ProjectIntegration> ProjectIntegrations { get; set; } = new List<ProjectIntegration>();
    public ICollection<ProjectMedia> ProjectMediaItems { get; set; } = new List<ProjectMedia>();
    public ICollection<ProjectDocument> ProjectDocuments { get; set; } = new List<ProjectDocument>();
    public ICollection<ProjectKnowledgeChunk> ProjectKnowledgeChunks { get; set; } = new List<ProjectKnowledgeChunk>();
}
