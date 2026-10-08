namespace DeUygulamaVitrini.Application.DTOs.Reports;

/// <summary>
/// Raporlama için zaman çizelgesi aylık proje başlangıç dağılımı.
/// </summary>
public class ReportTimelineDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public required string PeriodLabel { get; set; }
    public int ProjectCount { get; set; }
}
