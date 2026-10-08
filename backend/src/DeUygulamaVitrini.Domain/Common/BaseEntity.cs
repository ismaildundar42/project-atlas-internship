namespace DeUygulamaVitrini.Domain.Common;

/// <summary>
/// Tüm entity'ler için temel sınıf.
/// Ortak audit alanlarını merkezi olarak yönetir.
///
/// Tarihler UTC olarak saklanır.
/// CreatedBy / UpdatedBy alanları authentication implementasyonu
/// tamamlandıktan sonra aktif hale getirilecektir.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Authentication eklendiğinde doldurulacak.
    // Şimdilik nullable bırakıldı; zorunlu hale getirilmesi authentication sonrasına ertelendi.
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Soft delete desteği olan entity'ler için temel sınıf.
///
/// KARAR: Soft delete yalnızca Project entity'sine uygulanmaktadır.
/// Gerekçe: Bir proje kaydının fiziksel olarak silinmesi,
/// proje geçmişini, audit kayıtlarını ve ilgili medya/doküman referanslarını yok eder.
/// Soft delete ile silinen proje kurtarılabilir ve "arşiv" olarak kalabilir.
///
/// ProjectStatus → Archived durumu ile soft delete birbirini tamamlar:
/// - Archived: Proje bitti ama kayıt görünür ve aktif.
/// - IsDeleted: Kayıt listelerden tamamen kaldırıldı ama database'de korunuyor.
///
/// Reference data (Status, Category, Technology vb.) bu sınıfı KULLANMAZ.
/// Çünkü bu entity'ler başka projelerce referans alındığından
/// "silinmiş" görünen bir reference kaydı karmaşıklık yaratır.
/// Reference data silinmek istendiğinde "aktif/pasif" flag'i eklenebilir — ileride değerlendirilebilir.
///
/// EF Core Global Query Filter ApplicationDbContext'te tanımlanmıştır;
/// bu sayede sorgular otomatik olarak IsDeleted = false filtresi uygular.
/// </summary>
public abstract class SoftDeletableEntity : BaseEntity
{
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
