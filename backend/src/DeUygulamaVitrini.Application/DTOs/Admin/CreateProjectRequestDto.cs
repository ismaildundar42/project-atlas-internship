using System.ComponentModel.DataAnnotations;

namespace DeUygulamaVitrini.Application.DTOs.Admin;

public class ProjectTeamRequestDto
{
    public int TeamId { get; set; }
    public bool IsPrimary { get; set; }
}

public class ProjectMemberRequestDto
{
    public int MemberId { get; set; }
    public string? ProjectRole { get; set; }
}

public class ProjectIntegrationRequestDto
{
    [Required(ErrorMessage = "Entegrasyon adı zorunludur.")]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string IntegrationType { get; set; } = "REST_API";
}

public class ProjectDocumentRequestDto
{
    [Required(ErrorMessage = "Doküman adı zorunludur.")]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Required(ErrorMessage = "Dosya adı zorunludur.")]
    public string FileName { get; set; } = string.Empty;
    [Required(ErrorMessage = "Dosya URL adresi zorunludur.")]
    public string FileUrl { get; set; } = string.Empty;
    public string? DocumentType { get; set; }
}

public class ProjectMediaRequestDto
{
    public string MediaType { get; set; } = "Image";
    [Required(ErrorMessage = "Dosya adı zorunludur.")]
    public string FileName { get; set; } = string.Empty;
    [Required(ErrorMessage = "Medya URL adresi zorunludur.")]
    public string FileUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public string? Caption { get; set; }
    public int DisplayOrder { get; set; } = 0;
}

public class CreateProjectRequestDto
{
    [Required(ErrorMessage = "Proje adı zorunludur.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Slug adresi zorunludur.")]
    public string Slug { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kısa açıklama zorunludur.")]
    public string ShortDescription { get; set; } = string.Empty;

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

    [Required(ErrorMessage = "Proje durumu zorunludur.")]
    public int StatusId { get; set; }

    [Required(ErrorMessage = "Proje kategorisi zorunludur.")]
    public int CategoryId { get; set; }

    public string DevelopmentType { get; set; } = "Internal";

    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    public bool IsPublished { get; set; } = false;
    public bool IsFeatured { get; set; } = false;
    public string? CoverImageUrl { get; set; }

    public List<ProjectTeamRequestDto> Teams { get; set; } = new();
    public List<ProjectMemberRequestDto> Members { get; set; } = new();
    public List<int> LocationIds { get; set; } = new();
    public List<int> TechnologyIds { get; set; } = new();
    public List<int> TagIds { get; set; } = new();

    public List<ProjectIntegrationRequestDto> Integrations { get; set; } = new();
    public List<ProjectDocumentRequestDto> Documents { get; set; } = new();
    public List<ProjectMediaRequestDto> MediaItems { get; set; } = new();
}

public class UpdateProjectRequestDto : CreateProjectRequestDto
{
}

public class SetPublishedRequestDto
{
    public bool IsPublished { get; set; }
}

