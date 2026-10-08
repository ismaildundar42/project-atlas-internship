namespace DeUygulamaVitrini.Application.DTOs.Reports;

/// <summary>
/// Raporlama için lokasyon dağılım bilgisi.
/// </summary>
public class ReportLocationDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }
    public int ProjectCount { get; set; }
}
