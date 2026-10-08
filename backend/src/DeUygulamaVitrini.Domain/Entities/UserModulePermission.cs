using DeUygulamaVitrini.Domain.Common;
using DeUygulamaVitrini.Domain.Entities.Identity;
using DeUygulamaVitrini.Domain.Enums;

namespace DeUygulamaVitrini.Domain.Entities;

/// <summary>
/// Bir kullanıcının erişim izni verilmiş özel bir modüle (Raporlar, Ekipler vb.) yetkisini temsil eder.
/// </summary>
public class UserModulePermission : BaseEntity
{
    /// <summary>Yetki verilen kullanıcı kimliği.</summary>
    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    /// <summary>Yetkilendirilen uygulama modülü.</summary>
    public ApplicationModule Module { get; set; }

    /// <summary>Yetkiyi atayan yönetici kimliği (opsiyonel / doğrudan veya talep onayı).</summary>
    public int? GrantedByUserId { get; set; }
    public ApplicationUser? GrantedByUser { get; set; }

    /// <summary>Yetkinin verildiği UTC tarihi.</summary>
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
}
