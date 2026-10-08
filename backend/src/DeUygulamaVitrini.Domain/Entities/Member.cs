using DeUygulamaVitrini.Domain.Common;
using DeUygulamaVitrini.Domain.Entities.Identity;

namespace DeUygulamaVitrini.Domain.Entities;

/// <summary>
/// Projelerde yer alan kişileri temsil eden profil entity'si.
///
/// ÖNEMLİ AYRIM: Member, sisteme giriş yapan User ile kasıtlı olarak
/// ayrı tutulmuştur. Authentication eklenmeden önce de proje ekip listesi
/// oluşturulabilmesi için bu mimari tercih yapılmıştır.
///
/// Authentication eklendiğinde Member kaydı, Users tablosundaki bir kullanıcıyla
/// isteğe bağlı olarak ilişkilendirilebilir (örn. nullable UserId FK).
/// Bu ilişki ileriki bir phase'te tasarlanacaktır.
/// </summary>
public class Member : BaseEntity
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }

    /// <summary>Kurumsal unvan. (ör. "Kıdemli Yazılım Mühendisi")</summary>
    public string? Title { get; set; }

    /// <summary>Kurumsal e-posta adresi.</summary>
    public string? Email { get; set; }

    // ─── Identity Link (Phase 13.2) ──────────────────────────────────────────
    /// <summary>
    /// Bu organizasyonel kişinin sisteme giriş yapabilen ApplicationUser hesabı ile bağlantısı (opsiyonel).
    /// </summary>
    public int? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    // ─── Organization Link (Phase 19.7) ──────────────────────────────────────
    /// <summary>
    /// Bu kişinin bağlı olduğu organizasyonel ekip (opsiyonel).
    /// Departman bilgisi Team.Department üzerinden türetilir (no redundant DepartmentId).
    /// </summary>
    public int? TeamId { get; set; }
    public Team? Team { get; set; }

    // Navigation
    public ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
}

