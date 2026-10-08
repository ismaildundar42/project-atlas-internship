namespace DeUygulamaVitrini.Domain.Enums;

/// <summary>
/// Projenin yönetim onayı durumunu belirtir.
/// </summary>
public enum ProjectApprovalStatus
{
    /// <summary>Taslak — Düzenlenebilir, henüz incelemeye gönderilmedi.</summary>
    Draft = 0,

    /// <summary>İnceleme Bekliyor — Yönetici onayına sunuldu, kilitli.</summary>
    PendingReview = 1,

    /// <summary>Onaylandı — Yönetici tarafından onaylandı ve yayına alınabilir.</summary>
    Approved = 2,

    /// <summary>Reddedildi — Yönetici tarafından gerekçe belirtilerek reddedildi.</summary>
    Rejected = 3
}
