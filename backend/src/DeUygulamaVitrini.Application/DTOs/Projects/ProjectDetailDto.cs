using DeUygulamaVitrini.Application.DTOs.Lookups;

namespace DeUygulamaVitrini.Application.DTOs.Projects;

/// <summary>
/// Proje detay sayfası için tüm bilgileri ve ilişkili alt modelleri içeren kapsamlı DTO.
/// </summary>
public class ProjectDetailDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }

    public required string ShortDescription { get; set; }
    public string? Description { get; set; }

    public string? Purpose { get; set; }
    public string? ProblemSolved { get; set; }

    public string? NonTechnicalDescription { get; set; }
    public string? TechnicalDescription { get; set; }
    public string? BusinessImpact { get; set; }
    public string? TargetAudience { get; set; }
    public string? AccessInstructions { get; set; }
    public string? ApplicationUrl { get; set; }
    public string? RepositoryUrl { get; set; }

    public required string DevelopmentType { get; set; }

    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    public bool IsFeatured { get; set; }
    public string? CoverImageUrl { get; set; }

    public required ProjectStatusDto Status { get; set; }
    public required ProjectCategoryDto Category { get; set; }

    public List<TeamDto> Teams { get; set; } = new();
    public List<MemberDto> Members { get; set; } = new();
    public List<LocationDto> Locations { get; set; } = new();
    public List<TechnologyDto> Technologies { get; set; } = new();
    public List<TagDto> Tags { get; set; } = new();

    public List<ProjectIntegrationDto> Integrations { get; set; } = new();
    public List<ProjectMediaDto> Media { get; set; } = new();
    public List<ProjectDocumentDto> Documents { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
