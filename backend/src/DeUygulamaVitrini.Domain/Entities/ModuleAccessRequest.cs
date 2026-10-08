using DeUygulamaVitrini.Domain.Common;
using DeUygulamaVitrini.Domain.Entities.Identity;
using DeUygulamaVitrini.Domain.Enums;

namespace DeUygulamaVitrini.Domain.Entities;

/// <summary>
/// Kullanıcının kısıtlı bir modüle (Raporlar, Ekipler) erişebilmek için yöneticilere ilettiği erişim talebini temsil eder.
/// </summary>
public class ModuleAccessRequest : BaseEntity
{
    /// <summary>Talebi oluşturan kullanıcı kimliği.</summary>
    public int RequestedByUserId { get; set; }
    public ApplicationUser RequestedByUser { get; set; } = null!;

    /// <summary>Erişilmek istenen uygulama modülü.</summary>
    public ApplicationModule Module { get; set; }

    /// <summary>Kullanıcının talep oluştururken girdiği açıklama / gerekçe.</summary>
    public string? Reason { get; set; }

    /// <summary>Talebin onay durumu (Pending, Approved, Rejected).</summary>
    public AccessRequestStatus Status { get; set; } = AccessRequestStatus.Pending;

    /// <summary>Talebin oluşturulduğu UTC tarihi.</summary>
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Talebi inceleyen yönetici kimliği (opsiyonel).</summary>
    public int? ReviewedByUserId { get; set; }
    public ApplicationUser? ReviewedByUser { get; set; }

    /// <summary>Talebin incelendiği UTC tarihi.</summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>Yöneticinin inceleme / ret notu.</summary>
    public string? ReviewNote { get; set; }
}
