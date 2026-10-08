namespace DeUygulamaVitrini.Domain.Enums;

/// <summary>
/// Bir lokasyonun türünü belirtir.
///
/// KARAR: Enum olarak tanımlanmıştır.
/// Gerekçe: LocationType sabit ve kapalı bir küme olup iş kuralı kodunda
/// dallanma yapılabilir. Yeni tür eklenmesi nadirdir ve deployment gerektirir —
/// bu durum tablo yerine enum kullanımını meşrulaştırır.
/// </summary>
public enum LocationType
{
    /// <summary>Açık ocak veya yeraltı maden sahası.</summary>
    MineSite = 1,

    /// <summary>Yönetim veya ofis binası.</summary>
    Office = 2,

    /// <summary>Üretim tesisi, fabrika veya işleme tesisi.</summary>
    Facility = 3,

    /// <summary>Diğer lokasyon türleri.</summary>
    Other = 99
}
