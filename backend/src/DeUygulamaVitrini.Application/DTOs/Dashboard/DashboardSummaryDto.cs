namespace DeUygulamaVitrini.Application.DTOs.Dashboard;

/// <summary>
/// Dashboard özet API yanıt modeli.
/// GET /api/dashboard/summary çağrısı bu DTO'yu döner.
/// </summary>
public class DashboardSummaryDto
{
    // ─── KPI Sayıları ────────────────────────────────────────────────────────
    public required int TotalProjects { get; set; }
    public required int ActiveProjects { get; set; }
    public required int InProgressProjects { get; set; }
    public required int FeaturedProjectsCount { get; set; }

    // ─── Dağılımlar & Listeler ────────────────────────────────────────────────
    public IReadOnlyList<StatusDistributionDto> StatusDistribution { get; set; } = Array.Empty<StatusDistributionDto>();
    public IReadOnlyList<CategoryDistributionDto> CategoryDistribution { get; set; } = Array.Empty<CategoryDistributionDto>();
    public IReadOnlyList<RecentProjectDto> RecentProjects { get; set; } = Array.Empty<RecentProjectDto>();
    public IReadOnlyList<FeaturedProjectDto> FeaturedProjects { get; set; } = Array.Empty<FeaturedProjectDto>();
    public IReadOnlyList<TopTechnologyDto> TopTechnologies { get; set; } = Array.Empty<TopTechnologyDto>();
}
