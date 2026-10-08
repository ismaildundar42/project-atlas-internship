using DeUygulamaVitrini.Application.DTOs.Lookups;

namespace DeUygulamaVitrini.Application.DTOs.Dashboard;

/// <summary>
/// Dashboard öne çıkan proje kartı verisi.
/// </summary>
public class FeaturedProjectDto
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public required string ShortDescription { get; set; }
    public required ProjectStatusDto Status { get; set; }
    public required ProjectCategoryDto Category { get; set; }
    public IReadOnlyList<string> Technologies { get; set; } = Array.Empty<string>();
    public string? CoverImageUrl { get; set; }
}
