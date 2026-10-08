namespace DeUygulamaVitrini.Application.DTOs.Reports;

/// <summary>
/// Raporlama KPI özet metrikleri.
/// Dashboard ile aynı semantik kurallara ve tanımlara sahiptir.
/// </summary>
public class ReportSummaryDto
{
    /// <summary>Yayınlanmış toplam proje sayısı</summary>
    public int TotalProjects { get; set; }

    /// <summary>Aktif durumdaki (ACTIVE) proje sayısı</summary>
    public int ActiveProjects { get; set; }

    /// <summary>Devam eden (PLANNING, PROOF_OF_CONCEPT, PILOT, ACTIVE_DEVELOPMENT) proje sayısı</summary>
    public int InProgressProjects { get; set; }

    /// <summary>Tamamlanmış (COMPLETED) proje sayısı</summary>
    public int CompletedProjects { get; set; }

    /// <summary>Öne çıkarılmış (IsFeatured) proje sayısı</summary>
    public int FeaturedProjects { get; set; }
}
