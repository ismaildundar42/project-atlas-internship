using DeUygulamaVitrini.Application.DTOs.Dashboard;

namespace DeUygulamaVitrini.Application.Common.Interfaces;

/// <summary>
/// Dashboard özet ve portföy metrikleri için servis arayüzü.
/// Read-only ve amaç odaklı dashboard verileri sunar.
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Yayınlanmış projeler için dashboard özet metriklerini getirir.
    /// </summary>
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken = default);
}
