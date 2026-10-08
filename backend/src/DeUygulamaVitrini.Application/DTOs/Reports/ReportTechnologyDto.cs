namespace DeUygulamaVitrini.Application.DTOs.Reports;

/// <summary>
/// Raporlama için teknoloji kullanım bilgisi.
/// </summary>
public class ReportTechnologyDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Category { get; set; }
    public int ProjectCount { get; set; }
}
