using DeUygulamaVitrini.Application.DTOs.Lookups;

namespace DeUygulamaVitrini.Application.DTOs.Dashboard;

/// <summary>
/// Dashboard son eklenen/güncellenen proje özet verisi.
/// </summary>
public class RecentProjectDto
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public required string ShortDescription { get; set; }
    public required ProjectStatusDto Status { get; set; }
    public required ProjectCategoryDto Category { get; set; }
    public required DateTime UpdatedAt { get; set; }
    public string? CoverImageUrl { get; set; }
}
