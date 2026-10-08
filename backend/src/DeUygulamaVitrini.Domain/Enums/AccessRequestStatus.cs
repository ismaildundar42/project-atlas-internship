namespace DeUygulamaVitrini.Domain.Enums;

/// <summary>
/// Modül erişim taleplerinin onay durumunu belirtir.
/// </summary>
public enum AccessRequestStatus
{
    /// <summary>İnceleme bekleyen yeni talep.</summary>
    Pending = 1,

    /// <summary>Yönetici tarafından onaylandı ve yetki tanımlandı.</summary>
    Approved = 2,

    /// <summary>Yönetici tarafından reddedildi.</summary>
    Rejected = 3
}
