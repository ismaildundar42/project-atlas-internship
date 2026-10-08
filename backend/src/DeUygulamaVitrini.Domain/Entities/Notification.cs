using DeUygulamaVitrini.Domain.Entities.Identity;

namespace DeUygulamaVitrini.Domain.Entities;

/// <summary>
/// Proje onay iş akışları ve kullanıcı bilgilendirmeleri için sistem içi bildirim kaydı.
/// </summary>
public class Notification
{
    public long Id { get; set; }

    /// <summary>
    /// Bildirimin ait olduğu ApplicationUser ID'si.
    /// </summary>
    public int RecipientUserId { get; set; }

    public ApplicationUser? RecipientUser { get; set; }

    /// <summary>
    /// Bildirim tipi (örn. ProjectSubmittedForReview, ProjectApproved, ProjectRejected).
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Bildirim başlığı.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Bildirim kısa metni / açıklaması.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// İlişkili proje ID'si (opsiyonel).
    /// </summary>
    public int? ProjectId { get; set; }

    /// <summary>
    /// Tıklandığında yönlendirilecek hedef URL.
    /// </summary>
    public string? TargetUrl { get; set; }

    /// <summary>
    /// Bildirimin kullanıcı tarafından okunup okunmadığı.
    /// </summary>
    public bool IsRead { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? ReadAtUtc { get; set; }
}
