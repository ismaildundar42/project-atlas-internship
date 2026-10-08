namespace DeUygulamaVitrini.Domain.Enums;

/// <summary>
/// Bir projenin geliştirme kaynağını belirtir.
/// 
/// KARAR: Ayrı tablo yerine enum tercih edilmiştir.
/// Gerekçe: Bu değerler "kurum içi mi dışı mı" sorusuna verilen kapalı kümeli
/// bir yanıttır; iş mantığı kodu bu ayrıma göre dallanabilir.
/// Bir tablo olsaydı, yeni satır eklense dahi kodun güncellenmesi gerekirdi.
/// EnumToString dönüşümü ile database'de okunabilir değerler ("Internal", "External")
/// saklanır — magic integer problemi ortadan kalkar.
/// </summary>
public enum DevelopmentType
{
    /// <summary>Tamamen kurum içi kaynaklar ve ekiplerle geliştirilen proje.</summary>
    Internal = 1,

    /// <summary>Tamamen dış kaynak/tedarikçi tarafından geliştirilen proje.</summary>
    External = 2,

    /// <summary>Hem kurum içi hem dış kaynak birlikte çalışan karma projeler.</summary>
    Hybrid = 3
}
