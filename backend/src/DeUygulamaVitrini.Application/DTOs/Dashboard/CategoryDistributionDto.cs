namespace DeUygulamaVitrini.Application.DTOs.Dashboard;

/// <summary>
/// Dashboard proje kategori dağılımı verisi.
/// </summary>
public class CategoryDistributionDto
{
    public required int CategoryId { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; }
    public required int Count { get; set; }
}
