namespace DeUygulamaVitrini.Application.DTOs.Dashboard;

/// <summary>
/// Dashboard proje durum dağılımı verisi.
/// </summary>
public class StatusDistributionDto
{
    public required int StatusId { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; }
    public required int Count { get; set; }
}
