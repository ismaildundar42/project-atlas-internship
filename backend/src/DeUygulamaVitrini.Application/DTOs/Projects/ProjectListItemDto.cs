using DeUygulamaVitrini.Application.DTOs.Lookups;

namespace DeUygulamaVitrini.Application.DTOs.Projects;

/// <summary>
/// Proje kartı ve liste görünümleri için optimize edilmiş hafif DTO.
/// Ağır detay ve doküman alanlarını içermez.
/// </summary>
public class ProjectListItemDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public required string ShortDescription { get; set; }

    public required ProjectStatusDto Status { get; set; }
    public required ProjectCategoryDto Category { get; set; }

    public required string DevelopmentType { get; set; }
    public bool IsFeatured { get; set; }

    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    public TeamDto? PrimaryTeam { get; set; }
    public List<LocationDto> Locations { get; set; } = new();
    public List<TechnologyDto> Technologies { get; set; } = new();
    public List<TagDto> Tags { get; set; } = new();

    /// <summary>Projenin kapak görseli URL'si (ProjectMedia içerisindeki ilk resim).</summary>
    public string? CoverImageUrl { get; set; }
}
