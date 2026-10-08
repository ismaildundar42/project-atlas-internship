namespace DeUygulamaVitrini.Application.DTOs.Reports;

/// <summary>
/// Portföy raporlama genel bakış yanıt modeli.
/// GET /api/reports/overview kontratıdır.
/// </summary>
public class ReportOverviewDto
{
    public ReportSummaryDto Summary { get; set; } = new();
    public List<ReportStatusDistributionDto> StatusDistribution { get; set; } = new();
    public List<ReportCategoryDistributionDto> CategoryDistribution { get; set; } = new();
    public List<ReportDevelopmentTypeDistributionDto> DevelopmentTypeDistribution { get; set; } = new();
    public List<ReportTechnologyDto> TopTechnologies { get; set; } = new();
    public List<ReportLocationDto> LocationDistribution { get; set; } = new();
    public List<ReportTeamDto> TeamDistribution { get; set; } = new();
    public List<ReportTimelineDto> ProjectTimeline { get; set; } = new();
}
