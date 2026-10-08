using DeUygulamaVitrini.Application.DTOs.Reports;

namespace DeUygulamaVitrini.Application.Common.Interfaces;

/// <summary>
/// Proje portföyü raporlama ve içgörü metriklerini üreten servis arayüzü.
/// Yalnızca yayınlanmış (IsPublished = true) ve silinmemiş (IsDeleted = false) projeleri kapsar.
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Portföy geneli için tüm analitik ve dağılım raporlarını üretir.
    /// </summary>
    Task<ReportOverviewDto> GetPortfolioOverviewAsync(CancellationToken cancellationToken = default);
}
