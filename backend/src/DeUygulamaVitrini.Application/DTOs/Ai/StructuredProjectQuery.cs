using DeUygulamaVitrini.Domain.Enums;

namespace DeUygulamaVitrini.Application.DTOs.Ai;

/// <summary>
/// Proje Kütüphanesi yapılandırılmış (deterministik) SQL sorgu modeli.
/// LLM tarafından üretilmez; sunucu tarafı ayrıştırıcı (parser) tarafından
/// izin verilen alanlar ve filtreler çerçevesinde güvenli bir şekilde oluşturulur.
/// </summary>
public record StructuredProjectQuery
{
    /// <summary>İstenen maksimum proje adedi (1 ile 20 arasında sınırlandırılır).</summary>
    public int Limit { get; init; } = 5;

    /// <summary>Sıralama alanı: CreatedAt, UpdatedAt, Name, StartDate.</summary>
    public string SortField { get; init; } = "CreatedAt";

    /// <summary>Sıralama yönü: Desc (en yeni), Asc (en eski/alfabetik).</summary>
    public string SortDirection { get; init; } = "Desc";

    /// <summary>Filtrelenecek durum kodu veya adı (örn: production, planning, completed).</summary>
    public string? StatusKeyword { get; init; }

    /// <summary>Filtrelenecek kategori kodu veya adı (örn: ai, iot, yazılım, madencilik).</summary>
    public string? CategoryKeyword { get; init; }

    /// <summary>Filtrelenecek lokasyon adı (örn: Kangal, Divriği, Sivas).</summary>
    public string? LocationKeyword { get; init; }

    /// <summary>Filtrelenecek teknoloji adı (örn: Python, .NET, React, SCADA, SAP).</summary>
    public string? TechnologyKeyword { get; init; }

    /// <summary>Filtrelenecek ekip adı (örn: Yazılım Geliştirme, Dijital Dönüşüm, Kestirimci Bakım).</summary>
    public string? TeamKeyword { get; init; }

    /// <summary>Yalnızca öne çıkan projeler mi?</summary>
    public bool? IsFeatured { get; init; }

    /// <summary>Geliştirme tipi (Internal, External, Hybrid).</summary>
    public DevelopmentType? DevelopmentType { get; init; }

    /// <summary>Bu sorgu bir adet/sayı (Count) sorgusu mu? (örn: "Kangal'da kaç aktif proje var?")</summary>
    public bool IsCountOnly { get; init; } = false;

    /// <summary>Hibrit sorgu için anlamsal metin (örn: "Kangal sahasında kestirimci bakım projeleri" -> Lokasyon=Kangal, Anlamsal=kestirimci bakım).</summary>
    public string? SemanticTopic { get; init; }

    /// <summary>Önceki sonuç kümesi üzerinde filtreleme yapan takip sorgusu mu? (örn: "Bunlardan Kangal'da olanlar").</summary>
    public bool IsFollowUpFilter { get; init; } = false;

    /// <summary>Kullanıcı adet düzeltme sorgusu mu? (örn: "5 dedim ama 3 tane getirdin").</summary>
    public bool IsCorrection { get; init; } = false;
}
