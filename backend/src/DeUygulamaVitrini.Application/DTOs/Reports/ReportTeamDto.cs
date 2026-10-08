namespace DeUygulamaVitrini.Application.DTOs.Reports;

/// <summary>
/// Raporlama için ekip proje katılım bilgisi.
/// Performans/skor içermez; yalın katılım sayısı sunar.
/// </summary>
public class ReportTeamDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string DepartmentName { get; set; }
    public int ProjectCount { get; set; }
    public int PrimaryProjectCount { get; set; }
}
